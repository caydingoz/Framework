using Framework.Domain.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Framework.EF
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        protected TDbContext DbContext;
        public UnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
        }
        protected IDbContextTransaction? Transaction { get; private set; } = null;

        public event CommittedEventHandler? Committed;
        public event RollBackedEventHandler? RollBacked;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction == null)
                throw new Exception("Transaction is null");
            await Transaction.CommitAsync(cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            await Transaction.DisposeAsync();
            Transaction = null;
            if (Committed is not null && Committed.GetInvocationList().Any())
                await Task.Run(() => Committed(this, EventArgs.Empty), cancellationToken);
        }

        public void Commit()
        {
            if (Transaction == null)
                throw new Exception("Transaction is null");
            Transaction.Commit();
            DbContext.SaveChanges();
            Transaction.Dispose();
            Transaction = null;
            if (Committed is not null && Committed.GetInvocationList().Any())
                Committed(this, EventArgs.Empty);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction == null)
                throw new Exception("Transaction is null");
            await Transaction.RollbackAsync(cancellationToken);
            await Transaction.DisposeAsync();
            Transaction = null;
            if (RollBacked is not null && RollBacked.GetInvocationList().Any())
                await Task.Run(() => RollBacked(this, EventArgs.Empty), cancellationToken);
        }

        public void Rollback()
        {
            if (Transaction == null)
                throw new Exception("Transaction is null");
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
            if (RollBacked is not null && RollBacked.GetInvocationList().Any())
                RollBacked(this, EventArgs.Empty);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction == null)
                Transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public void BeginTransaction()
        {
            if (Transaction == null)
                Transaction = DbContext.Database.BeginTransaction();
        }
    }
}
