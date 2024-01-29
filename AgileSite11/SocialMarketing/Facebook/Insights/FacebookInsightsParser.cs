using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Parses Facebook API response for Insights request.
    /// </summary>
    internal class FacebookInsightsParser
    {
        #region "Fields"

        /// <summary>
        /// Contains Facebook Graph API response data section, or null if not present.
        /// </summary>
        private readonly ICollection<dynamic> mResponseData;


        /// <summary>
        /// Cache for latest end time which occurs in data.values.value of the response.
        /// Null value means the value has not been cached yet.
        /// DateTime.MinValue means the end time is not present in response (data section is probably empty).
        /// </summary>
        private DateTime? mLatestValueEndTime;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new instance of parser for given API response.
        /// </summary>
        /// <param name="facebookApiReponse">Facebook API response.</param>
        public FacebookInsightsParser(dynamic facebookApiReponse)
        {
            mResponseData = (ICollection<dynamic>)facebookApiReponse.data;
        }

        #endregion


        #region "Public methods"


        /// <summary>
        /// Tells you whether Facebook response contains data section with any data.
        /// </summary>
        /// <returns>True if data section exists and contains any data, false otherwise.</returns>
        public bool HasData()
        {
            if ((mResponseData != null) && (mResponseData.Count > 0))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Gets long value of specified Insight.
        /// </summary>
        /// <param name="name">Name of the Insight.</param>
        /// <param name="period">Period of the Insight.</param>
        /// <param name="endTime">End time of the Insight.</param>
        /// <returns>Long value, or null if not present.</returns>
        public long? GetLongValue(string name, string period, DateTime endTime)
        {
            dynamic dataItem = mResponseData.FirstOrDefault(it => StringExtensions.EqualsCSafe(name, it.name) && StringExtensions.EqualsCSafe(period, it.period));
            if (dataItem != null)
            {
                return GetValueFromValues(dataItem.values, endTime);
            }
            return null;
        }


        /// <summary>
        /// Gets all key-value pairs of specified Insight.
        /// Returns null, if specified Insight is not contained.
        /// </summary>
        /// <param name="name">Name of the Insight.</param>
        /// <param name="period">Period of the Insight.</param>
        /// <param name="endTime">End time of the Insight.</param>
        /// <returns>Structured value (string, object), or null if not present.</returns>
        public IDictionary<string, object> GetStructuredValue(string name, string period, DateTime endTime)
        {
            dynamic dataItem = mResponseData.FirstOrDefault(it => StringExtensions.EqualsCSafe(name, it.name) && StringExtensions.EqualsCSafe(period, it.period));
            if (dataItem != null)
            {
                return GetValueFromValues(dataItem.values, endTime) as IDictionary<string, object> ?? new Dictionary<string, object>();
            }
            return null;
        }


        /// <summary>
        /// Gets latest date time contained in data.values.end_date section.
        /// </summary>
        /// <param name="name">Name of the Insight.</param>
        /// <param name="period">Period of the Insight.</param>
        /// <returns>Latest date or DateTime.MinValue, if not present.</returns>
        public DateTime GetLatestDateTime(string name, string period)
        {
            if (mLatestValueEndTime.HasValue)
            {
                return mLatestValueEndTime.Value;
            }

            string endTimeString;
            DateTime? endTime;
            DateTime latestEndTime = DateTime.MinValue;
            dynamic dataItem = mResponseData.FirstOrDefault(it => StringExtensions.EqualsCSafe(name, it.name) && StringExtensions.EqualsCSafe(period, it.period));
            if (dataItem != null)
            {
                for (int j = 0; j < dataItem.values.Count; ++j)
                {
                    endTimeString = dataItem.values[j].end_time;
                    endTime = FacebookDateTimeStringToDateTime(endTimeString);
                    if (endTime.HasValue && (latestEndTime < endTime))
                    {
                        latestEndTime = endTime.Value;
                    }
                }
            }

            mLatestValueEndTime = latestEndTime;
            
            return latestEndTime;
        }


        /// <summary>
        /// Gets set of DateTime objects describing available end_time values.
        /// The set is union of all end_time values for given namesAndPeriods.
        /// </summary>
        /// <param name="namesAndPeriods">Array of pairs (name, period) for which to return DateTime</param>
        /// <returns>Set of DateTime with available end_time values.</returns>
        public SortedSet<DateTime> GetDateTimes(string[][] namesAndPeriods)
        {
            IDictionary<string, object> insight;
            SortedSet<DateTime> res = new SortedSet<DateTime>();

            foreach (dynamic item in mResponseData)
            {
                insight = item as IDictionary<string, object>;
                if (insight != null)
                {
                    if (namesAndPeriods.Any(i => StringExtensions.EqualsCSafe(i[0], item.name) && StringExtensions.EqualsCSafe(i[1], item.period)))
                    {
                        for (int i = 0; i < item.values.Count; ++i)
                        {
                            string endTimeString = item.values[i].end_time;
                            DateTime? endTime = FacebookDateTimeStringToDateTime(endTimeString);
                            if (endTime.HasValue)
                            {
                                res.Add(endTime.Value);
                            }
                        }
                    }
                }
            }
            return res;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Converts Facebook's date format (i.e. "2013-08-02T07:00:00+0000") to DateTime.
        /// </summary>
        /// <param name="dateTimeString">Facebook's date.</param>
        /// <returns>DateTime, or null if given string could not be converted</returns>
        private DateTime? FacebookDateTimeStringToDateTime(string dateTimeString)
        {
            DateTime d;
            if (DateTime.TryParse(dateTimeString, out d))
            {
                return d;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Gets value with certain end time from values section of the response.
        /// </summary>
        /// <param name="valuesSection">data.values section from the Facebook response</param>
        /// <param name="endTime">End time to search for</param>
        /// <returns>Value from values section with corresponding end time, or null if not present</returns>
        public dynamic GetValueFromValues(dynamic valuesSection, DateTime? endTime)
        {
            string processedEndTimeString;
            DateTime? processedEndTime;
            for (int i = 0; i < valuesSection.Count; ++i)
            {
                processedEndTimeString = valuesSection[i].end_time;
                processedEndTime = FacebookDateTimeStringToDateTime(processedEndTimeString);

                if (processedEndTime.HasValue && processedEndTime.Value == endTime)
                {
                    return valuesSection[i].value;
                }
            }

            return null;
        }

        #endregion
    }
}
