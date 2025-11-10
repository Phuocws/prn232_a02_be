using System.Linq.Expressions;

namespace Application.Extensions.Queries
{
	public static class QueryableExtensions
	{
		public static IOrderedQueryable<T> ApplySorting<T>(
			this IQueryable<T> query,
			string? sortBy,
			bool descending)
		{
			if (string.IsNullOrWhiteSpace(sortBy))
				return query.OrderBy(e => 0); // default no-op sort to avoid exception

			var parameter = Expression.Parameter(typeof(T), "x");
			var property = typeof(T).GetProperty(sortBy);

			if (property == null)
				return query.OrderBy(e => 0); // fallback if prop not found

			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			var orderByExp = Expression.Lambda(propertyAccess, parameter);

			var methodName = descending ? "OrderByDescending" : "OrderBy";
			var resultExp = Expression.Call(
				typeof(Queryable),
				methodName,
				new Type[] { typeof(T), property.PropertyType },
				query.Expression,
				Expression.Quote(orderByExp));

			return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExp);
		}
	}
}
