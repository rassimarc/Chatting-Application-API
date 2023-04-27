using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Conversation(
    string conversationId,
    long lastModified,
    string[] participants
);