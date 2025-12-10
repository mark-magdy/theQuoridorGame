using Microsoft.EntityFrameworkCore.Storage;
using QuoridorBackend.DAL.Data;
using QuoridorBackend.DAL.Repositories.Interfaces;

namespace QuoridorBackend.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly QuoridorDbContext _context;
    private IDbContextTransaction? _transaction;

    public QuoridorDbContext Context => _context;
    public IUserRepository Users { get; }
    public IGameRepository Games { get; }

    public UnitOfWork(QuoridorDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Games = new GameRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
