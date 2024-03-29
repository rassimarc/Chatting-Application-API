using System.Net;
using Microsoft.Azure.Cosmos;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage.Entities;

namespace ProfileService.Web.Storage;
public class CosmosMessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;

    public CosmosMessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("ConversationMessages").GetContainer("ConversationMessages");

    public async Task AddMessage(Message message)
    {
        if (message == null ||
            string.IsNullOrWhiteSpace(message.senderUsername) ||
            string.IsNullOrWhiteSpace(message.text) ||
            string.IsNullOrWhiteSpace(message.conversationId) ||
            string.IsNullOrWhiteSpace(message.messageId) ||
            message.unixTime == 0
           )
        {
            throw new ArgumentException($"Invalid message {message}", nameof(message));
        }

        await Container.UpsertItemAsync(ToEntity(message));
    }


    public async Task<(List<Message> messages, string? continuationToken)> GetMessages(int? pageSize,
        string? continuationToken, string? conversationId, string lastSeenMessageTime)
    {
        var queryText = "SELECT * FROM c WHERE c.partitionKey = @conversationId AND c.time > @lastSeenMessageTime ORDER BY c.time DESC";
        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@conversationId", conversationId)
            .WithParameter("@lastSeenMessageTime", lastSeenMessageTime);
        var queryResultSetIterator = Container.GetItemQueryIterator<MessageEntity>(queryDefinition, requestOptions: new QueryRequestOptions()
        {
            MaxItemCount = pageSize
        }, continuationToken: continuationToken);
        
        var messages = new List<Message>();
        while (queryResultSetIterator.HasMoreResults)
        {
            var queryResponse = await queryResultSetIterator.ReadNextAsync();
            foreach (var entity in queryResponse)
            {
                var conversation = ToMessage(entity);
                messages.Add(conversation);
            }

            if (messages.Count == pageSize)
            {
                continuationToken = queryResponse.ContinuationToken;
                break;
            }
        }

        if (messages.Count != pageSize) continuationToken = null;
        return (messages, continuationToken);
    }
    
    public async Task DeleteMessage(string messageId, string conversationId)
    {
        try
        {
            await Container.DeleteItemAsync<Message>(
                id: messageId,
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

    private static MessageEntity ToEntity(Message message)
    {
        return new MessageEntity(
            partitionKey: message.conversationId,
            id: message.messageId,
            message.unixTime.ToString(),
            message.text,
            message.senderUsername
        );
    }

    private static Message ToMessage(MessageEntity entity)
    {
        return new Message(
            messageId: entity.id,
            entity.partitionKey,
            entity.senderUsername,
            entity.text,
            long.Parse(entity.time)
        );
    }
}