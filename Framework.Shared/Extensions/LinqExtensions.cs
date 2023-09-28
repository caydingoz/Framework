using Framework.Shared.Entities;
using Framework.Shared.Enums;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Framework.Shared.Extensions
{
    public static class LinqExtensions
    {

        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, ICollection<Sort>? sorts)
        {
            if(sorts is null)
                return query;
            bool isFirstElement = true;

            foreach (var sort in sorts)
            {
                string orderByMethod;

                if (isFirstElement)
                {
                    orderByMethod = (sort.Type == SortTypes.ASC) ? "OrderBy" : "OrderByDescending";

                    isFirstElement = false;
                }
                else
                {
                    orderByMethod = (sort.Type == SortTypes.ASC) ? "ThenBy" : "ThenByDescending";
                }

                ParameterExpression pe = Expression.Parameter(query.ElementType);
                MemberExpression me = Expression.Property(pe, sort.Name);

                MethodCallExpression orderByCall = Expression.Call(
                    type : typeof(Queryable), 
                    orderByMethod, 
                    new Type[] { query.ElementType, me.Type }, 
                    query.Expression,
                    Expression.Quote(Expression.Lambda(me, pe)
                    ));

                query = query.Provider.CreateQuery(orderByCall) as IOrderedQueryable<T>;
            }

            return query;
        }
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, Pagination? pagination)
        {
            if (pagination is null)
                return query;
            if (pagination.Count < 0) pagination.Count = 0;
            if (pagination.Page < 0) pagination.Page = 0;
            return query.Skip(pagination.Page * pagination.Count).Take(pagination.Count);
        }
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, string whereClause)
        {
            if (!condition || string.IsNullOrEmpty(whereClause))
                return query;

            return query.Where(whereClause);
        }

    }
}
