using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;

namespace workpoint.Infrastructure.Repositories;

public class SpaceRepository : IRepository<Space>
{
    private readonly AppDbContext _context;
    
    public SpaceRepository(AppDbContext context)
    {
        _context = context;
    }   
    
    // -----------------------------------------------
    
    // GET ALL:
    public async Task<IEnumerable<Space>> GetAllAsync()
    {
        return await _context.Spaces.ToListAsync();
    }

    
    // GET BY ID:
    public async Task<Space> GetByIdAsync(int id)
    {
        return await _context.Spaces.FindAsync(id);
    }

    
    // CREATE:
    public async Task<Space> CreateAsync(Space space)
    {
        _context.Spaces.Add(space);
        await _context.SaveChangesAsync();
        return space;
    }

    
    // UPDATE:
    public async Task<Space> UpdateAsync(Space space)
    {
        _context.Spaces.Update(space);
        await _context.SaveChangesAsync();
        return space;
    }

    
    // DELETE:
    public async Task<bool> DeleteAsync(Space space)
    {
        _context.Spaces.Remove(space);
        await _context.SaveChangesAsync();
        return true;
    }
}