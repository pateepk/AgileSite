using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Base class for overview UI pages of AB test.
    /// </summary>
    public abstract class CMSABTestOverviewPage : CMSABTestPage
    {
        #region "Variables"

        /// <summary>
        /// Current AB test.
        /// </summary>
        private ABTestInfo mABTest;


        /// <summary>
        /// Gets status of the current test.
        /// </summary>
        private ABTestStatusEnum? mTestStatus = null;


        /// <summary>
        /// AB Variant performance calculator.
        /// </summary>
        protected IABVariantPerformanceCalculator mVariantPerformanceCalculator;


        /// <summary>
        /// The minimum lower bound of the conversion rate intervals for the test.
        /// </summary>
        protected double mMinConversionRateLowerBound;


        /// <summary>
        /// The difference between the minimum lower bound and the maximum upper bound values for the test.
        /// </summary>
        protected double mConversionRateRange;


        /// <summary>
        /// Class that writes info messages into the page.
        /// </summary>
        protected ABTestMessagesWriter mMessagesWriter;


        /// <summary>
        /// Indicates whether the user is authorized to manage the test.
        /// </summary>
        protected bool? mIsUserAuthorizedToManageTest;

        #endregion


        #region "Constants"

        /// <summary>
        /// Max length of a link that is in the upper right box - long links can break that table.
        /// </summary>
        protected const int MAX_LINK_LENGTH = 22;


        /// <summary>
        /// Minimum of conversions to mark a variant as winning.
        /// </summary>
        protected const int WINNING_VARIANT_MIN_CONVERSIONS = 25;


        /// <summary>
        /// Minimum chance to beat to mark a variant as winning.
        /// </summary>
        protected const double WINNING_VARIANT_MIN_CHANCETOBEAT = 0.95d;


        /// <summary>
        /// Resource string of the show advanced filters text.
        /// </summary>
        protected const string SHOW_FILTERS_TEXT = "abtesting.showfilters";


        /// <summary>
        /// Resource string of the hide advanced filters text.
        /// </summary>
        protected const string HIDE_FILTERS_TEXT = "abtesting.hidefilters";


        /// <summary>
        /// Code name of the conversions over visits metric.
        /// </summary>
        protected const string METRIC_CONVERSIONS_OVER_VISITS = "conversionsovervisits";


        /// <summary>
        /// Code name of the chance to beat original metric.
        /// </summary>
        protected const string METRIC_CHANCE_TO_BEAT_ORIGINAL = "chancetobeatoriginal";


        /// <summary>
        /// Code name of the conversion rate metric.
        /// </summary>
        protected const string METRIC_CONVERSION_RATE = "conversionrate";


        /// <summary>
        /// Code name of the conversion value metric.
        /// </summary>
        protected const string METRIC_CONVERSION_VALUE = "conversionvalue";


        /// <summary>
        /// Code name of the average conversion value metric.
        /// </summary>
        protected const string METRIC_AVG_CONVERSION_VALUE = "averageconversionvalue";


        /// <summary>
        /// Code name of the conversion count metric.
        /// </summary>
        protected const string METRIC_CONVERSION_COUNT = "conversioncount";


        /// <summary>
        /// Names of sampling selector actions.
        /// </summary>
        protected string[] SamplingActions = { "day", "week", "month" };


        /// <summary>
        /// Names of data format selector actions.
        /// </summary>
        protected string[] DataFormatActions = { "cumulative", "daywise" };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current AB test.
        /// </summary>
        protected ABTestInfo ABTest
        {
            get
            {
                return mABTest ?? (mABTest = ABTestInfoProvider.GetABTestInfo(QueryHelper.GetInteger("objectid", 0)));
            }
        }


        /// <summary>
        /// Gets status of the current test.
        /// </summary>
        protected ABTestStatusEnum TestStatus
        {
            get
            {
                if (!mTestStatus.HasValue)
                {
                    mTestStatus = ABTestStatusEvaluator.GetStatus(ABTest);
                }

                return mTestStatus.Value;
            }
        }


        /// <summary>
        /// Gets class that writes info messages into the page.
        /// </summary>
        protected ABTestMessagesWriter MessagesWriter
        {
            get
            {
                return mMessagesWriter ?? (mMessagesWriter = new ABTestMessagesWriter(ShowMessage));
            }
        }


        /// <summary>
        /// Gets key of the cookie saving important selectors' state.
        /// </summary>
        protected string SelectorsCookieKey
        {
            get
            {
                return CookieName.ABSelectorStatePrefix + ABTest.ABTestName;
            }
        }


        /// <summary>
        /// Indicates whether the user is authorized to finish the test.
        /// </summary>
        protected bool IsUserAuthorizedToManageTest
        {
            get
            {
                if (!mIsUserAuthorizedToManageTest.HasValue)
                {
                    SiteInfo site = SiteInfoProvider.GetSiteInfo(ABTest.ABTestSiteID);
                    mIsUserAuthorizedToManageTest = CurrentUser.IsAuthorizedPerResource(ModuleName.ABTEST, "Manage", site.SiteName);
                }

                return mIsUserAuthorizedToManageTest.Value;
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Sets CSS class of a parent of given control.
        /// </summary>
        /// <param name="control">Child control that has a parent that's being changed</param>
        /// <param name="cssClass">CSS class to change parent to</param>
        protected static void AddCSSToParentControl(Control control, string cssClass)
        {
            var row = control?.Parent as WebControl;
            row?.AddCssClass(cssClass);
        }


        /// <summary>
        /// Returns where condition which specifies selected conversion.
        /// </summary>
        /// <param name="conversion">CSS class to change parent to</param>
        /// <param name="testConversions">All names of conversions assigned to the AB test</param>
        protected string GetConversionCondition(string conversion, IEnumerable<string> testConversions)
        {
            if (!String.IsNullOrEmpty(conversion))
            {
                return "StatisticsObjectName = N'" + SqlHelper.EscapeQuotes(conversion) + "'";
            }
            // User has selected 'all' in conversion selector. Try to select only those conversions, that belong to the test (that are set in settings tab)
            if (testConversions.Any())
            {
                return SqlHelper.GetWhereCondition("StatisticsObjectName", testConversions);
            }
            return null;
        }


        /// <summary>
        /// Gets finish date of the AB test or current date if the date is empty.
        /// </summary>
        protected DateTime GetFinishDate()
        {
            if ((ABTest.ABTestOpenTo < DateTime.Now) && (ABTest.ABTestOpenTo != DateTimeHelper.ZERO_TIME))
            {
                return ABTest.ABTestOpenTo;
            }
            return DateTime.Now;
        }


        /// <summary>
        /// Checks whether there is test data for filters specified by the user.
        /// </summary>
        /// <param name="statisticsData">Statistics data</param>
        /// <param name="successMetric">Success metric</param>
        protected bool DataAvailable(IEnumerable<ABVariantStatisticsData> statisticsData, string successMetric)
        {
            bool dataAvailable = false;

            // Search data for all AB variants
            foreach (var data in statisticsData)
            {
                // If we have visits we have data for conversion rate
                if (successMetric == METRIC_CONVERSION_RATE)
                {
                    if (data.Visits > 0)
                    {
                        dataAvailable = true;
                        break;
                    }
                }
                // For other success metrics we need conversions
                else if (data.ConversionsCount > 0)
                {
                    dataAvailable = true;
                    break;
                }
            }
            return dataAvailable;
        }


        /// <summary>
        /// Shortens URL, adds dots and rounds URL length to the nearest slash occurrence.
        /// </summary>
        /// <returns>Unchanged URL, if <paramref name="maxLength"/> doesn't exceed length of <paramref name="url"/> or shortened URL in format '../something/something'</returns>
        protected string ShortenUrl(string url, int maxLength = MAX_LINK_LENGTH)
        {
            // No need to shorten
            if (url.Length <= maxLength)
            {
                return url;
            }

            // Get last index that can be kept
            int maxIndex = url.Length - maxLength;

            // Get index of last slash, so that URL will start with that
            var indexOfSlash = url.IndexOf('/', maxIndex);
            if (indexOfSlash != -1)
            {
                return ".." + url.Substring(indexOfSlash);
            }

            // There wasn't any slash found, so return just shortened URL
            return ".." + url.Substring(maxIndex);
        }


        /// <summary>
        /// Returns panel with information about improvement.
        /// </summary>
        /// <param name="improvement">Improvement</param>
        protected Panel GetPercentageImprovementPanel(double improvement)
        {
            var panel = new Panel();

            if (!improvement.Equals(0d))
            {
                // Add picture representing improvement
                string iconClass = (improvement > 0d ? "green-arrow" : "red-arrow");
                string tooltip = (improvement > 0d ? "abtesting.overview.increase" : "abtesting.overview.decrease");

                panel.Controls.Add(new LiteralControl(UIHelper.GetAccessibleIconTag(iconClass, GetString(tooltip))));
            }

            // Add text representing improvement
            panel.Controls.Add(new Label
            {
                Text = String.Format(" {0:P2}", improvement),
            });

            return panel;
        }


        /// <summary>
        /// Returns hits for specified codename.
        /// </summary>
        /// <param name="codename">Statistics codename</param>
        /// <param name="columns">Selected columns</param>
        /// <param name="culture">Culture</param>
        /// <param name="where">Additional where condition</param>
        protected DataRow GetHits(string codename, string columns, string culture, string where = null)
        {
            return HitsInfoProvider.GetAllHitsInfo(SiteContext.CurrentSiteID, HitsIntervalEnum.Year, codename, columns, culture, where).Tables[0].Rows[0];
        }


        /// <summary>
        /// Creates a DataTable object with predefined columns.
        /// </summary>
        /// <returns></returns>
        protected DataTable InitGraphColumns()
        {
            var data = new DataTable();

            data.Columns.Add("FromDate", typeof(DateTime));
            data.Columns.Add("ToDate", typeof(DateTime));
            data.Columns.Add("TestName", typeof(string));
            data.Columns.Add("ConversionName", typeof(string));
            data.Columns.Add("GraphType", typeof(string));
            data.Columns.Add("ABTestID", typeof(int));
            data.Columns.Add("VariationName", typeof(string));
            data.Columns.Add("ABTestCulture", typeof(string));
            data.Columns.Add("ConversionType", typeof(string));

            return data;
        }


        /// <summary>
        /// Initializes sampling button group in radio button panel.
        /// </summary>
        protected void InitSamplingButtons(CMSButtonGroup buttonGroup)
        {
            buttonGroup.Actions.Add(new CMSButtonGroupAction
            {
                Name = SamplingActions[0],
                Text = GetString("general.day")
            });

            buttonGroup.Actions.Add(new CMSButtonGroupAction
            {
                Name = SamplingActions[1],
                Text = GetString("general.week")
            });

            buttonGroup.Actions.Add(new CMSButtonGroupAction
            {
                Name = SamplingActions[2],
                Text = GetString("general.month")
            });
        }


        /// <summary>
        /// Initializes graph data button group in radio button panel.
        /// </summary>
        protected void InitGraphDataButtons(CMSButtonGroup buttonGroup)
        {
            buttonGroup.Actions.Add(new CMSButtonGroupAction
            {
                Name = DataFormatActions[0],
                Text = GetString("abtesting.rate.cumulative")
            });

            buttonGroup.Actions.Add(new CMSButtonGroupAction
            {
                Name = DataFormatActions[1],
                Text = GetString("abtesting.rate.daywise")
            });
        }

        #endregion
    }
}