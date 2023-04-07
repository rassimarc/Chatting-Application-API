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
        
        if (existingProfile1 == null || existingProfile2 ==null)
        {
            return Conflict($"A user with username {conversation.participants[0]}" +
                            $" or {conversation.participants[1]} doesn't exist");
        }

        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Guid conversationId = new Guid();
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
        /*
         TODO: Create a database and store conversation and message in the database (Give Priority)
         TODO: Finish conversation DTO
         TODO: Add more tests to validate the conversation and messages
         */
        return CreatedAtAction(nameof(GetConversation), new { conversationId = conversationresponse.conversationId },
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
        /*
         TODO: Message database
         TODO: Correct return message
         */
        return CreatedAtAction(nameof(GetConversation), new { conversationId = conversationId },
            messageresponse);
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetConversation(string username)
    {
        var profile = await _profileStore.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }
            
        return Ok(profile);
    }

    [HttpPut("{username}")]
    public async Task<ActionResult<Profile>> UpdateProfile(string username, PutProfileRequest request)
    {
        var existingProfile = await _profileStore.GetProfile(username);
        if (existingProfile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        var existingImage = await _imageStore.GetImage(request.ProfilePictureId.ToString());

        if (existingImage == null)
        {
            return NotFound("The image you are trying to upload cannot be found. Please try uploading it again.");
        }
        
        var profile = new Profile(username, request.firstName, request.lastName, request.ProfilePictureId);
        await _profileStore.UpsertProfile(profile);
        return Ok(profile);
    }
}
