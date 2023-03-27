using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record ConversationRequest(
    List<string> participants,
    Message firstMessage
);