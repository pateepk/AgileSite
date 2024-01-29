using System;

using CMS.DataEngine;

namespace CMS.ContactManagement.Internal
{
	/// <summary>
	/// Extensions of <see cref="WhereConditionBase{TQuery}"/> for usage mainly with <see cref="ObjectQuery{ScoreContactRuleInfo}"/> or as a chained condition with
	/// another <see cref="WhereConditionBase{TQuery}"/> considering instances of <see cref="ScoreContactRuleInfo"/>.
	/// </summary>
	public static class ScoreContactRuleInfoObjectQueryExtensions
	{
		/// <summary>
		/// Filters out objects which have their 'Expiration' column's day (e.g. <see cref="ScoreContactRuleInfo.Expiration"/>) before 
		/// <paramref name="currentDateTime" />.
		/// </summary>
		/// <typeparam name="TQuery">Type of the query</typeparam>
		/// <param name="whereCondition">A query which has the expiration filter applied.</param>
		/// <param name="currentDateTime">A date which the 'Expiration' column has to exceed for the object to be considered expired and filtered out.</param>
		/// <returns>Query with the applied expiration filter.</returns>
		public static TQuery WhereNotExpired<TQuery>(this WhereConditionBase<TQuery> whereCondition, DateTime currentDateTime) 
			where TQuery : WhereConditionBase<TQuery>, new()
		{
			string dateTimeSQLFormat = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
			string dayDifferenceCondition = String.Format("DATEDIFF(day, '{0}', Expiration) >= 0", dateTimeSQLFormat);

			return whereCondition.Where(w => w.WhereNull("Expiration")
											  .Or()
											  .Where(dayDifferenceCondition));
		}
	}
}