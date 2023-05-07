using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Tests.Controllers;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> _profileStoreMock = new();
    private readonly HttpClient _httpClient;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
        }).CreateClient();
    }

    [Fact]
    public async Task GetProfile()
    {
        var profilePictureId = Guid.NewGuid().ToString();
        var profile = new Profile("foobar", "Foo", "Bar", profilePictureId);
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        var response = await _httpClient.GetAsync($"/api/Profile/{profile.username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetProfile_NotFound()
    {
        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);

        var response = await _httpClient.GetAsync($"/api/Profile/foobar");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile()
    {
        var profilePictureId = Guid.NewGuid().ToString();
        var profile = new Profile("foobar", "Foo", "Bar", profilePictureId);
        var response = await _httpClient.PostAsync("/api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("http://localhost/api/Profile/foobar", response.Headers.GetValues("Location").First());
    }

    [Fact]
    public async Task AddProfile_Conflict()
    {
        var profilePictureId = Guid.NewGuid().ToString();
        var profile = new Profile("foobar", "Foo", "Bar", profilePictureId);
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);

        var response = await _httpClient.PostAsync("/api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        _profileStoreMock.Verify(m => m.AddProfile(profile), Times.Never);
    }

    [Theory]
    [InlineData(null, "Foo", "Bar")]
    [InlineData("", "Foo", "Bar")]
    [InlineData(" ", "Foo", "Bar")]
    [InlineData("foobar", null, "Bar")]
    [InlineData("foobar", "", "Bar")]
    [InlineData("foobar", "   ", "Bar")]
    [InlineData("foobar", "Foo", "")]
    [InlineData("foobar", "Foo", null)]
    [InlineData("foobar", "Foo", " ")]
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName)
    {
        var profile = new Profile(username, firstName, lastName, "");
        var response = await _httpClient.PostAsync("/api/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.AddProfile(profile), Times.Never);
    }

    [Fact]
    public async Task UpdateProfile()
    {
        var profile = new Profile("foobar", "Foo", "Bar", "");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);
        

        var updatedProfile = profile with { firstName = "Foo2" };

        var response = await _httpClient.PutAsync($"/api/Profile/{profile.username}",
            new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateProfile_NotFound()
    {
        var profile = new Profile("foobar", "Foo", "Bar", "");

        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);
        
        var response = await _httpClient.PutAsync($"/api/Profile/{profile.username}",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.AddProfile(profile), times: Times.Exactly(0));
    }
    
    [Theory]
    [InlineData("foobar", null, "Bar")]
    [InlineData("foobar", "", "Bar")]
    [InlineData("foobar", "   ", "Bar")]
    [InlineData("foobar", "Foo", "")]
    [InlineData("foobar", "Foo", null)]
    [InlineData("foobar", "Foo", " ")]
    public async Task UpdateProfile_InvalidArgs(string username, string firstName, string lastName)
    {
        var profile = new Profile("foobar", "Foo", "Bar", "");
        _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);
        

        var updatedProfile = new Profile(username, firstName, lastName, "");

        var response = await _httpClient.PutAsync($"/api/Profile/{profile.username}",
            new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.AddProfile(updatedProfile), times: Times.Exactly(0));
    }
}