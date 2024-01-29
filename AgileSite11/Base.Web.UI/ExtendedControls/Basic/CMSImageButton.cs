using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMS Image Button with additional functionality.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSImageButton : ImageButton
    {
        /// <summary>
        /// Image URL of the button.
        /// </summary>
        public override string ImageUrl
        {
            get
            {
                return base.ImageUrl;
            }
            set
            {
                base.ImageUrl = value;

                // Reset the src attribute if image URL set
                Attributes.Remove("src");
            }
        }


        /// <summary>
        /// Renders the button.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (!IsEnabled)
            {
                // Add opacity class
                this.AddCssClass("ButtonDisabled");
            }

            base.Render(writer);
        }
    }
}