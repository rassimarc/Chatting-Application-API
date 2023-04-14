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
        await Container.UpsertItemAsync(ToEntity(conversation, 0));
        await Container.UpsertItemAsync(ToEntity(conversation, 1));
    }

    public async Task<List<Conversation>?> GetConversations(string participant)
    {
        try
        {
            List<Conversation> conversations= new List<Conversation>();
            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(participant)
            };
            QueryDefinition query = new QueryDefinition(
                "SELECT * FROM c WHERE c.participants[0] = @participant OR c.participants[1] = @participant")
                .WithParameter("@participant", participant);

            FeedIterator<ConversationEntity> iterator = Container.GetItemQueryIterator<ConversationEntity>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(participant) });

            while (iterator.HasMoreResults)
            {
                FeedResponse<ConversationEntity> response = await iterator.ReadNextAsync();
                foreach (ConversationEntity entity in response.Resource)
                {
                    conversations.Add(toConversation(entity));
                }
            }

            return conversations;
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

    private static ConversationEntity ToEntity(Conversation conversation, int participant)
    {
        return new ConversationEntity(
            partitionKey: conversation.participants[participant],
            id: conversation.conversationId.ToString(),
            conversation.lastModified.ToString(),    
            conversation.participants
        );
    }

    private static Conversation toConversation(ConversationEntity entity)
    {
        return new Conversation(
            conversationId: new Guid(entity.id),
             long.Parse(entity.lastModified),
        new List<string> {entity.participants[0],entity.participants[1]}
        );
    }
}