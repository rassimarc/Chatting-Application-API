using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Conversation(
    string conversationId,
    UnixDateTime lastModified,
    List<string> participants
);