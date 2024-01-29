using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Literal control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedLiteral runat=server></{0}:LocalizedLiteral>"), Serializable()]
    public class LocalizedLiteral : Literal
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
        /// Name of a resource string used for text.
        /// </summary>
        public string ResourceString
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
            if (DisplayColon && !string.IsNullOrEmpty(text) && !text.EndsWithCSafe(ResHelper.Colon, StringComparison.InvariantCultureIgnoreCase))
            {
                text += ResHelper.Colon;
            }

            return text;
        }


        /// <summary>
        /// Refreshes button's text on pre-render.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RefreshText();
        }

        #endregion
    }
}