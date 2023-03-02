using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;
using ContentDispositionHeaderValue = System.Net.Http.Headers.ContentDispositionHeaderValue;

namespace ProfileService.Web.Tests.Controllers;

public class ImageControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IImageStore> _imageStoreMock = new();
    private readonly HttpClient _httpClient;

    public ImageControllerTests(WebApplicationFactory<Program> factory)
    {
        // DRY: Don't repeat yourself
        _httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(_imageStoreMock.Object); });
        }).CreateClient();
    }
    private readonly Image _image = new(
        id: "3b072fac-df4a-4d5b-be2f-2350adc38af1"
    );
    
    
    [Fact]
    public async Task UploadImage()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "test.png"
            };

            using var formData = new MultipartFormDataContent();
            formData.Add(fileStreamContent);
            formData.Add(new StringContent(JsonConvert.SerializeObject(_image), Encoding.Default, "application/json"), "json");
            
            var response = await _httpClient.PostAsync("/Image", formData);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _imageStoreMock.Verify(mock => mock.UpsertImage(It.IsAny<Image>()), Times.Once);

    }
    
[Fact]
    public async Task DownloadImage_ExistingImage()
    {
        var imageBytes = new byte[1024];
        var random = new Random();
        random.NextBytes(imageBytes);
        var contentType = "image/png";
            //_imageStoreMock.Setup(x => x.GetImage(_image.id)).ReturnsAsync(_image);
            _imageStoreMock.Setup(x => x.GetImage(_image.id)).ReturnsAsync(_image);

            var response = await _httpClient.GetAsync($"/Image/{_image.id}");
            var contentStream = await response.Content.ReadAsStreamAsync();
        
            Assert.NotNull(contentStream);
            Assert.Equal(contentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal($"{_image.id}.png", response.Content.Headers.ContentDisposition.FileName);
        
            using (var memoryStream = new MemoryStream())
            {
                await contentStream.CopyToAsync(memoryStream);
                Assert.Equal(imageBytes, memoryStream.ToArray());
            }
        }
        

}