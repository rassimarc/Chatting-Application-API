using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Storage.Entities;

public record MessageEntity(string partitionKey, string id, string time, string text, string senderUsername);