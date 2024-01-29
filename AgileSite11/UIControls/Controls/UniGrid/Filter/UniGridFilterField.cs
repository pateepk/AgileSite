using System;
using System.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents a field of UniGrid's filter.
    /// </summary>
    public class UniGridFilterField
    {
        #region "Properties"
        
        /// <summary>
        /// Filter value control (textbox or custom filter control).
        /// </summary>
        public Control ValueControl
        {
            get;
            set;
        }
        
        #endregion
    }
}
