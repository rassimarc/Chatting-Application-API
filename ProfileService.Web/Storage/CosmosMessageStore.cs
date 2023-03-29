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

    private Container Container => _cosmosClient.GetDatabase("ContainerProfile").GetContainer("ContainerProfile");
    
    public async Task UpsertMessage(Message message)
    {
        if (message == null ||
            string.IsNullOrWhiteSpace(message.senderUsername) ||
            string.IsNullOrWhiteSpace(message.text) ||
            string.IsNullOrWhiteSpace(message.messageId) ||
            string.IsNullOrWhiteSpace(message.conversationId.ToString()) ||
            message.unixTime == 0
           )
        {
            throw new ArgumentException($"Invalid message {message}", nameof(message));
        }

        await Container.UpsertItemAsync(ToEntity(message));
    }
    
    public async Task<List<Message?>> GetMessages(string conversationId)
    {
        try
        {
            var messages = new List<Message>();
            string partitionKeyName = "conversationId";
            string query = $"SELECT * FROM c WHERE c.{partitionKeyName} = '{conversationId}' ORDER BY c.time DESC";
            
            var queryDefinition = new QueryDefinition(query);
            var resultSetIterator = Container.GetItemQueryIterator<MessageEntity>(queryDefinition);
            
            while (resultSetIterator.HasMoreResults)
            {
                var response = await resultSetIterator.ReadNextAsync();
                foreach (var item in response)
                {
                    var entity = item;
                    messages.Add(ToMessage(entity));
                }
            }
            return messages;
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
    
    public async Task<Message?> GetMessage(string messageId, string conversationId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<MessageEntity>(
                id: messageId,
                partitionKey: new PartitionKey(conversationId),
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
            partitionKey: message.conversationId.ToString(),
            id: message.messageId,
            message.unixTime.ToString(),
            message.text,
            message.senderUsername
        );
    }

    private static Message ToMessage(MessageEntity entity)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(entity.time);
        return new Message(
            messageId: entity.id,
            new Guid(entity.partitionKey),
            entity.senderUsername,
            entity.text,
            long.Parse(entity.time)
        );
    }
}