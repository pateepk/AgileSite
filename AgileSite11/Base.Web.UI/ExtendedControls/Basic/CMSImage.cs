using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMS Image with support for disabled state.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSImage : Image
    {
        #region "Variables"

        private string mDisabledImageUrl = String.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets path to image which should be used for disabled image state.
        /// </summary>
        public string DisabledImageUrl
        {
            get
            {
                return mDisabledImageUrl;
            }
            set
            {
                mDisabledImageUrl = value;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// PreRender event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!IsEnabled && (DisabledImageUrl != String.Empty))
            {
                ImageUrl = DisabledImageUrl;
            }
        }

        #endregion
    }
}