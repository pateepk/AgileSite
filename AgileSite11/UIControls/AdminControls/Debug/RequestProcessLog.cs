using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Cache log base class.
    /// </summary>
    public class RequestProcessLog : LogControl
    {
        #region "Variables"

        private DateTime firstTime = DateTime.MinValue;
        private DateTime lastTime = DateTime.MinValue;

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return RequestDebug.Settings;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the time from the first action.
        /// </summary>
        /// <param name="time">Time of the action</param>
        protected string GetFromStart(object time)
        {
            DateTime t = ValidationHelper.GetDateTime(time, DateTime.MinValue);
            if (firstTime == DateTime.MinValue)
            {
                firstTime = t;
            }

            lastTime = t;
            TotalDuration = t.Subtract(firstTime).TotalSeconds;

            return TotalDuration.ToString("F3");
        }

        #endregion
    }
}