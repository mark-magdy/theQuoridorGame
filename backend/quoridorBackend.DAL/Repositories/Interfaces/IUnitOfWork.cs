using QuoridorBackend.DAL.Data;

namespace QuoridorBackend.DAL.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    QuoridorDbContext Context { get; }
    IUserRepository Users { get; }
    IGameRepository Games { get; }
    
    Task<int> CompleteAsync();
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
