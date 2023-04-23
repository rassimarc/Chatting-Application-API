using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IConversationService
{
    MessageResponse GetMessages(List<Message> messages, string? continuationToken);
}