using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Framework.EF.Extensions
{
    public static class EfCoreExtensions
    {
        private static List<string> IncludedEntityProps = [];
        public static IQueryable<T> MultipleInclude<T>(this IQueryable<T> query, Expression<Func<T, object>>? include = null) where T : class
        {
            if (include is not null)
                query = query.RecursiveInclude(include.Body);

            return query;
        }
        private static IQueryable<K> RecursiveInclude<T,K>(this IQueryable<K> query, T exp) where T : Expression where K : class
        {
            if (exp is MemberExpression mExp)                   //prop ise (örnek: x.Classrooms)
            {
                string includeStr = string.Empty;
                IncludedEntityProps.ForEach(prop => includeStr += prop + "." );
                includeStr += mExp.Member.Name;
                query = query.Include(includeStr);
            }
            else if (exp is MethodCallExpression mcExp)         //select ise (örnek: x.Classrooms.Select())
            {
                var upperLayerPropName = mcExp.Arguments[0].ToString().Split('.')[1];//K entitysinin propu (içindekilerin dahil olması istenen ilk katman prop)
                IncludedEntityProps.Add(upperLayerPropName);

                var insideSelect = ((LambdaExpression)mcExp.Arguments[1]).Body; //selectin içerisi

                query = query.RecursiveInclude(insideSelect);
                IncludedEntityProps.Remove(upperLayerPropName);
            }
            else if (exp is ListInitExpression liExp)           //list ise (örnek: List<T,object>)
            {
                foreach (var listItem in liExp.Initializers)//List<T,object> listesini döndürüyoruz
                {
                    var includeItem = listItem.Arguments[0];//include itemi (argument hep 1 adet)

                    query = query.RecursiveInclude(includeItem);
                }
            }
            else if (exp is NewExpression nExp)                 //object ise (örnek: new { x.Classrooms, x.Students })
            {
                foreach (var includeItem in nExp.Arguments)//List<T,object> listesini döndürüyoruz
                {
                    query = query.RecursiveInclude(includeItem);
                }
            }
            return query;
        }

        //private static readonly MethodInfo IncludeMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo()
        //    .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include)).Single(mi => mi.GetParameters().Any(pi => pi.Name == "navigationPropertyPath") && !mi.ReturnType.Name.Equals("IQueryable`1"));

        //private static readonly MethodInfo IncludeAfterCollectionMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo()
        //    .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude)).Single(mi => !mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);

        //private static readonly MethodInfo IncludeAfterReferenceMethodInfo = typeof(EntityFrameworkQueryableExtensions).GetTypeInfo()
        //    .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude)).Single(mi => mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);

        //public static IQueryable<T> DeepMultipleIncludeWithString<T>(this IQueryable<T> source, params string[]? propertyPaths)
        //    where T : class
        //{
        //    if (propertyPaths is null || !propertyPaths.Any())
        //        return source;

        //    var entityType = typeof(T);
        //    object query = source;
        //    foreach (var propertyPath in propertyPaths)
        //    {
        //        Type prevPropertyType = null;
        //        foreach (var propertyName in propertyPath.Split('.'))
        //        {
        //            Type parameterType;
        //            MethodInfo method;
        //            if (prevPropertyType == null)
        //            {
        //                parameterType = entityType;
        //                method = IncludeMethodInfo;
        //            }
        //            else
        //            {
        //                parameterType = prevPropertyType;
        //                method = IncludeAfterReferenceMethodInfo;
        //                if (parameterType.IsConstructedGenericType && parameterType.GenericTypeArguments.Length == 1)
        //                {
        //                    var elementType = parameterType.GenericTypeArguments[0];
        //                    var collectionType = typeof(ICollection<>).MakeGenericType(elementType);
        //                    if (collectionType.IsAssignableFrom(parameterType))
        //                    {
        //                        parameterType = elementType;
        //                        method = IncludeAfterCollectionMethodInfo;
        //                    }
        //                }
        //            }
        //            var parameter = Expression.Parameter(parameterType, "e");
        //            var property = Expression.PropertyOrField(parameter, propertyName);
        //            if (prevPropertyType == null)
        //                method = method.MakeGenericMethod(entityType, property.Type);
        //            else
        //                method = method.MakeGenericMethod(entityType, parameter.Type, property.Type);
        //            query = method.Invoke(null, new object[] { query, Expression.Lambda(property, parameter) });
        //            prevPropertyType = property.Type;
        //        }
        //    }
        //    return (IQueryable<T>)query;
        //}
    }
}
