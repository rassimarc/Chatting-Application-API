using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Services;

public class ProfileService : IProfileService
{
    private readonly ICreateProfilePublisher _createProfilePublisher;
    private readonly IProfileStore _profileStore;

    public ProfileService(
        ICreateProfilePublisher createProfilePublisher,
        IProfileStore profileStore)
    {
        _createProfilePublisher = createProfilePublisher;
        _profileStore = profileStore;
    }

    public async Task EnqueueCreateProfile(Profile profile)
    {
        await _createProfilePublisher.Send(profile);
    }

    public Task CreateProfile(Profile profile)
    {
        return _profileStore.AddProfile(profile);
    }

    public Task<Profile?> GetProfile(string username)
    {
        return _profileStore.GetProfile(username);
    }

    public Task UpdateProfile(Profile profile)
    {
        return _profileStore.UpsertProfile(profile);
    }
}