using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IMessageStore
{
    Task UpsertMessage(Message message);
    Task<(List<Message> messages, string? continuationToken)> GetMessages(int? pageSize, string? continuationToken,
        string conversationId, string lastSeenMessageTime);
    
    Task DeleteMessage(string messageId, string conversationId);

}