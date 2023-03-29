using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Storage.Entities;

public record ConversationEntity(string partitionKey, string id, string lastModified, List<string> participants);