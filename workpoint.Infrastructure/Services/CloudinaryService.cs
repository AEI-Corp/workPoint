using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;

namespace workpoint.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly IConfiguration _config;

    public CloudinaryService(IConfiguration config)
    {
        _config = config;
        _cloudinary = connectCloudinaryEnviroment();
    }

    private Cloudinary connectCloudinaryEnviroment()
    {
       var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") 
                        ?? _config["Cloudinary:Cloud_Name"];
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") 
                     ?? _config["Cloudinary:Api_Key"];
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") 
                        ?? _config["Cloudinary:Api_Secret"];
        
        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret)) 
            throw new ArgumentNullException("Cloudinary connection without any parameter");

        var account = new Account(cloudName, apiKey, apiSecret);
        return new Cloudinary(account);
    }
    
    public async Task<string?> UploadPhotoAsync(UploadPhotoDto entityDto)
    {
        if (entityDto == null) throw new ArgumentNullException("It couldnt Upload a Photo without a photo");

        var uploadparams = new ImageUploadParams()
        {
            // Ahora SÍ tienes el nombre del archivo disponible
            File = new FileDescription(entityDto.Photo.FileName, entityDto.Photo.OpenReadStream()),
            Folder = entityDto.SpaceId != null ? $"workpoint/spaces/{entityDto.SpaceId}" : "workpoint/general"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadparams);

        if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);

        return uploadResult.SecureUrl.AbsoluteUri;
    }
}