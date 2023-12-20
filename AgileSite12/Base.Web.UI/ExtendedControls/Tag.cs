using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Control representing tag (text in colored box). 
    /// Renders css class depending on color brightness to ensure sufficient contrast.
    /// </summary>
    public class Tag : WebControl, IOutputEncodingControl
    {
        /// <summary>
        /// Text to be written on tag. This text will be localized and encoded.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// HTML code of color for this tag (e.g. #bebebe, red, #abc).
        /// </summary>
        public string Color
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value indicating whether control output is encoded or not.
        /// </summary>
        /// <remarks>
        /// <c>True</c> by default.
        /// </remarks>
        public bool EncodeOutput
        {
            get;
            set;
        }


        /// <summary>
        /// Creates a new instance of tag rendered as HTML span element.
        /// </summary>
        public Tag()
            : base(HtmlTextWriterTag.Span)
        {
            EncodeOutput = true;
        }


        /// <summary>
        /// Sets styles and colors of tag.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Set style (color and css classes)
            SetStyle();
        }


        /// <summary>
        /// Renders the contents of the tag to the specified writer.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            var output = ResHelper.LocalizeString(Text);

            if (EncodeOutput)
            {
                output = HTMLHelper.HTMLEncode(output);
            }

            writer.Write(output);

            base.RenderContents(writer);
        }


        /// <summary>
        /// Sets background color and CSS class specific for dark/bright colors. Does not overwrite existing styles set from outside.
        /// </summary>
        private void SetStyle()
        {
            var style = new Style();
            style.CssClass = "tag";

            // Check if Color has valid value
            var color = ValidationHelper.GetColor(Color, System.Drawing.Color.Transparent);
            if (color != System.Drawing.Color.Transparent)
            {
                // Set background color
                style.BackColor = color;

                // Get 'luma' component of color in YIQ color space
                var y = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;

                // Use specific css class for dark/bright tag
                var isDark = y < 140;
                style.CssClass += isDark ? " tag-dark" : " tag-bright";
            }

            // Fill blank style elements of control with prepared style.
            MergeStyle(style);
        }
    }
}


