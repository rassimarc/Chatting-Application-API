using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.IntegrationTests;

public class CosmosConversationStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore _profileStore;
    private readonly IConversationStore _store;

    private readonly Profile _profile1 = new(
        username: "FooBar1",
        firstName: "Foo1",
        lastName: "Bar1",
        ProfilePictureId: Guid.NewGuid().ToString()
    );
    
    private readonly Profile _profile2 = new Profile(
        username: "FooBar1",
        firstName: "Foo2",
        lastName: "Bar2",
        ProfilePictureId: Guid.NewGuid().ToString()
    );
    
    public readonly Conversation _conversation = new Conversation(
        conversationId: Guid.NewGuid().ToString(),
        lastModified: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        participants: new[] { "FooBar1", "FooBar1"}
    );

    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteConversation(_conversation.participants[0], _conversation.conversationId);
    }
    

    public CosmosConversationStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IConversationStore>();
        _profileStore = factory.Services.GetRequiredService<IProfileStore>();
    }
    
    [Fact]
    public async Task AddConversation()
    {
        await _store.AddConversation(_conversation);
        Assert.Equal(_conversation, await _store.GetConversation(_conversation.participants[0], _conversation.conversationId));
    }

    [Fact]
    public async Task GetNonExistingConversation()
    {
        Assert.Null(await _store.GetConversation(_conversation.participants[0], _conversation.conversationId));
    }
}