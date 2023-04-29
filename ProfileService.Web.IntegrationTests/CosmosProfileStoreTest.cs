using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.IntegrationTests;

public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore _store;

    private readonly Profile _profile = new(
        username: "FooBar",
        firstName: "Foo",
        lastName: "Bar",
        ProfilePictureId: Guid.NewGuid().ToString()
    );
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteProfile(_profile.username);
    }

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IProfileStore>();
    }
    
    [Fact]
    public async Task AddNewProfile()
    {
        await _store.AddProfile(_profile);
        Assert.Equal(_profile, await _store.GetProfile(_profile.username));
    }

    [Fact]
    public async Task GetNonExistingProfile()
    {
        Assert.Null(await _store.GetProfile(_profile.username));
    }
}