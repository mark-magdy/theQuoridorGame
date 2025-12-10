using Microsoft.EntityFrameworkCore;
using QuoridorBackend.DAL.Data;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(QuoridorDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetWithStatsAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.Stats)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}
