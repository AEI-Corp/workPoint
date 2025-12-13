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
    private readonly ICloudinaryService _cloudinaryService; // <- Ahora inyectamos CloudinaryService

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
        
        // 1. Verificar la cantidad máxima de fotos usando el repositorio
        // Usamos photoDto.spaceId (minúscula) para respetar tu DTO de entrada.
        var maxReached = await _photoRepository.MaxQty(photoDto.SpaceId); 
        if (maxReached)
        {
            return null; 
        }

        // 2. Subir la imagen y obtener la URL (Usando ICloudinaryService)
        var uploadDto = new UploadPhotoDto
        {
            Photo = photoDto.Photo,
            SpaceId = photoDto.SpaceId // Usar SpaceId
        };
        
        // 
        var urlImage = await _cloudinaryService.UploadPhotoAsync(uploadDto);

        if (string.IsNullOrEmpty(urlImage))
        {
            throw new InvalidOperationException("No se pudo subir la imagen a Cloudinary."); 
        }

        // 3. Crear la entidad y guardar en la base de datos
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
            // Opcional: Si la adición falla después de la subida, se recomienda
            // implementar una lógica para eliminar la foto de Cloudinary (rollback).
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
        // Lógica de eliminación en DB
        var removed = await _photoRepository.RemoveAsync(id);
        
        // Opcional: Podrías buscar la URL de la foto antes de eliminarla de DB
        // y llamar a un método de CloudinaryService para eliminar el recurso.
        
        return removed;
    }
}