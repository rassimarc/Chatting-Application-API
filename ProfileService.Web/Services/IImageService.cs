using Microsoft.AspNetCore.Http;
using ProfileService.Web.Dtos;
using System;
using System.Threading.Tasks;

namespace ProfileService.Web.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<byte[]> DownloadImageAsync(Guid guid);
    }
}