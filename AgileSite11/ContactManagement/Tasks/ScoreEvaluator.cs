using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Task for score recalculation. For internal purposes only, for manual run please use ScoreAsyncRecalculator instead.
    /// Internally calls ScoreAsyncRecalculator thus recalculation runs in another thread.
    /// </summary>
    public class ScoreEvaluator : ITask
    {
        #region "Methods"

        /// <summary>
        /// Evaluates the membership of contacts to given contact group or persona.
        /// </summary>
        /// <param name="task">Task to process</param>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is null</exception>
        public string Execute(TaskInfo task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            // Check license
            if (!string.IsNullOrEmpty(DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty)))
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.LeadScoring);
            }

            int scoreId = ValidationHelper.GetInteger(task.TaskData, 0);
            var score = ScoreInfoProvider.GetScoreInfo(scoreId);
            
            if (score == null)
            {
                return ResHelper.GetString("om.score.taskdatanotprovided");
            }

            var asyncEvaluator = new ScoreAsyncRecalculator(score);
            asyncEvaluator.RunAsync();

            return string.Empty;
        }

        #endregion
    }
}