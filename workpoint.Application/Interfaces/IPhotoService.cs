using workpoint.Application.DTOs;

namespace workpoint.Application.Interfaces;

public interface IPhotoService
{
    Task<IEnumerable<PhotoResponseDto>?> GetAllPhotosAsync();
    Task<PhotoResponseDto?> AddPhotoAsync(PhotoAddDto photoDto);
    Task<bool> ChangePhotoStatusAsync(int id);
    Task<bool> RemovePhotoAsync(int id);
}