using workpoint.Domain.Entities;

namespace workpoint.Domain.Interfaces.Repositories;

public interface IPhotoRepository
{
    Task<IEnumerable<Photo>?> GetAllAsync();
    Task<Photo?> AddAsync(Photo entity);
    Task<bool> RemoveAsync(int id);
    Task<bool> ChangeStatus(int id);
    Task<bool> MaxQty(int? idSpace);
}