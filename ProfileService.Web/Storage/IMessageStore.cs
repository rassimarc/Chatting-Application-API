using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IMessageStore
{
    Task UpsertMessage(SendMessageRequest message);
    Task<SendMessageResponse?> GetMessage(string messageId);
    
    Task DeleteMessage(string messageId);

}