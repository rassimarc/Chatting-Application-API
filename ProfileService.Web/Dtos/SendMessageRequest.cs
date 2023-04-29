using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Newtonsoft.Json;

namespace ProfileService.Web.Dtos;

public record SendMessageRequest(
    [Required][JsonProperty("SenderUsername")] string SenderUsername, 
    [Required][JsonProperty("Text")] string Text,
    [JsonProperty("Id")]string? Id
);