using Newtonsoft.Json;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class JsonMessageSerializer : IMessageSerializer
{
    public string SerializeMessage(SendMessageServiceBus message)
    {
        return JsonConvert.SerializeObject(message);
    }

    public SendMessageServiceBus DeserializeMessage(string serialized)
    {
        return JsonConvert.DeserializeObject<SendMessageServiceBus>(serialized);
    }
}