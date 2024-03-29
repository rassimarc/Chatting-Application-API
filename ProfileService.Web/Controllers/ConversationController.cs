using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;
namespace ProfileService.Web.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IProfileStore _profileStore;
    private readonly IConversationStore _conversationStore;
    private readonly IMessageStore _messageStore;
    private readonly ILogger<ConversationController> _logger;
    private readonly IConversationService _conversationService;

    public ConversationController(IProfileStore profileStore, ILogger<ConversationController> logger, IConversationStore conversationStore, IMessageStore messageStore, IConversationService conversationService)
    {
        _profileStore = profileStore;
        _conversationStore = conversationStore;
        _messageStore = messageStore;
        _conversationService = conversationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> AddConversation(ConversationRequest conversation)
    {
        using (_logger.BeginScope("{participants}", conversation.Participants))
        {
            if (conversation.Participants.Length != 2) return BadRequest("There should be 2 participants");
            var existingProfile1 = await _profileStore.GetProfile(conversation.Participants[0]);
            var existingProfile2 = await _profileStore.GetProfile(conversation.Participants[1]);
            var conversations = (await _conversationStore.GetConversations(conversation.Participants[0], null, null, "0")).conversations;

            if (existingProfile1 == null || existingProfile2 == null) return NotFound($"A user with username {conversation.Participants[0]} or {conversation.Participants[1]} doesn't exist");
            if (conversation.FirstMessage.Text.Length == 0 || conversation.Participants.Length != 2) return BadRequest("Invalid message, please try again.");
            foreach (var userConversations in conversations)
            {
                if ((userConversations.participants[0] == conversation.Participants[0] &&
                     userConversations.participants[1] == conversation.Participants[1]) ||
                    (userConversations.participants[0] == conversation.Participants[1] &&
                     userConversations.participants[1] == conversation.Participants[0]))
                    return Conflict($"A conversation between {existingProfile1.username} " +
                                    $"and {existingProfile2.username} already exists.");
            }
        
            _logger.LogInformation("Creating Conversation for user {participants}", conversation.Participants);

            var conversationResponse =  await _conversationService.AddConversation(conversation);
            return CreatedAtAction(nameof(GetConversations), new { username = conversation.Participants[0] },
                conversationResponse);
        }

    }

    [HttpPost("{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse>> AddMessage(SendMessageRequest message, string conversationId)
    {
        using (_logger.BeginScope("{message}", message.SenderUsername))
        {
            var existingProfile = await _profileStore.GetProfile(message.SenderUsername);
            var existingConversation = await _conversationStore.GetConversation(message.SenderUsername, conversationId);
            var existingMessages = (await _messageStore.GetMessages(null, null, conversationId, "0")).messages;

            if (existingProfile == null) return NotFound($"A user with username {message.SenderUsername} doesn't exist");
            if (existingConversation == null) return NotFound($"A Conversation with conversationId {conversationId} doesn't exist");
            if (message.Text.Length == 0) return BadRequest("Invalid message, please try again.");
            foreach (var messageId in existingMessages)
            {
                if (messageId.messageId == message.Id) return Conflict($"A message with Id = {message.Id} already exists");
            }

            //var messageresponse = await _conversationService.AddMessage(message, conversationId, existingConversation);
            var messageservicebus = new SendMessageServiceBus(message, conversationId, existingConversation);
            _logger.LogInformation("Creating message for {conversation}", conversationId);
            var messageresponse =   await _conversationService.AddMessageServiceBus(messageservicebus);


            return CreatedAtAction(nameof(GetConversations), new { username = message.SenderUsername },
                messageresponse);
        }

    }
    
    [HttpGet]
    public async Task<ActionResult<GetConversationResponse>> GetConversations([FromQuery] string username,
        [FromQuery] int? limit, [FromQuery] string? continuationtoken, [FromQuery] long lastSeenConversationTime)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null) return NotFound($"There is no profile with username: {username}");
        var getConversationResponse = await _conversationStore.GetConversations(username, limit, continuationtoken, lastSeenConversationTime.ToString());
        var conversations = getConversationResponse.conversations;
        var conversationResponse = await _conversationService.GetConversations(conversations, username, limit,
            getConversationResponse.continuationToken, lastSeenConversationTime);
        return Ok(conversationResponse);
    }

    [HttpGet("{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse?>> GetMessages(string conversationId,
        [FromQuery] int? limit, [FromQuery] string? continuationtoken, [FromQuery] long lastSeenMessageTime)
    {
        var messagesToken = await _messageStore.GetMessages(limit , continuationtoken, conversationId, lastSeenMessageTime.ToString());
        if (messagesToken.messages.Count == 0) return NotFound($"There is no conversation with Conversation ID = {conversationId}");
        var messageResponse = _conversationService.GetMessages(messagesToken.messages, messagesToken.continuationToken, conversationId, limit, lastSeenMessageTime);
        return Ok(messageResponse);
    }
}
