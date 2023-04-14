using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IConversationStore
{
    Task UpsertConversation(Conversation conversation);
    Task<List<Conversation>?> GetConversations(string participant);
    
    Task DeleteConversation(string participant, string conversationId);

}