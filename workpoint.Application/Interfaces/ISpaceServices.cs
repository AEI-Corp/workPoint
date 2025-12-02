using workpoint.Domain.Entities;

namespace workpoint.Application.Interfaces;

public interface ISpaceServices
{
    Task<IEnumerable<Space>> GetAllAsync();
    Task<Space?> GetByIdAsync(int id);
    Task<Space> CreateAsync(RegisterSpaceRequest dto);
    Task<bool> UpdateAsync(int id, SpaceUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    //TODO:
}