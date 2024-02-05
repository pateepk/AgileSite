using System;

using CMS.Base;
using CMS.OutputFilter;

namespace CMS.UIControls
{
    /// <summary>
    /// Output log base class.
    /// </summary>
    public class OutputLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return OutputDebug.Settings;
            }
        }

        #endregion
    }
}