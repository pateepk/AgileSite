using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Button that generates appropriate HTML code
    /// for its use in actions container.
    /// </summary>
    public class CMSAccessibleButton : CMSButton
    {
        #region "Properties"

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
        /// When true button is render as type="submit". Otherwise type is type="button".
        /// </summary>
        public override bool UseSubmitBehavior
        {
            get;
            set;
        }

        #endregion


        #region "Overridden properties"

        /// <summary>
        /// HTML tag for this control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Button;
            }
        }


        /// <summary>
        /// When true input tag is rendered instead of button tag.
        /// </summary>
        public override bool RenderInputTag
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException("CMSAccessibleButton must always render button tag. This property cannot be changed.");
            }
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

        #endregion


        #region "Methods"

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

        #endregion
    }
}
