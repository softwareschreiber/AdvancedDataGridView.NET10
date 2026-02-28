using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/*
 found on: http://stackoverflow.com/questions/2841585/create-linq-to-entities-orderby-expression-on-the-fly from Jon Skeet
 modified by Hartmut Schreiber http://www.HartmutSchreiber.com
 */
namespace Zuby
{
	public static class SortLambdaBuilder
	{
		/// <summary>
		/// Calls the order by.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static IQueryable<TSource> CallOrderBy<TSource>
			(IQueryable<TSource> source, string propertyName, Zuby.SortType sortType)
		{
			ParameterExpression parameter = Expression.Parameter(typeof(TSource), "posting");
			Expression orderByProperty = Expression.Property(parameter, propertyName);

			LambdaExpression lambda = Expression.Lambda(orderByProperty, new[] { parameter });
			MethodInfo genericMethod;
			if (sortType == SortType.ASC)
			{
				genericMethod = OrderByMethod.MakeGenericMethod
					(new[] { typeof(TSource), orderByProperty.Type });
			} 
			else
			{
				genericMethod = OrderByDescendMethod.MakeGenericMethod
					(new[] { typeof(TSource), orderByProperty.Type });
			}
			object ret = genericMethod.Invoke(null, new object[] { source, lambda });
			return (IQueryable<TSource>)ret;
		}

		private static readonly MethodInfo OrderByMethod =
												typeof(Queryable).GetMethods()
													.Where(method => method.Name == "OrderBy")
													.Where(method => method.GetParameters().Length == 2)
													.Single();

		private static readonly MethodInfo OrderByDescendMethod =
												typeof(Queryable).GetMethods()
													.Where(method => method.Name == "OrderByDescending")
													.Where(method => method.GetParameters().Length == 2)
													.Single();
	}
}
