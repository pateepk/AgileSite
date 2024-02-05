using System.Collections.Generic;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;

namespace CMS.ContactManagement.Web.UI
{
	/// <summary>
	/// Provides manipulation of contact's scoring view models.
	/// </summary>
	internal class ContactScoringService : IContactScoringService
	{
		/// <summary>
		/// Retrieves instances of <see cref="ContactScoringViewModel"/> for a contact.
		/// </summary>
		/// <param name="contactId">ID of a contact for which the view models are retrieved.</param>
		/// <returns>Scoring view models for the given contact.</returns>
		public IEnumerable<ContactScoringViewModel> GetScoringViewModel(int contactId)
		{
			var dateTimeService = Service.Resolve<IDateTimeNowService>();

			var scoringPointsQuery = ScoreContactRuleInfoProvider.GetScoreContactRules()
																 .Columns(
																 	 new QueryColumn("ScoreDisplayName"), 
																 	 new AggregatedColumn(AggregationType.Sum, "Value").As("Score"))
																 .Source(s => s.RightJoin<ScoreInfo>("ScoreID", "ScoreID", 
																	new WhereCondition()
																		.WhereEquals("ContactID", contactId)
																		.WhereNotExpired(dateTimeService.GetDateTimeNow())))
																 .WhereFalse("ScoreBelongsToPersona")
																 .WhereTrue("ScoreEnabled")
																 .GroupBy("OM_Score.ScoreID", "ScoreDisplayName")
																 .OrderBy("ScoreDisplayName");

			return scoringPointsQuery.Select(dataRow => new ContactScoringViewModel()
			{
				Name = dataRow["ScoreDisplayName"].ToString(""),
				Points = dataRow["Score"].ToInteger(0)
			});
		}
	}
}