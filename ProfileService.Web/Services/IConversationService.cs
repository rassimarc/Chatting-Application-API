using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IConversationService
{
    MessageResponse GetMessages(List<Message> messages, string? continuationToken, string conversationId, int limit);
    Task<ConversationResponse> GetConversations(List<Conversation> conversations, string? continuationToken,
        string username, int limit);

}