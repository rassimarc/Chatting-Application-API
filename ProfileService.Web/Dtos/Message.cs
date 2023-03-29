using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Message(
    string messageId,
    string conversationId,
    string senderUsername,
    string text,
    UnixDateTime time
);