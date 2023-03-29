using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IConversationStore
{
    Task UpsertConversation(Conversation conversation);
    Task<Conversation?> GetConversation(string participant, string conversationId);
    
    Task DeleteConversation(string participant, string conversationId);

}