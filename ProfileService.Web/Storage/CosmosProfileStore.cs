using System.Net;
using Microsoft.Azure.Cosmos;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage.Entities;

namespace ProfileService.Web.Storage;

public class CosmosProfileStore : IProfileStore
{
    private readonly CosmosClient _cosmosClient;

    public CosmosProfileStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("ContainerProfile").GetContainer("ContainerProfile");
    public async Task AddProfile(Profile profile)
    {
        ValidateProfile(profile);
        await Container.UpsertItemAsync(ToEntity(profile));
    }

    
    private static void ValidateProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.username) ||
            string.IsNullOrWhiteSpace(profile.firstName) ||
            string.IsNullOrWhiteSpace(profile.lastName)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
    }
    
    public async Task UpsertProfile(Profile profile)
    {
        ValidateProfile(profile);
        await Container.UpsertItemAsync(ToEntity(profile));
    }
    public async Task<Profile?> GetProfile(string username)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ProfileEntity>(
                id: username,
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
    
// ConsistencyLevel.Strong - This level provides strong consistency, which means that all replicas will have the same data at all times. This level is appropriate for applications that require the highest level of consistency, but it can also have an impact on performance and availability.
// ConsistencyLevel.Session - This level provides consistency within a session, which means that all operations within a session will see their own updates in the order they were performed. This level is appropriate for applications that require low-latency and high throughput, while still maintaining a reasonable level of consistency.
// ConsistencyLevel.Eventual - This level provides eventual consistency, which means that all replicas will eventually converge to the same state, but there may be some lag in the data. This level is appropriate for applications that can tolerate some inconsistency or lag in the data, and prioritize high availability and partition tolerance over strong consistency.

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

    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.username,
            id: profile.username,
            profile.firstName,
            profile.lastName,
            profile.ProfilePictureId
        );
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        return new Profile(
            username: entity.id,
            entity.firstName,
            entity.lastName,
            entity.ProfilePictureId
        );
    }
}
