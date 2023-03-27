using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<Profile>> UploadConversation(Profile profile)
    {
        var existingConversation = await _profileStore.GetProfile(profile.username);
        if (existingConversation != null)
        {
            return Conflict($"A user with username {profile.username} already exists");
        }
        var existingImage = await _imageStore.GetImage(profile.ProfilePictureId.ToString());
        
        if (existingImage == null)
        {
            Guid guid = new Guid("3f7c9a85-1825-499f-92fb-c46882afbea2");
            var profile1 = new Profile(
                profile.username,
                profile.firstName,
                profile.lastName,
                guid
            );
            await _profileStore.UpsertProfile(profile1);
            return NotFound("The image you are trying to put cannot be found," +
                            " please try uploading it again and update your profile. " +
                            "Default profile picture has been set.");
        }
        await _profileStore.UpsertProfile(profile);
        return CreatedAtAction(nameof(GetProfile), new { username = profile.username },
            profile);
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
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
