using workpoint.Application.DTOs;

namespace workpoint.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string?> UploadPhotoAsync(UploadPhotoDto entityDto);
}