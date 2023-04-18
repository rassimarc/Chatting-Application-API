using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class ConversationController : ControllerBase
{
    private readonly IProfileStore _profileStore;
    private readonly IImageStore _imageStore;
    private readonly IConversationStore _conversationStore;
    private readonly IMessageStore _messageStore;

    public ConversationController(IProfileStore profileStore, IImageStore imageStore, IConversationStore conversationStore, IMessageStore messageStore)
    {
        _profileStore = profileStore;
        _imageStore = imageStore;
        _conversationStore = conversationStore;
        _messageStore = messageStore;
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> AddConversation(ConversationRequest conversation)
    {
        var existingProfile1 = await _profileStore.GetProfile(conversation.participants[0]);
        var existingProfile2 = await _profileStore.GetProfile(conversation.participants[1]);
        
        if (existingProfile1 == null || existingProfile2 == null)
        {
            return Conflict($"A user with username {conversation.participants[0]}" +
                            $" or {conversation.participants[1]} doesn't exist");
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
            return Conflict($"A user with username {message.senderUsername} doesn't exist");
        }

        var existingConversation = await _conversationStore.GetConversations(message.senderUsername);
        if (existingConversation == null)
        {
            return Conflict($"A Conversation with conversationId {conversationId} doesn't exist");
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
        /*
         TODO: Correct return message
         */
        return CreatedAtAction(nameof(GetConversations), new { conversationIduser = conversationId },
            messageresponse);
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<List<Conversation>>> GetConversations(string username)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null) return NotFound($"There is no profile with username: {username}");
        var conversations = await _conversationStore.GetConversations(username);
        /*
         TODO: Add more tests to verify
         */
        return Ok(conversations);
    }


    [HttpGet("{conversationId}")]
    public async Task<ActionResult<List<Conversation>?>> GetMessages(string conversationId)
    {
        var messages = await _messageStore.GetMessages(conversationId);
        /*
         TODO: Add more tests to verify
         */
        return Ok(messages);
    }
}
