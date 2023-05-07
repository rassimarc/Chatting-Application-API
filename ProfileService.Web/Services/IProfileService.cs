using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IProfileService
{
    Task EnqueueCreateProfile(Profile profile);
    Task CreateProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task UpdateProfile(Profile profile);
}