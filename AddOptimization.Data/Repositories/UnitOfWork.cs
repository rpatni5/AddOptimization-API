using Microsoft.EntityFrameworkCore.Storage;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;

namespace AddOptimization.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AddOptimizationContext _context;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(AddOptimizationContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await _currentTransaction.RollbackAsync();
                throw;
            }
            finally
            {
                _currentTransaction.Dispose();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            await _currentTransaction.RollbackAsync();
            _currentTransaction.Dispose();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
