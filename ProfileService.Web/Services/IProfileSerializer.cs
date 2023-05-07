using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IProfileSerializer
{
    string SerializeProfile(Profile profile);
    Profile DeserializeProfile(string serialized);
}