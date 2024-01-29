using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Inserts invisible span with "." because of RTL first character problem.
    /// </summary>
    [ToolboxData("<{0}:RTLFix runat=server />")]
    public class RTLfix : Label
    {
        #region "Variables"

        private bool mIsLiveSite = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether the control is rendered on live site or not.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return mIsLiveSite;
            }
            set
            {
                mIsLiveSite = value;
            }
        }

        #endregion


        #region "Overidden methods"

        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            EnableViewState = false;

            if (mIsLiveSite)
            {
                if (CultureHelper.IsPreferredCultureRTL())
                {
                    Style.Add("visibility", "hidden");
                    Text = ".";
                }
            }
            else
            {
                if (CultureHelper.IsUICultureRTL())
                {
                    Style.Add("visibility", "hidden");
                    Text = ".";
                }
            }
        }

        #endregion
    }
}