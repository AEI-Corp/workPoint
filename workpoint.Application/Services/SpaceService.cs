using System.Diagnostics;
using AutoMapper;
using workpoint.Application.DTOs;
using workpoint.Application.Interfaces;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;

namespace workpoint.Application.Service;

public class SpaceService : ISpaceService
{
    private readonly IRepository<Space> _spaceRepository;
    private readonly IMapper _mapper;

    public SpaceService(IRepository<Space> spaceRepository, IMapper maper)
    {
        _spaceRepository = spaceRepository;
        _mapper = maper;
    }

    // ------------------------------------------------------
    
    public async Task<IEnumerable<ResponseSpaceDto>> GetAllAsync()
    {
        var spaces = await _spaceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ResponseSpaceDto>>(spaces);
    }

    
    public async Task<ResponseSpaceDto?> GetByIdAsync(int id)
    {
        var space = await _spaceRepository.GetByIdAsync(id);
        return _mapper.Map<ResponseSpaceDto>(space);
    }

    
    // CREATE:
    public async Task<ResponseSpaceDto> CreateAsync(SpaceCreateDto spaceDto)
    {
        if (spaceDto == null)
            throw new ArgumentNullException(nameof(spaceDto), "El cuerpo de la petición no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(spaceDto.SpaceName))
            throw new ArgumentNullException("El nombre es obligatorio.");

        if (spaceDto.CategorieId <= 0)
            throw new ArgumentNullException("La categoría debe ser válida.");

        if (spaceDto.BranchId <= 0)
            throw new ArgumentNullException("La sede debe ser válida.");

        if (spaceDto.Price <= 0)
            throw new ArgumentNullException("El precio no puede ser negativo.");

        if (spaceDto.MaxCapacity <= 0)
            throw new ArgumentNullException("La capacidad debe ser mayor a cero.");
        
        // TODO:
        // Validations: exists in DB:
        // var branches = await _branchRepository.GetAllAsync();
        // if (!branches.Any(b => b.Id == spaceDto.BranchId))
        //     throw new ArgumentException($"El Branch con Id={spaceDto.BranchId} no existe.");
        //
        // var categories = await _categoryRepository.GetAllAsync();
        // if (!categories.Any(c => c.Id == spaceDto.CategorieId))
        //     throw new ArgumentException($"La categoría con Id={spaceDto.CategorieId} no existe.");
        
        var space = _mapper.Map<Space>(spaceDto);
        space.CreatedAt = DateTime.UtcNow;
        space.UpdatedAt = DateTime.UtcNow;
        
        var response = await _spaceRepository.CreateAsync(space);

        return _mapper.Map<ResponseSpaceDto>(response);
    }

    
    // UPDATE:
    public async Task<bool> UpdateAsync(SpaceUpdateDto spaceDto)
    {
        if (spaceDto == null)
            throw new ArgumentNullException(nameof(spaceDto), "El cuerpo de la petición no puede estar vacío.");
        
        if (spaceDto.Id <= 0)
            throw new ArgumentException("El Id del espacio debe ser un número válido.");

        if (string.IsNullOrWhiteSpace(spaceDto.SpaceName))
            throw new ArgumentNullException("El nombre es obligatorio.");

        if (spaceDto.CategorieId <= 0)
            throw new ArgumentNullException("La categoría debe ser válida.");

        if (spaceDto.BranchId <= 0)
            throw new ArgumentNullException("La sede debe ser válida.");

        if (spaceDto.Price <= 0)
            throw new ArgumentNullException("El precio no puede ser negativo.");

        if (spaceDto.MaxCapacity <= 0)
            throw new ArgumentNullException("La capacidad debe ser mayor a cero.");
        
        var exists = await _spaceRepository.GetByIdAsync(spaceDto.Id);

        if (exists == null)
            return false;

        // Validation to allow spaces updates only if the user is the space owner: 
        if (exists.UserId != null && exists.UserId != spaceDto.UserId)
            throw new UnauthorizedAccessException("No tienes permiso para actualizar este espacio.");
        
        _mapper.Map(spaceDto, exists);
        exists.UpdatedAt = DateTime.UtcNow;

        await _spaceRepository.UpdateAsync(exists);
        return true;
    }

    
    // DELETE:
    public async Task<bool> DeleteAsync(int id)
    {
        
        var exists = await _spaceRepository.GetByIdAsync(id);

        if (exists == null)
            return false;

        return await _spaceRepository.DeleteAsync(exists);

    }
    
}