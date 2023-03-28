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
            string.IsNullOrWhiteSpace(message.time.ToString()) ||
            message.messageId == Guid.Empty ||
            message.conversationId == Guid.Empty
           )
        {
            throw new ArgumentException($"Invalid profile {message}", nameof(message));
        }

        await Container.UpsertItemAsync(ToEntity(message));
    }

    public async Task<SendMessageResponse?> GetMessage(string messageId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<MessageEntity>(
                id: messageId,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToProfile(entity);
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

    public async Task DeleteProfile(string username)
    {
        try
        {
            await Container.DeleteItemAsync<Profile>(
                id: username,
                partitionKey: new PartitionKey(username)
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
            id: message.messageId.ToString(),
            message.time.ToString(),
            message.text,
            message.senderUsername
        );
    }

    private static Message ToProfile(MessageEntity entity)
    {
        return new Message(
            messageId: new Guid(entity.id),
            new Guid(entity.partitionKey),
            entity.senderUsername,
            entity.text,
            entity.time
        );
    }
}