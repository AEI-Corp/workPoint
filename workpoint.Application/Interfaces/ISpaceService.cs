using workpoint.Application.DTOs;
using workpoint.Domain.Entities;

namespace workpoint.Application.Interfaces;

public interface ISpaceService
{
    Task<IEnumerable<ResponseSpaceDto>> GetAllAsync();
    Task<ResponseSpaceDto?> GetByIdAsync(int id);
    Task<ResponseSpaceDto> CreateAsync(SpaceCreateDto spaceDto);
    Task<bool> UpdateAsync(SpaceUpdateDto spaceDto);
    Task<bool> DeleteAsync(int id);
    
}