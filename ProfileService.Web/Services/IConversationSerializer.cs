using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IConversationSerializer
{
    string SerializeConversation(ConversationRequest conversation);
    ConversationRequest DeserializeConversation(string serialized);
}