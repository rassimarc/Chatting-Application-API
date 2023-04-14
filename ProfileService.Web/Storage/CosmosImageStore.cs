using System.Net;
using Microsoft.Azure.Cosmos;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage.Entities;

namespace ProfileService.Web.Storage;

public class CosmosImageStore : IImageStore
{  
    private readonly CosmosClient _cosmosClient;

    public CosmosImageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    // DRY
    private Container Container => _cosmosClient.GetDatabase("ContainerImages").GetContainer("ContainerImages");

    public async Task UpsertImage(Image image)
    {
        await Container.UpsertItemAsync(ToEntity(image));
    }
    
    public async Task<Image?> GetImage(string name)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ImageEntity>(
                id: name,
                partitionKey: new PartitionKey(name),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToImage(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }    }
    
    public async Task DeleteImage(string image)
    {
        try
        {
            await Container.DeleteItemAsync<Image>(
                id: image,
                partitionKey: new PartitionKey(image)
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

    
    private static ImageEntity ToEntity(Image image)
    {
        return new ImageEntity(
            PartitionKey: image.id,
            id: image.id
            );
    }
    
    private static Image ToImage(ImageEntity entity)
    {
        return new Image(
            id: entity.id
        );
    }
}