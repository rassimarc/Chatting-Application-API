using System.Web;
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
        var existingProfile1 = await _profileStore.GetProfile(conversation.participants[0]);
        var existingProfile2 = await _profileStore.GetProfile(conversation.participants[1]);
        var conversations = await _conversationStore.GetConversations(conversation.participants[0]);

        if (existingProfile1 == null || existingProfile2 == null)
        {
            return NotFound($"A user with username {conversation.participants[0]}" +
                            $" or {conversation.participants[1]} doesn't exist");
        }
        
        if (conversation.firstMessage.text.Length == 0 ||
            conversation.participants.Count != 2)
        {
            return BadRequest("Invalid message, please try again.");
        }
        
        foreach (var UserConversations in conversations)
        {
            if ((UserConversations.participants[0] == conversation.participants[0] &&
                 UserConversations.participants[1] == conversation.participants[1]) ||
                (UserConversations.participants[0] == conversation.participants[1] &&
                 UserConversations.participants[1] == conversation.participants[0]))
                return Conflict($"A conversation between {existingProfile1.username} " +
                                $"and {existingProfile2.username} already exists.");
        }

        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var conversationId = Guid.NewGuid();
        
        var message = new Message(
            conversation.firstMessage.messageId,
            conversationId,
            conversation.firstMessage.senderUsername,
            conversation.firstMessage.text,
            time
        );
        
        var conversationdb = new Conversation(
            conversationId,
            time,
            conversation.participants
        );
        var conversationresponse = new ConversationResponse(
            conversationId,
            time
        );
        await _conversationStore.UpsertConversation(conversationdb);
        await _messageStore.UpsertMessage(message);

        return CreatedAtAction(nameof(GetConversations), new { username = conversation.participants[0] },
            conversationresponse);
    }
    
    [HttpPost("{conversationId}")]
    public async Task<ActionResult<ConversationResponse>> AddMessage(SendMessageRequest message, Guid conversationId)
    {
        var existingProfile = await _profileStore.GetProfile(message.senderUsername);

        if (existingProfile == null)
        {
            return NotFound($"A user with username {message.senderUsername} doesn't exist");
        }

        var existingConversation = await _conversationStore.GetConversations(message.senderUsername);
        if (existingConversation == null)
        {
            return Conflict($"A Conversation with conversationId {conversationId.ToString()} doesn't exist");
        }
        
        if (message.text.Length == 0)
        {
            return BadRequest("Invalid message, please try again.");
        }

        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messageDB = new Message(
            message.messageId,
            conversationId,
            message.senderUsername,
            message.text,
            time
        );

        var messageresponse = new SendMessageResponse(
            time
        );
        await _messageStore.UpsertMessage(messageDB);
        return CreatedAtAction(nameof(GetConversations), new { username = message.senderUsername },
            messageresponse);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Conversation>>> GetConversations([FromQuery] string username)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null) return NotFound($"There is no profile with username: {username}");
        var conversations = await _conversationStore.GetConversations(username);
        return Ok(conversations);
    }


    [HttpGet("{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse?>> GetMessages(string conversationId,
        [FromQuery] int limit = 50, [FromQuery] string? continuationtoken = null, [FromQuery] long lastSeenMessageTime = 0)
    {
        var decodedContinuationToken = HttpUtility.UrlDecode(continuationtoken);
        var messagesToken = await _messageStore.GetMessages(limit , decodedContinuationToken, conversationId, lastSeenMessageTime);
        if (messagesToken.messages.Count == 0) return NotFound($"There is no conversation with Conversation ID = {conversationId}");
        var messageResponse = _conversationService.GetMessages(messagesToken.messages, messagesToken.continuationToken, conversationId, limit);
        return Ok(messageResponse);
    }
}
