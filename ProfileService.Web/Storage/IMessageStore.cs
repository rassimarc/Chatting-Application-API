using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IMessageStore
{
    Task UploadMessage(SendMessageRequest message);
    Task<SendMessageRequest?> GetMessage(string messageId);
}