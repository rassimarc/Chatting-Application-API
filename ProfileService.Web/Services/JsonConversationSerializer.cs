using Newtonsoft.Json;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class JsonConversationSerializer : IConversationSerializer
{
    public string SerializeConversation(ConversationRequest conversation)
    {
        return JsonConvert.SerializeObject(conversation);
    }

    public ConversationRequest DeserializeConversation(string serialized)
    {
        return JsonConvert.DeserializeObject<ConversationRequest>(serialized);
    }
}