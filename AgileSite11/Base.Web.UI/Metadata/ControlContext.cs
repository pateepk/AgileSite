using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Context object contains properties with information about current context.
    /// </summary>
    public class ControlContext
    {
        #region "Constants"

        /// <summary>
        /// Widget properties.
        /// </summary>
        public const string WIDGET_PROPERTIES = "widgetproperties";

        /// <summary>
        /// Live site.
        /// </summary>
        public const string LIVE_SITE = "livesite";

        #endregion


        #region "Variables"

        private string mContextName = String.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the current context name.
        /// </summary>
        public string ContextName
        {
            get
            {
                return mContextName;
            }
            set
            {
                mContextName = value;
            }
        }

        #endregion
    }
}