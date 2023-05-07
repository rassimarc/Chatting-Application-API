using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IProfileStore
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>>
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);
    Task AddProfile(Profile profile);
}