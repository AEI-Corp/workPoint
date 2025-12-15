using AutoMapper;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;


namespace workpoint.Application.Services;

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository _photoRepository; 
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService; // <- Now we inject CloudinaryService

    public PhotoService(IPhotoRepository photoRepository, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _photoRepository = photoRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IEnumerable<PhotoResponseDto>?> GetAllPhotosAsync()
    {
        var photos = await _photoRepository.GetAllAsync();
        if (photos == null) return null; 

        return _mapper.Map<IEnumerable<PhotoResponseDto>>(photos);
    }

    public async Task<PhotoResponseDto?> AddPhotoAsync(PhotoAddDto photoDto)
    {
        if (photoDto == null)
            throw new ArgumentNullException(nameof(photoDto), "El DTO de la foto no puede ser nulo.");
        
        // 1. To verify the max amount of photos using the repository.
        // We use photoDto.spaceId (lower case) to respect the entrance DTO.
        var maxReached = await _photoRepository.MaxQty(photoDto.SpaceId); 
        if (maxReached)
        {
            return null; 
        }

        // 2. Uploading images and obtain the url (using ICloudinaryService)
        var uploadDto = new UploadPhotoDto
        {
            Photo = photoDto.Photo,
            SpaceId = photoDto.SpaceId // To use SpaceId
        };
        
        
        var urlImage = await _cloudinaryService.UploadPhotoAsync(uploadDto);

        if (string.IsNullOrEmpty(urlImage))
        {
            throw new InvalidOperationException("No se pudo subir la imagen a Cloudinary."); 
        }

        // 3. Creating the entity and save on the database.
        var newPhoto = new Photo
        {
            SpaceId = photoDto.SpaceId,
            UrlImage = urlImage,
            Active = true, 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var addedPhoto = await _photoRepository.AddAsync(newPhoto);

        if (addedPhoto == null) 
        {
            // Optional: If the addition fails after uploading, it is recommended
            // to implement a logic to delete the picture of Cloudinary (rollback).
            return null;
        }

        return _mapper.Map<PhotoResponseDto>(addedPhoto);
    }

    public async Task<bool> ChangePhotoStatusAsync(int id)
    {
        return await _photoRepository.ChangeStatus(id);
    }

    public async Task<bool> RemovePhotoAsync(int id)
    {
        // Logic to delete on DB
        var removed = await _photoRepository.RemoveAsync(id);
        
        // Optional: You could search the picture url before that delete its from DB
        // and call a CloudinaryService method for deleting the resource.
        
        return removed;
    }
}