using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            try
            {
                await _context.SaveChangesAsync(ct);
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(ct);
                }
            }
            catch
            {
                await RollbackTransactionAsync(ct);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(ct);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
                _context.ChangeTracker.Clear();
            }
        }
    }
}
