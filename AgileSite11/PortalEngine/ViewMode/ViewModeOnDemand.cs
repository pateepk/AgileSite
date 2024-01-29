using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Encapsulates the view mode but doesn't request it until it is demanded by Value.
    /// </summary>
    public class ViewModeOnDemand
    {
        private ViewModeEnum mValue = ViewModeEnum.Unknown;


        /// <summary>
        /// Value.
        /// </summary>
        public ViewModeEnum Value
        {
            get
            {
                // Get view mode from context
                if (mValue == ViewModeEnum.Unknown)
                {
                    mValue = PortalContext.ViewMode;
                }

                return mValue;
            }
            set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Converts the view mode to its enum representation
        /// </summary>
        /// <param name="viewMode">View mode</param>
        public static implicit operator ViewModeEnum(ViewModeOnDemand viewMode)
        {
            if (viewMode == null)
            {
                return ViewModeEnum.LiveSite;
            }

            return viewMode.Value;
        }


        /// <summary>
        /// Returns true if the view mode is live site mode
        /// </summary>
        public bool IsLiveSite()
        {
            return Value.IsLiveSite();
        }
    }
}