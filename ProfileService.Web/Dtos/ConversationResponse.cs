using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record ConversationResponse(
    string Id,
    long CreatedUnixTime
);