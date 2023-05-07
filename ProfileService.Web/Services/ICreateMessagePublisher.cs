using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface ICreateMessagePublisher
{
    Task Send(SendMessageServiceBus message);
}