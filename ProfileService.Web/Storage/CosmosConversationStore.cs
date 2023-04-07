using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage.Entities;

namespace ProfileService.Web.Storage;
public class CosmosConversationStore : IConversationStore
{
    private readonly CosmosClient _cosmosClient;

    public CosmosConversationStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("ContainerProfile").GetContainer("ContainerProfile");
    public async Task UpsertConversation(Conversation conversation)
    {
        if (
            conversation == null ||
            string.IsNullOrWhiteSpace(conversation.participants[0]) ||
            string.IsNullOrWhiteSpace(conversation.participants[1]) ||
            conversation.lastModified == 0
            ) 
        {
            throw new ArgumentException($"Invalid profile {conversation}", nameof(conversation));
        }

        await Container.UpsertItemAsync(ToEntity(conversation));
    }

    public async Task<Conversation?> GetConversation(string participant, string conversationId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ConversationEntity>(
                id: conversationId,
                partitionKey: new PartitionKey(participant),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToMessage(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }

    public async Task DeleteConversation(string participant, string conversationId)
    {
        try
        {
            await Container.DeleteItemAsync<Conversation>(
                id: participant,
                partitionKey: new PartitionKey(conversationId)
            );
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            throw;
        }
    }

    private static ConversationEntity ToEntity(Conversation conversation)
    {
        return new ConversationEntity(
            partitionKey: conversation.participants[0],
            id: conversation.conversationId.ToString(),
            conversation.lastModified.ToString(),    
            conversation.participants
        );
    }

    private static Conversation ToMessage(ConversationEntity entity)
    {
        return new Conversation(
            conversationId: new Guid(entity.id),
             long.Parse(entity.lastModified),
        new List<string> {entity.participants[0],entity.participants[1]}
        );
    }
}