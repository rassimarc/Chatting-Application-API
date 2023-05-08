using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;
    private readonly IImageStore _imageStore;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger, IImageStore imageStore)
    {
        _profileService = profileService;
        _logger = logger;
        _imageStore = imageStore;
    }
        
    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await _profileService.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }
            
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        using (_logger.BeginScope("{Username}", profile.username))
        {
            var existingProfile = await _profileService.GetProfile(profile.username);
            if (existingProfile != null)
            {
                return Conflict($"A user with username {profile.username} already exists");
            }
            _logger.LogInformation("Creating Profile for user {ProfileUsername}", profile.username);
            await _profileService.EnqueueCreateProfile(profile);

            return CreatedAtAction(nameof(GetProfile), new { username = profile.username },
                profile);
        }
    }

    [HttpPut("{username}")]
    public async Task<ActionResult<Profile>> UpdateProfile(string username, PutProfileRequest request)
    {
        var existingProfile = await _profileService.GetProfile(username);
        if (existingProfile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }

        if (!string.IsNullOrWhiteSpace(request.ProfilePictureId))
        {
            var existingImage = await _imageStore.GetImage(request.ProfilePictureId);
    
            if (existingImage == null)
            {
                return NotFound("The image you are trying to upload cannot be found. Please try uploading it again.");
            }
        }
        
        var profile = new Profile(username, request.firstName, request.lastName, request.ProfilePictureId.ToString());
        await _profileService.UpdateProfile(profile);

        _logger.LogInformation("Updated Profile for {Username}", profile.username);
        return Ok(profile);
    }
}