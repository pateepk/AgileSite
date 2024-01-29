using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Link button control that generates appropriate HTML for CMS design.
    /// </summary>
    public class CMSAccessibleLinkButton : LinkButton
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the CSS class that serves as icon for the button.
        /// </summary>
        public string IconCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the description for screen readers.
        /// </summary>
        public string ScreenReaderDescription
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders content.
        /// </summary>
        /// <param name="writer">HTML text writer.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            var iconString = String.Format("<i aria-hidden=\"true\" class=\"{0}\" id=\"{1}\"></i>", IconCssClass, ClientID + "_icon");
            var descString = String.Format("<span class=\"sr-only\">{0}</span>", ScreenReaderDescription);

            writer.Write(iconString);
            writer.Write(descString);
        }

        #endregion
    }
}
