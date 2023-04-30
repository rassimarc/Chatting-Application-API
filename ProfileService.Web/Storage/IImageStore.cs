using ProfileService.Web.Dtos;

namespace ProfileService.Web.Storage;

public interface IImageStore
{
    Task UpsertImage(Image image);
    Task<Image?> GetImage(string name);
    Task DeleteImage(string imageId);

}