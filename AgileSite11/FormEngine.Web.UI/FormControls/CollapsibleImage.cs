using System;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Collapsible image control. Used for collapsing categories.
    /// </summary>
    public class CollapsibleImage : CompositeControl
    {
        #region "Variables"

        private readonly Image mImage = new Image { ID = "CollapseImage", EnableViewState = false};
        private readonly HiddenField mHiddenField = new HiddenField { ID = "HdnCollapseValue" };

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner state of control.
        /// </summary>
        public bool Collapsed
        {
            get
            {
                EnsureChildControls();
                string state = string.IsNullOrEmpty(mHiddenField.Value) ? Page.Request.Params[mHiddenField.UniqueID] : mHiddenField.Value;
                return ValidationHelper.GetBoolean(state, CollapsedByDefault);
            }
            set
            {
                mHiddenField.Value = value.ToString();
            }
        }


        /// <summary>
        /// Initial collapsed value.
        /// </summary>
        public bool CollapsedByDefault
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the location of an image to display in the System.Web.UI.WebControls.Image control.
        /// </summary>
        public String ImageUrl
        {
            get
            {
                return mImage.ImageUrl;
            }
            set
            {
                mImage.ImageUrl = value;
            }
        }


        /// <summary>
        /// Gets or sets the Cascading Style Sheet (CSS) class rendered by the Web server
        /// control on the client.
        /// </summary>
        public override String CssClass
        {
            get
            {
                return mImage.CssClass;
            }
            set
            {
                mImage.CssClass = value;
            }
        }


        /// <summary>
        /// Gets or sets the alternate text displayed in the System.Web.UI.WebControls.Image
        /// control when the image is unavailable. Browsers that support the ToolTips
        /// feature display this text as a ToolTip.
        /// </summary>
        public String AlternateText
        {
            get
            {
                return mImage.AlternateText;
            }
            set
            {
                mImage.AlternateText = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Add(mImage);
            Controls.Add(mHiddenField);
        }


        /// <summary>
        /// OnPreRender event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            // Set value of hidden field even on first load
            mHiddenField.Value = Collapsed.ToString();
            if (Collapsed)
            {
                this.AddCssClass("Collapsed");
            }
            else
            {
                this.RemoveCssClass("Collapsed");
            }

            this.AddCssClass("ToggleImage");
        }

        #endregion
    }
}