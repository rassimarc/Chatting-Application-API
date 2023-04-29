using Newtonsoft.Json;

namespace ProfileService.Web.Dtos;

public record ListConversationsResponseItem(
    [JsonProperty("Id")] string Id,
    [JsonProperty("LastModifiedUnixTime")] long LastModifiedUnixTime,
    [JsonProperty("Recipient")] Profile Recipient
    
    );