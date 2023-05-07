using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IMessageSerializer
{
    string SerializeMessage(SendMessageServiceBus message);
    SendMessageServiceBus DeserializeMessage(string serialized);
}