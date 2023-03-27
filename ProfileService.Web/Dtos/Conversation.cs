using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Conversation(
    Guid conversationId,
    Profile participant1,
    Profile participant,
    UnixDateTime modifiedTime
);