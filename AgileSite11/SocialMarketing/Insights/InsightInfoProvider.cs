using System;
using System.Globalization;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.SocialMarketing
{

    /// <summary>
    /// Provides management of social marketing insights.
    /// </summary>
    public class InsightInfoProvider : AbstractInfoProvider<InsightInfo, InsightInfoProvider>
    {

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the InsightInfoProvider class.
        /// </summary>
        public InsightInfoProvider() : base(InsightInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Load = LoadHashtableEnum.None,
                    UseWeakReferences = true
                })
        {

        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Retrieves the insight with the specified identifier, and returns it.
        /// </summary>
        /// <param name="insightId">The insight identifier.</param>
        /// <returns>The insight with the specified identifier, if found; otherwise, null.</returns>
        public static InsightInfo GetInsight(int insightId)
        {
            return ProviderObject.GetInfoById(insightId);
        }

        
        /// <summary>
        /// Retrieves the insight matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <returns>The insight matching the specified criteria, if found; otherwise, null.</returns>
        public static InsightInfo GetInsight(string codeName, string externalId, string periodType)
        {
            return ProviderObject.GetInsightInternal(codeName, externalId, periodType);
        }


        /// <summary>
        /// Retrieves the partial insight matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <param name="valueName">The value name.</param>
        /// <returns>The partial insight matching the specified criteria, if found; otherwise, null.</returns>
        public static InsightInfo GetInsight(string codeName, string externalId, string periodType, string valueName)
        {
            return ProviderObject.GetInsightInternal(codeName, externalId, periodType, valueName);
        }


        /// <summary>
        /// Creates or updates the specified insight.
        /// </summary>
        /// <param name="insight">The insight to create or update.</param>
        public static void SetInsight(InsightInfo insight)
        {
            ProviderObject.SetInfo(insight);
        }


        /// <summary>
        /// Stores the value of the specified insight and comuptes the aggregated values.
        /// </summary>
        /// <param name="insight">The insight to store the value for.</param>
        /// <param name="periodDateTime">The local date and time that applies to the specified value.</param>
        /// <param name="retrieveHits">The function that returns the value for the specified parameters.</param>
        public static void UpdateHits(InsightInfo insight, DateTime periodDateTime, Func<InsightInfo, DateTime, Int64> retrieveHits)
        {
            ProviderObject.UpdateHitsInternal(insight, periodDateTime, retrieveHits);
        }


        /// <summary>
        /// Stores the value of the specified insight and comuptes the aggregated values.
        /// </summary>
        /// <param name="insight">The insight to store the value for.</param>
        /// <param name="periodDateTime">The local date and time that applies to the specified value.</param>
        /// <param name="hits">The value to store.</param>
        public static void UpdateHits(InsightInfo insight, DateTime periodDateTime, Int64 hits)
        {
            ProviderObject.UpdateHitsInternal(insight, periodDateTime, hits);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Retrieves the insight matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <returns>The insight matching the specified criteria, if found; otherwise, null.</returns>
        protected virtual InsightInfo GetInsightInternal(string codeName, string externalId, string periodType)
        {
            return GetInsightInternal(codeName, externalId, periodType, null);
        }


        /// <summary>
        /// Retrieves the partial insight matching the specified criteria, and returns it.
        /// </summary>
        /// <param name="codeName">The insight code name.</param>
        /// <param name="externalId">The external identifier of the object associated with the insight.</param>
        /// <param name="periodType">The type of the time period that the insight represents.</param>
        /// <param name="valueName">The value name.</param>
        /// <returns>The partial insight matching the specified criteria, if found; otherwise, null.</returns>
        protected virtual InsightInfo GetInsightInternal(string codeName, string externalId, string periodType, string valueName)
        {
            ObjectQuery<InsightInfo> query = GetObjectQuery().WhereEquals("InsightCodeName", codeName).WhereEquals("InsightExternalID", externalId).WhereEquals("InsightPeriodType", periodType);
            if (valueName != null)
            {
                query.WhereEquals("InsightValueName", valueName);
            }
            else
            {
                query.WhereNull("InsightValueName");
            }

            return query.SingleOrDefault();
        }


        /// <summary>
        /// Stores the value of the specified insight and comuptes the aggregated values.
        /// </summary>
        /// <param name="insight">The insight to store the value for.</param>
        /// <param name="periodDateTime">The local date and time that applies to the specified value.</param>
        /// <param name="retrieveHits">The function that returns the value for the specified parameters.</param>
        protected virtual void UpdateHitsInternal(InsightInfo insight, DateTime periodDateTime, Func<InsightInfo, DateTime, Int64> retrieveHits)
        {
            DataQuery query = new DataQuery
            {
                ClassName = "sm.insighthit_day"
            };
            query = query.WhereEquals("InsightHitInsightID", insight.InsightID).WhereEquals("InsightHitPeriodFrom", periodDateTime.ToLocalTime().Date);
            if (DataHelper.DataSourceIsEmpty(query.Result))
            {
                Int64 hits = retrieveHits(insight, periodDateTime);
                UpdateHitsInternal(insight, periodDateTime, hits);
            }
        }
        

        /// <summary>
        /// Stores the value of the specified insight and comuptes the aggregated values.
        /// </summary>
        /// <param name="insight">The insight to store the value for.</param>
        /// <param name="periodDateTime">The local date and time that applies to the specified value.</param>
        /// <param name="hits">The value to store.</param>
        protected virtual void UpdateHitsInternal(InsightInfo insight, DateTime periodDateTime, Int64 hits)
        {
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int databaseFirstDayOfWeek = (int)firstDayOfWeek;
            if (firstDayOfWeek == DayOfWeek.Sunday)
            {
                databaseFirstDayOfWeek = 7;
            }

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@insightID", insight.InsightID);
            parameters.Add("@periodDateTime", periodDateTime.ToLocalTime());
            parameters.Add("@value", hits);
            parameters.Add("@periodType", insight.InsightPeriodType);
            parameters.Add("@firstDayOfWeek", databaseFirstDayOfWeek);

            using (var scope = BeginTransaction())
            {
                ConnectionHelper.ExecuteQuery("sm.insight.updatehits", parameters);
                scope.Commit();
            }
        }

        #endregion

    }

}