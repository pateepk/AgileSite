using System;

using CMS.Helpers;
using CMS.Helpers.Markup;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Evaluates status of an AB test based on its start and finish dates and times.
    /// </summary>
    public static class ABTestStatusEvaluator
    {
        #region "Public methods"

        /// <summary>
        /// Checks whether the AB test is running.
        /// </summary>
        public static bool ABTestIsRunning(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            return GetStatus(abTest) == ABTestStatusEnum.Running;
        }


        /// <summary>
        /// Gets status of the AB test.
        /// </summary>
        public static ABTestStatusEnum GetStatus(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            DateTime start = abTest.ABTestOpenFrom;
            DateTime finish = abTest.ABTestOpenTo;

            if (start == DateTimeHelper.ZERO_TIME)
            {
                return ABTestStatusEnum.NotStarted;
            }

            if ((finish != DateTimeHelper.ZERO_TIME) && (finish <= DateTime.Now))
            {
                return ABTestStatusEnum.Finished;
            }

            if (start > DateTime.Now)
            {
                return ABTestStatusEnum.Scheduled;
            }

            return ABTestStatusEnum.Running;
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Gets formatted status of the AB test.
        /// </summary>
        /// <param name="status">AB test status</param>
        public static FormattedText GetFormattedStatus(ABTestStatusEnum status)
        {
            var text = new FormattedText(ResHelper.GetString("abtesting.status" + status));

            switch (status)
            {
                case ABTestStatusEnum.Running:
                    return text.ColorGreen();

                case ABTestStatusEnum.Scheduled:
                    return text.ColorOrange();

                case ABTestStatusEnum.Finished:
                    return text;

                case ABTestStatusEnum.NotStarted:
                    return text.ColorRed();
            }

            return text;
        }

        #endregion
    }
}