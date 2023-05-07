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
    private readonly IConversationService _conversationService;

    public ConversationController(IProfileStore profileStore, IConversationStore conversationStore, IMessageStore messageStore, IConversationService conversationService)
    {
        _profileStore = profileStore;
        _conversationStore = conversationStore;
        _messageStore = messageStore;
        _conversationService = conversationService;
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> AddConversation(ConversationRequest conversation)
    {
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
        
       

        //var conversationResponse = await _conversationService.AddConversation(conversation);
        var conversationResponse =  await _conversationService.AddConversation(conversation);
        return CreatedAtAction(nameof(GetConversations), new { username = conversation.Participants[0] },
            conversationResponse);
    }

    [HttpPost("{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse>> AddMessage(SendMessageRequest message, string conversationId)
    {
        var existingProfile = await _profileStore.GetProfile(message.SenderUsername);
        var existingConversation = await _conversationStore.GetConversation(message.SenderUsername, conversationId);
        var existingMessages = (await _messageStore.GetMessages(null, null, conversationId, "0")).messages;

        if (existingProfile == null) return NotFound($"A user with username {message.SenderUsername} doesn't exist");
        if (existingConversation == null) return Conflict($"A Conversation with conversationId {conversationId} doesn't exist");
        if (message.Text.Length == 0) return BadRequest("Invalid message, please try again.");
        foreach (var messageId in existingMessages)
        {
            if (messageId.messageId == message.Id) return Conflict($"A message with Id = {message.Id} already exists");
        }

        var messageresponse = await _conversationService.AddMessage(message, conversationId, existingConversation);
        return CreatedAtAction(nameof(GetConversations), new { username = message.SenderUsername },
            messageresponse);
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
