using System;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Handlers log control for debug purposes.
    /// </summary>
    public class HandlersLog : LogControl
    {
        #region "Properties"
        
        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return HandlersDebug.Settings;
            }
        }
        
        #endregion
    }
}