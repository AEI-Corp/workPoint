using workpoint.Application.DTOs;
using workpoint.Domain.Entities;

namespace workpoint.Application.Interfaces;

public interface ISpaceService
{
    Task<IEnumerable<Space>> GetAllAsync();
    Task<Space?> GetByIdAsync(int id);
    Task<Space> CreateAsync(SpaceCreateDto spaceDto);
    Task<bool> UpdateAsync(SpaceUpdateDto spaceDto);
    Task<bool> DeleteAsync(Space space);
    
}