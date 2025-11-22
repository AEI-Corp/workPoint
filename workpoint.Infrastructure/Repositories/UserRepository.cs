using Microsoft.EntityFrameworkCore;
using workpoint.Domain.Entities;
using workpoint.Domain.Interfaces.Repositories;
using workpoint.Infrastructure.Extensions;

namespace workpoint.Infrastructure.Repositories;

public class UserRepository : IRepository<User>
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    //----------------------------------------------------
    
    // Get All:
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    
    // Get By Id:
    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    
    // Create:
   public async Task<User> CreateAsync(User entity)
    {
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

   
    // Update:
    public async Task<User> UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    
    // Delete:
    public async Task<bool> DeleteAsync(User entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}