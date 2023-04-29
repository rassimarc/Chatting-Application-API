using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IConversationStore
{
    Task AddConversation(Conversation conversation);
    Task<(List<Conversation> conversations, string? continuationToken)> GetConversations(string participant,
        int? pageSize,
        string? continuationToken, string lastSeenMessageTime);
    
    Task DeleteConversation(string participant, string conversationId);

}