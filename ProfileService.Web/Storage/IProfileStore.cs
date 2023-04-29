using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IProfileStore
{
    Task AddProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);
}
