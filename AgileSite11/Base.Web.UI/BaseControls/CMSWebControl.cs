using System;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.PortalEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Web control.
    /// </summary>
    public abstract class CMSWebControl : WebControl, IShortID
    {
        #region "Variables"

        private bool? mIsLiveSite = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Short ID of the control.
        /// </summary>
        public string ShortID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        public virtual bool IsLiveSite
        {
            get
            {
                if (mIsLiveSite == null)
                {
                    IAdminPage page = Page as IAdminPage;
                    mIsLiveSite = (page == null);

                    // Try to get the property value from parent controls
                    mIsLiveSite = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.IsLiveSite, mIsLiveSite.Value);
                }

                return mIsLiveSite.Value;
            }
            set
            {
                mIsLiveSite = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSWebControl()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        public CMSWebControl(HtmlTextWriterTag tag)
            : base(tag)
        {
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }

        #endregion
    }
}