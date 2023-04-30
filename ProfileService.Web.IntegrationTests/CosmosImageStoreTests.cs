using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.IntegrationTests;

public class CosmosImageStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IImageStore _store;

    private readonly Image _image = new(
        id: Guid.NewGuid().ToString()
    );
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteImage(_image.id);
    }

    public CosmosImageStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IImageStore>();
    }
    
    [Fact]
    public async Task AddNewImage()
    {
        await _store.UpsertImage(_image);
        Assert.Equal(_image, await _store.GetImage(_image.id));
    }

    [Fact]
    public async Task GetNonExistingImage()
    {
        Assert.Null(await _store.GetImage(_image.id));
    }
}