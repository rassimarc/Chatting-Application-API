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
    private readonly Mock<IImageStore> _imageStoreMock = new();
    private readonly HttpClient _httpClient;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); services.AddSingleton(_imageStoreMock.Object); });
        }).CreateClient();
    }
    
    
    private readonly Profile _profile = new(
        username: "FooBar",
        firstName: "Foo",
        lastName: "Bar",
        ProfilePictureId: Guid.NewGuid().ToString()
    );

    [Fact]
    public async Task GetProfile()
    {
        
        _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
            .ReturnsAsync(_profile);

        var response = await _httpClient.GetAsync($"/Profile/{_profile.username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(_profile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetProfile_NotFound()
    {
        _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);

        var response = await _httpClient.GetAsync($"/Profile/foobar");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile_ImageNotExist()
    {
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(_profile), Encoding.Default, "application/json"));

        _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
            .ReturnsAsync((Profile?)null);
        _profileStoreMock.Setup(x => x.UpsertProfile(It.IsAny<Profile>())).Returns(Task.CompletedTask);

        _imageStoreMock.Setup(m => m.GetImage(_profile.ProfilePictureId.ToString()))
            .ReturnsAsync((Image?)null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        _profileStoreMock.Verify(x => x.GetProfile(_profile.username), Times.Once);
        _profileStoreMock.Verify(x => x.UpsertProfile(It.Is<Profile>(p => p.username == _profile.username && p.firstName == _profile.firstName && p.lastName == _profile.lastName && p.ProfilePictureId == "3f7c9a85-1825-499f-92fb-c46882afbea2")), Times.Once);
        
    }

    [Fact]
    public async Task AddProfile_Conflict()
    {
        _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
            .ReturnsAsync(_profile);

        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(_profile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        _profileStoreMock.Verify(m => m.UpsertProfile(_profile), Times.Never);
    }

    [Theory]
    [InlineData(null, "Foo", "Bar","{3f7c9a85-1825-499f-92fb-c46882afbea2}" )]
    [InlineData("", "Foo", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    [InlineData(" ", "Foo", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    [InlineData("foobar", null, "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    [InlineData("foobar", "", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    [InlineData("foobar", "   ", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    [InlineData("foobar", "Foo", "", "{3fa85f64-5717-4562-b3fc-2c963f66afa6}")]
    [InlineData("foobar", "Foo", " ", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
    {
        var profile = new Profile(username, firstName, lastName, profilePictureId);
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
    
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
    }
    
    [Fact]
    public async Task AddProfile_InvalidArgsGuid ()
    {
        var updatedProfile = _profile with {ProfilePictureId = ""};
        var response = await _httpClient.PostAsync("/Profile",
            new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
    
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _profileStoreMock.Verify(mock => mock.UpsertProfile(updatedProfile), Times.Never);
    }

    // [Fact]
    // public async Task UpdateProfile()
    // {
    //     _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
    //         .ReturnsAsync(_profile);
    //
    //     var updatedProfile = _profile with { firstName = "Foo2" };
    //
    //     var response = await _httpClient.PutAsync($"/Profile/{_profile.username}",
    //         new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     _profileStoreMock.Verify(mock => mock.UpsertProfile(updatedProfile));
    // }
    //
    // [Fact]
    // public async Task UpdateProfile_NotFound()
    // {
    //     _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
    //         .ReturnsAsync((Profile?)null);
    //
    //     var updatedProfile = _profile with { firstName = "Foo2" };
    //
    //     var response = await _httpClient.PutAsync($"/Profile/{_profile.username}",
    //         new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //     _profileStoreMock.Verify(mock => mock.UpsertProfile(updatedProfile), Times.Never);
    // }
    //
    //
    // [Theory]
    // [InlineData(null, "Foo", "Bar","{3f7c9a85-1825-499f-92fb-c46882afbea2}" )]
    // [InlineData("", "Foo", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData(" ", "Foo", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData("foobar", null, "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData("foobar", "", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData("foobar", "   ", "Bar", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData("foobar", "Foo", "", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // [InlineData("foobar", "Foo", null, "")]
    // [InlineData("foobar", "Foo", " ", "{3f7c9a85-1825-499f-92fb-c46882afbea2}")]
    // public async Task UpdateProfile_InvalidArgs(string username, string firstName, string lastName, Guid profilePictureId)
    // {
    //  
    //     _profileStoreMock.Setup(m => m.GetProfile(_profile.username))
    //         .ReturnsAsync(_profile);
    //     
    //     var updatedProfile = _profile with {username = username, firstName = firstName, lastName = lastName, ProfilePictureId = profilePictureId};
    //
    //     var response = await _httpClient.PutAsync($"/Profile/{_profile.username}",
    //         new StringContent(JsonConvert.SerializeObject(updatedProfile), Encoding.Default, "application/json"));
    //     
    //     Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    //     _profileStoreMock.Verify(mock => mock.UpsertProfile(updatedProfile), Times.Never);
    //     
    // }
}