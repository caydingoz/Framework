﻿using Framework.Shared.Entities;
using System.Linq.Expressions;
using Framework.Domain.Interfaces.Entities;

namespace Framework.Domain.Interfaces.Repositories
{
    public interface IGenericQueryRepository<T, U> where T : IBaseEntity<U>
    {
        Task<T?> GetByIdAsync(U id, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? selector = null, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> WhereAsync(Expression<Func<T, bool>> selector, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default);
        Task<ICollection<TResult>> WhereWithSelectAsync<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector, bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default);
        Task<ICollection<T>> GetAllAsync(bool readOnly = false, bool includeLogicalDeleted = false, Expression<Func<T, object>>? includes = null, Pagination? pagination = null, ICollection<Sort>? sorts = null, CancellationToken cancellationToken = default);
        Task<long> CountAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>>? selector = null, bool includeLogicalDeleted = false, CancellationToken cancellationToken = default);
    }
}
