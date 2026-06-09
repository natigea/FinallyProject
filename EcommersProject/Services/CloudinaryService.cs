using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EcommersProject.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config["Cloudinary:CloudName"];
        var apiKey = config["Cloudinary:ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"];

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        _cloudinary.Api.Secure = true;
    }

    public async Task<string?> UploadAsync(IFormFile file, string folder)
    {
        if (file.Length == 0) return null;

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.StatusCode == System.Net.HttpStatusCode.OK ? result.SecureUrl.ToString() : null;
    }

    public async Task<string?> UploadRawAsync(Stream stream, string fileName, string folder)
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.StatusCode == System.Net.HttpStatusCode.OK ? result.SecureUrl.ToString() : null;
    }

    public async Task DeleteAsync(string publicId)
    {
        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
