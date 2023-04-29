using System.Net;
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
        var existingProfile1 = await _profileStore.GetProfile(conversation.Participants[0]);
        var existingProfile2 = await _profileStore.GetProfile(conversation.Participants[1]);
        var conversations = (await _conversationStore.GetConversations(conversation.Participants[0], null, null, "0")).conversations;

        if (existingProfile1 == null || existingProfile2 == null)
        {
            return NotFound($"A user with username {conversation.Participants[0]}" +
                            $" or {conversation.Participants[1]} doesn't exist");
        }
        
        if (conversation.FirstMessage.text.Length == 0 ||
            conversation.Participants.Length != 2)
        {
            return BadRequest("Invalid message, please try again.");
        }
        
        foreach (var UserConversations in conversations)
        {
            if ((UserConversations.participants[0] == conversation.Participants[0] &&
                 UserConversations.participants[1] == conversation.Participants[1]) ||
                (UserConversations.participants[0] == conversation.Participants[1] &&
                 UserConversations.participants[1] == conversation.Participants[0]))
                return Conflict($"A conversation between {existingProfile1.username} " +
                                $"and {existingProfile2.username} already exists.");
        }

        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var conversationId = Guid.NewGuid();
        string messageid;
        var message = new Message(
            conversation.FirstMessage.messageId,
            conversationId,
            conversation.FirstMessage.senderUsername,
            conversation.FirstMessage.text,
            time
        );
        
        var conversationdb = new Conversation(
            conversationId.ToString(),
            time,
            conversation.Participants
        );
        var conversationresponse = new ConversationResponse(
            conversationId.ToString(),
            time
        );
        await _conversationStore.AddConversation(conversationdb);
        await _messageStore.AddMessage(message);

        return CreatedAtAction(nameof(GetConversations), new { username = conversation.Participants[0] },
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

        var existingConversation = (await _conversationStore.GetConversations(message.senderUsername, null, null, "0")).conversations;
        if (existingConversation == null)
        {
            return Conflict($"A Conversation with conversationId {conversationId.ToString()} doesn't exist");
        }
        
        if (message.text.Length == 0)
        {
            return BadRequest("Invalid message, please try again.");
        }

        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messageDb = new Message(
            message.messageId,
            conversationId,
            message.senderUsername,
            message.text,
            time
        );

        var messageresponse = new SendMessageResponse(
            time
        );
        await _messageStore.AddMessage(messageDb);
        return CreatedAtAction(nameof(GetConversations), new { username = message.senderUsername },
            messageresponse);
    }
    
    [HttpGet]
    public async Task<ActionResult<ConversationResponse>> GetConversations([FromQuery] string username,
        [FromQuery] int? limit, [FromQuery] string? continuationtoken, [FromQuery] long lastSeenConversationTime)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null) return NotFound($"There is no profile with username: {username}");
        var getConversationResponse = await _conversationStore.GetConversations(username, limit, continuationtoken, lastSeenConversationTime.ToString());
        var conversations = getConversationResponse.conversations;
        var conversationResponseList = new List<ListConversationsResponseItem>();
        string participant;
        foreach (var conversation in conversations)
        {
            if (conversation.participants[0] == username) participant = conversation.participants[1];
            else participant = conversation.participants[0];
            conversationResponseList.Add(new ListConversationsResponseItem
                (conversation.conversationId,
                    conversation.lastModified,
                    await _profileStore.GetProfile(participant)
                    )
            );
        }
        var nextUri = $"/api/conversations?username={username}";
        if (limit != null) nextUri += $"&limit={limit}";
        if (getConversationResponse.continuationToken != null)
        {
            continuationtoken = WebUtility.UrlEncode(getConversationResponse.continuationToken);
            nextUri += $"&continuationToken={continuationtoken}";
        }
        if (lastSeenConversationTime > 0) nextUri += $"&lastSeenConversationTime={lastSeenConversationTime}";
        var conversationResponse = new GetConversationResponse(conversationResponseList, nextUri);
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
