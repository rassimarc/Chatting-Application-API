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
    public async Task AddConversation(Conversation conversation)
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

    public async Task<(List<Conversation> conversations, string? continuationToken)> GetConversations(
        string participant, int? pageSize,
        string? continuationToken, string lastSeenConversationTime)
    {
        var queryText = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.participants, @participant) AND c.lastModified > @lastSeenConversationTime ORDER BY c.lastModified DESC";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@participant", participant)
            .WithParameter("@lastSeenConversationTime", lastSeenConversationTime);
            

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
                var conversation = ToConversation(entity);
                conversations.Add(conversation);
            }
            if (conversations.Count == pageSize)
            {
                continuationToken = queryResponse.ContinuationToken;
                break;
            }
        }
        if (conversations.Count != pageSize) continuationToken = null;
        return (conversations, continuationToken);
    }
    
    public async Task<Conversation?> GetConversation(string participant, string conversationId)
    {
        var queryText = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.participants, @participant) AND c.id = @conversationId";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@participant", participant)
            .WithParameter("@conversationId", conversationId);

        var queryResultSetIterator = Container.GetItemQueryIterator<ConversationEntity>(queryDefinition);

        var queryResponse = await queryResultSetIterator.ReadNextAsync();
        var entity = queryResponse.FirstOrDefault();

        if (entity == null)
        {
            return null;
        }

        var conversation = ToConversation(entity);
        return conversation;
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
            id: conversation.conversationId,
            conversation.lastModified.ToString(),    
            conversation.participants
        );
    }

    private static Conversation ToConversation(ConversationEntity entity)
    {
        return new Conversation(
            conversationId: new Guid(entity.id).ToString(),
            lastModified: long.Parse(entity.lastModified),
            participants: new[] { entity.participants[0], entity.participants[1] }
        );

    }
}