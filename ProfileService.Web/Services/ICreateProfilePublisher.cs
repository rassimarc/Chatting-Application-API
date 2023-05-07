using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface ICreateProfilePublisher
{
    Task Send(Profile profile);
}