using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button that generates appropriate HTML code for its use in actions container.
    /// </summary>
    public class CMSAccessibleButton : CMSAccessibleButtonBase
    {
        /// <summary>
        /// If true, the button is the only-image button. The button is then without a button-like background and only image is dispayed. I.e. it adds "btn-icon" class.
        /// </summary>
        public virtual bool IconOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the description for screen readers.
        /// </summary>
        public virtual string ScreenReaderDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the style of the button.
        /// </summary>
        public override ButtonStyle ButtonStyle
        {
            get
            {
                return IconOnly ? ButtonStyle.None : ButtonStyle.Default;
            }
            set
            {
                throw new NotSupportedException("Style of CMSAccessibleButton cannot be changed");
            }

        }


        /// <summary>
        /// Renders content.
        /// </summary>
        /// <param name="writer">HTML text writer.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            var iconString = String.Format("<i aria-hidden=\"true\" class=\"{0}\" id=\"{1}\"></i>", IconCssClass, ClientID + "_icon");

            var descText = HTMLHelper.HTMLEncode(ScreenReaderDescription ?? ToolTip);
            var descString = String.Format("<span class=\"sr-only\">{0}</span>", descText);

            writer.Write(iconString);
            writer.Write(descString);
        }


        /// <summary>
        /// Renders the button.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            this.AddCssClass("icon-only");
            if (IconOnly)
            {
                this.AddCssClass("btn-icon");
            }

            // Spinnig icons easter egg
            if (IconOnly && !IsLiveSite && ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSSpinningIcons"], false))
            {
                this.AddCssClass("spinning");
            }

            base.Render(writer);
        }
    }
}
