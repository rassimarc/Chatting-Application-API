using System.Net;
using Microsoft.Azure.Cosmos;
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

    private Container Container => _cosmosClient.GetDatabase("ContainerConversations").GetContainer("ContainerConversations");
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

    public Task<List<Conversation>?> GetConversations(string participant)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Conversation>?> GetConversations(int pageSize,
        string? continuationToken, string? username, long lastSeenMessageTime)
    {
        var queryText = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.participants, @participant)";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@participant", username);

        var queryResultSetIterator = Container.GetItemQueryIterator<ConversationEntity>(queryDefinition, requestOptions: new QueryRequestOptions()
        {
            MaxItemCount = pageSize
        }, continuationToken: continuationToken);

        var conversations = new List<Conversation>();
        while (queryResultSetIterator.HasMoreResults)
        {
            var queryResponse = await queryResultSetIterator.ReadNextAsync();
            foreach (var entity in queryResponse)
            {
                var conversation = toConversation(entity);
                conversations.Add(conversation);
            }
            if (conversations.Count == pageSize)
            {
                continuationToken = queryResponse.ContinuationToken;
                break;
            }
        }
        if (conversations.Count != pageSize) continuationToken = null;
        return conversations;
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