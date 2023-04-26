using Newtonsoft.Json;

namespace ProfileService.Web.Dtos;

public record MessageResponse(
    [JsonProperty("Messages")] List<GetMessageResponse> Messages, string? NextUri);