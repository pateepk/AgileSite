using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Hyperlink control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedHyperlink runat=server></{0}:LocalizedHyperlink>"), Serializable()]
    public class LocalizedHyperlink : HyperLink
    {
        #region "Variables"

        /// <summary>
        /// Source text.
        /// </summary>
        private string mText = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Localized string source property - may be 'database' or 'file'.
        /// </summary>
        public string Source
        {
            get;
            set;
        }


        /// <summary>
        /// Display text.
        /// </summary>
        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                mText = value;
            }
        }


        /// <summary>
        /// Gets or sets the client-side script that executes when link event is clicked.
        /// </summary>
        public string OnClientClick
        {
            get;
            set;
        }


        /// <summary>
        /// Name of a resource string used for text.
        /// </summary>
        public string ResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Name of a resource string used for tooltip.
        /// </summary>
        public string ToolTipResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Display colon at the end of the text.
        /// </summary>
        public bool DisplayColon
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Refreshes the control text.
        /// </summary>
        public void RefreshText()
        {
            base.Text = GetText();
        }


        /// <summary>
        /// Gets the final text of the control.
        /// </summary>
        protected string GetText()
        {
            // Get the localized text
            if (mText == null)
            {
                mText = base.Text;
            }
            string text = ControlsLocalization.GetLocalizedText(this, ResourceString, Source, mText);

            // Add additional characters
            text = AddCharacters(text);

            return text;
        }


        /// <summary>
        /// Adds the characters to the string.
        /// </summary>
        /// <param name="text">Base text</param>
        public string AddCharacters(string text)
        {
            // Add colon to the end of the text
            if (DisplayColon && !String.IsNullOrEmpty(text) && !text.EndsWithCSafe(ResHelper.Colon, StringComparison.InvariantCultureIgnoreCase))
            {
                text += ResHelper.Colon;
            }

            return text;
        }


        /// <summary>
        /// Refreshes link's text on pre-render.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);

            RefreshText();
            if (!String.IsNullOrEmpty(OnClientClick))
            {
                Attributes.Add("onclick", OnClientClick);
            }
        }


        /// <summary>
        /// Render override.
        /// </summary>
        /// <param name="writer">Output writer stream</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // For design mode display control ID
            if (Context == null)
            {
                writer.Write(" [ LocalizedHyperlink : " + ID + " ]");
                return;
            }

            base.Render(writer);
        }

        #endregion
    }
}