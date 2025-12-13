using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace workpoint.Infrastructure.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly AppDbContext _context;

    public PhotoRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Photo>?> GetAllAsync()
    {
        return await _context.Photos.ToListAsync();
    }

    public async Task<Photo?> AddAsync(Photo entity)
    {
        if (await MaxQty(entity.SpaceId) == true) return null;
        await _context.Photos.AddAsync(entity);
        _context.SaveChanges();
        return entity;
    }

    public async Task<bool> ChangeStatus(int id)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null) return false;
        photo.Active = !photo.Active;
        _context.Photos.Update(photo);
        _context.SaveChanges();
        return true;
    }
    
    public async Task<bool> RemoveAsync(int id)
    {
        var entity = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return false;
        _context.Photos.Remove(entity);
        _context.SaveChanges();
        return true;
    }

    public async Task<bool> MaxQty(int? idSpace)
    {
        if (idSpace == null) return true;
        var list = await _context.Photos.Where(p => p.SpaceId == idSpace).ToListAsync();
        if (list.Count >= 5) return true;
        return false;
    }
}