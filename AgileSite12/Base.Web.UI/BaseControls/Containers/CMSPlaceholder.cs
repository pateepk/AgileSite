using System;
using System.Text;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;
using CMS.PortalEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Placeholder.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSPlaceHolder : PlaceHolder, IShortID
    {
        #region "Variables"

        private bool? mIsLiveSite = null;
        private StringBuilder sb = null;

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
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            FinishCurrent();

            base.OnPreRender(e);
        }


        /// <summary>
        /// Appends the given control to the placeholder
        /// </summary>
        /// <param name="ctrl">control to append</param>
        public void Append(Control ctrl)
        {
            FinishCurrent();
            Controls.Add(ctrl);
        }


        /// <summary>
        /// Appends the given content to the placeholder
        /// </summary>
        /// <param name="content">Content to append</param>
        public void Append(params string[] content)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }
            
            // Add all given contents
            foreach (var c in content)
            {
                sb.Append(c);
            }
        }


        /// <summary>
        /// Appends the given content to the placeholder
        /// </summary>
        protected void FinishCurrent()
        {
            if ((sb != null) && (sb.Length > 0))
            {
                Controls.Add(new LiteralControl(sb.ToString()));

                sb.Clear();
            }
        }


        /// <summary>
        /// Clears the placeholder content
        /// </summary>
        public void Clear()
        {
            Controls.Clear();

            if (sb != null)
            {
                sb.Clear();
            }
        }

        #endregion
    }
}