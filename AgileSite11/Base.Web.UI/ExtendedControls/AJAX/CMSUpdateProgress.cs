using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Progress notification
    /// </summary>
    [ToolboxData("<{0}:CMSUpdateProgress runat=server />")]
    public class CMSUpdateProgress : PlaceHolder
    {
        #region "Properties"

        /// <summary>
        /// Progress text
        /// </summary>
        public string ProgressText
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the progress bar is displayed also on postback
        /// </summary>
        public bool HandlePostback
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the progress bar is displayed also on async postback. Note, that the update progress must be in update panel in this case to hid properly.
        /// </summary>
        public bool HandleAsyncPostback
        {
            get;
            set;
        }


        /// <summary>
        /// Number of milliseconds after which the progress should be displayed
        /// </summary>
        public int DisplayTimeout
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSUpdateProgress()
        {
            DisplayTimeout = 1000;
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!ScriptHelper.AllowProgressScript || !UIHelper.AllowUpdateProgress)
            {
                Visible = false;
            }

            if (Visible)
            {
                // Register the scripts
                ScriptHelper.RegisterLoader(Page, ProgressText);

                if (HandlePostback)
                {
                    Page.Form.Attributes.Add("onsubmit", String.Format("if (window.Loader) {{ window.Loader.submitForm({0}, {1}); }}; return true;", DisplayTimeout, HandleAsyncPostback.ToString().ToLowerInvariant()));
                }
            }
        }

        #endregion
    }
}
