using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IConversationStore
{
    Task UpsertConversation(ConversationRequest conversation);
    Task<ConversationRequest?> GetConversation(string conversationId);
}