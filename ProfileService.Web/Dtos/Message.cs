using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Message(
    Guid messageId,
    string senderUsername,
    string text,
    UnixDateTime time
);