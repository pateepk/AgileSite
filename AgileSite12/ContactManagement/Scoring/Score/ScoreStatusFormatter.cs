using System;

using CMS.Helpers.Markup;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Returns formatted text for score status.
    /// </summary>
    public class ScoreStatusFormatter
    {
        #region "Variables and properties"

        private readonly ScoreInfo scoreInfo;

        /// <summary>
        /// Optional. URL of the dialog for recalculation details.
        /// </summary>
        public string RecalculationURL
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if tooltips should be displayed.
        /// </summary>
        public bool DisplayTooltips
        {
            get;
            set;
        }

        #endregion


        #region "Public methods and constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scoreInfo">Score info object.</param>
        public ScoreStatusFormatter(ScoreInfo scoreInfo)
        {
            if (scoreInfo == null)
            {
                throw new ArgumentNullException("scoreInfo");
            }

            this.scoreInfo = scoreInfo;
        }


        /// <summary>
        /// Gets formatted score status. Score can be disabled, it can be scheduled to rebuild in the future or its status is one of <see cref="ScoreStatusEnum"/>.
        /// </summary>
        public FormattedText GetFormattedStatus()
        {
            // "Disabled" status
            if (!scoreInfo.ScoreEnabled)
            {
                return GetDisabledStatusText();
            }

            // "Recalculation scheduled" status
            if (scoreInfo.ScoreScheduledTaskID > 0)
            {
                TaskInfo taskInfo = TaskInfoProvider.GetTaskInfo(scoreInfo.ScoreScheduledTaskID);
                if ((taskInfo != null) && taskInfo.TaskEnabled)
                {
                    return GetScheduledStatusText(taskInfo);
                }
            }

            // Other statuses
            switch (scoreInfo.ScoreStatus)
            {
                case ScoreStatusEnum.Ready:
                    return GetEnabledStatusText();
                case ScoreStatusEnum.RecalculationRequired:
                    return GetRecalculationRequiredStatusText();
                case ScoreStatusEnum.Recalculating:
                    return GetRecalculatingStatusText();
                case ScoreStatusEnum.RecalculationFailed:
                    return GetFailedStatusText();
                default:
                case ScoreStatusEnum.Unspecified:
                    throw new InvalidOperationException("[ScoreStatusFromatter.GetFormattedStatus]: Score status not specified.");
            }
        }

        #endregion


        #region "Private methods"

        private FormattedText GetDisabledStatusText()
        {
            return new FormattedText()
                .SetText(ResHelper.GetString("general.disabled"))
                .ColorRed();
        }


        private FormattedText GetScheduledStatusText(TaskInfo taskInfo)
        {
            var text = new FormattedText();
            string nextRunTime = taskInfo.TaskNextRunTime.ToString();
            text.SetText(String.Format(ResHelper.GetString("om.score.recalculatescheduledat2"), nextRunTime))
                .ColorOrange();
            if (DisplayTooltips)
            {
                text.AddTooltip(String.Format(ResHelper.GetString("om.score.clicktoreschedule"), nextRunTime));
            }
            AddRecalculationURL(text);
            return text;
        }


        private static FormattedText GetEnabledStatusText()
        {
            return new FormattedText()
                .SetText(ResHelper.GetString("general.enabled"))
                .ColorGreen();
        }


        private FormattedText GetRecalculationRequiredStatusText()
        {
            var text = new FormattedText();
            text.SetText(ResHelper.GetString("om.score.recalcrequired"))
                .ColorOrange();
            if (DisplayTooltips)
            {
                text.AddTooltip(ResHelper.GetString("om.score.setrecalculation"));
            }
            AddRecalculationURL(text);
            return text;
        }


        private FormattedText GetRecalculatingStatusText()
        {
            return new FormattedText()
                .SetText(ResHelper.GetString("om.score.recalculating"))
                .ColorOrange();
        }


        private FormattedText GetFailedStatusText()
        {
            var text = new FormattedText();
            text.SetText(ResHelper.GetString("om.score.recalcfailed"))
                .ColorRed();
            if (DisplayTooltips)
            {
                text.AddTooltip(ResHelper.GetString("general.seeeventlog"));
            }
            return text;
        }


        private void AddRecalculationURL(FormattedText text)
        {
            if (!String.IsNullOrEmpty(RecalculationURL))
            {
                text.OnClientClick("modalDialog('" + RecalculationURL + @"', '', 660, 320);");
            }
        }

        #endregion
    }
}
