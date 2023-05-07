using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface ICreateConversationPublisher
{
    Task Send(ConversationRequest conversation);
}