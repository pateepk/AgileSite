using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Class representing an icon control.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSIcon : Panel
    {
        /// <summary>
        /// Alternative text for screen readers.
        /// </summary>
        public string AlternativeText
        {
            get;
            set;
        }
        

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag" /> value that corresponds to this Web server control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.I;
            }
        }


        /// <summary>
        /// Gets the icon image HTML
        /// </summary>
        /// <param name="cssClass">Icon CSS class</param>
        public static string GetIconImageHtml(string cssClass)
        {
            return String.Format("<i class=\"{0}\"></i>", HTMLHelper.EncodeForHtmlAttribute(cssClass));
        }


        /// <summary>
        /// Gets the icon text HTML for screen-readers
        /// </summary>
        /// <param name="text">Icon text</param>
        public static string GetIconTextHtml(string text)
        {
            return String.Format("<span class=\"sr-only\">{0}</span>", HTMLHelper.HTMLEncode(text));
        }


        /// <summary>
        /// Renders icon and alternative text for screen readers.
        /// </summary>
        /// <param name="writer">Output writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            WriteText(writer);
        }


        /// <summary>
        /// Writes the icon text to the given writer
        /// </summary>
        /// <param name="writer">Output writer</param>
        private void WriteText(HtmlTextWriter writer)
        {
            var text = ResHelper.LocalizeString(AlternativeText);
            if (!String.IsNullOrEmpty(text))
            {
                var html = GetIconTextHtml(text);

                writer.Write(html);
            }
        }


        /// <summary>
        /// Gets the complete icon HTML
        /// </summary>
        /// <param name="cssClass">Icon CSS class</param>
        /// <param name="text">Icon text</param>
        public static string GetIconHtml(string cssClass, string text = null)
        {
            if (cssClass == null)
            {
                throw new ArgumentNullException("cssClass");
            }

            // Get icon image
            string result = GetIconImageHtml(cssClass);

            // Add icon text
            if (!String.IsNullOrEmpty(text))
            {
                result += GetIconTextHtml(text);
            }

            return result;
        }
    }
}