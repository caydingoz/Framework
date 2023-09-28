namespace Framework.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWork<TDbContext> : IUnitOfWorkEvents
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        void BeginTransaction();
        Task CommitAsync(CancellationToken cancellationToken = default);
        void Commit();
        Task RollbackAsync(CancellationToken cancellationToken = default);
        void Rollback();
    }
}