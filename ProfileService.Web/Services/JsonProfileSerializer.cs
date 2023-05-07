using Newtonsoft.Json;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class JsonProfileSerializer : IProfileSerializer
{
    public string SerializeProfile(Profile profile)
    {
        return JsonConvert.SerializeObject(profile);
    }

    public Profile DeserializeProfile(string serialized)
    {
        return JsonConvert.DeserializeObject<Profile>(serialized);
    }
}