using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IConversationStore
{
    Task UploadConversation(ConversationRequest conversation);
    Task<ConversationRequest?> GetConversation(string conversationId);
}