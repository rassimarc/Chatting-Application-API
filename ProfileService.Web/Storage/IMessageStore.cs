using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IMessageStore
{
    Task UpsertMessage(Message message);
    Task<Message?> GetMessage(string messageId, string conversationId);
    
    Task DeleteMessage(string messageId, string conversationId);

}