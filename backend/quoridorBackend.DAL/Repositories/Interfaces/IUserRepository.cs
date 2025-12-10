using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetWithStatsAsync(Guid userId);
}
