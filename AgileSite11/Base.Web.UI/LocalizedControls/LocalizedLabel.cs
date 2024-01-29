using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Label control with localized text string.
    /// </summary>
    [ToolboxData("<{0}:LocalizedLabel runat=server></{0}:LocalizedLabel>"), Serializable]
    public class LocalizedLabel : Label
    {
        #region "Variables"

        // Source text.
        private string mText;

        // Indicates if the label should be displayed (not visible but rendered to page).
        private bool mDisplay = true;

        private bool dataLoaded;

        private string mRequiredMarkCssClass;

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
                return mText ?? (mText = base.Text);
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
        public virtual bool DisplayColon
        {
            get;
            set;
        }


        /// <summary>
        /// Display star at the end of the text.
        /// </summary>
        public virtual bool ShowRequiredMark
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets CSS class for the required mark (*)
        /// </summary>
        public string RequiredMarkCssClass
        {
            get
            {
                return mRequiredMarkCssClass ?? (mRequiredMarkCssClass = "required-mark");
            }
            set
            {
                mRequiredMarkCssClass = value;
            }
        }


        /// <summary>
        /// Associated control client ID.
        /// </summary>
        public string AssociatedControlClientID
        {
            get;
            set;
        }


        /// <summary>
        /// Tag key.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (AssociatedControlClientID != null)
                {
                    return HtmlTextWriterTag.Label;
                }
                return base.TagKey;
            }
        }


        /// <summary>
        /// Indicates if the label should be displayed (not visible but rendered to page). Default value is 'true'.
        /// </summary>
        public bool Display
        {
            get
            {
                return mDisplay;
            }
            set
            {
                mDisplay = value;
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Refreshes label's text on pre-render.
        /// </summary>        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            LoadData();
        }


        /// <summary>
        /// Render override.
        /// </summary>
        /// <param name="writer">Output writer stream</param>
        protected override void Render(HtmlTextWriter writer)
        {
            LoadData();

            // For design mode display control ID
            if (Context == null)
            {
                writer.Write("[LocalizedLabel: " + ID + "]");
                return;
            }

            base.Render(writer);
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
        /// <param name="addCharacters">If allowed special characters are added to the text, e.g. colon, required mark (optional; default value is TRUE).</param>
        public string GetText(bool addCharacters = true)
        {
            // Get the localized text
            if (mText == null)
            {
                mText = base.Text;
            }
            string text = ControlsLocalization.GetLocalizedText(this, ResourceString, Source, mText);

            if (addCharacters)
            {
                // Add additional characters
                text = AddCharacters(text);
            }

            return text;
        }


        /// <summary>
        /// Adds the characters to the string.
        /// </summary>
        /// <param name="text">Base text</param>
        public string AddCharacters(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string requiredMarkHTML = string.Format("<span class=\"{0}\">{1}</span>", RequiredMarkCssClass, ResHelper.RequiredMark);
                bool textEndsWithColon = text.EndsWithCSafe(ResHelper.Colon, StringComparison.InvariantCultureIgnoreCase);
                
                //Text ends with RequiredMark symbol or with RequiredMark html code
                bool textEndsWithRequiredMark = text.EndsWithCSafe(ResHelper.RequiredMark, StringComparison.InvariantCultureIgnoreCase)
                                                || text.EndsWithCSafe(string.Format("{0}</span>", ResHelper.RequiredMark), StringComparison.InvariantCultureIgnoreCase);

                if (DisplayColon && ShowRequiredMark)
                {
                    if (!textEndsWithRequiredMark)
                    {   
                        if (textEndsWithColon)
                        {
                            // Add required mark to the end of the text
                            text += requiredMarkHTML;
                        }
                        else
                        {
                            // Add colon and required mark to the end of the text
                            text += ResHelper.Colon + requiredMarkHTML;
                        }
                    }
                }
                else if (DisplayColon && !textEndsWithColon)
                {
                    // Add colon the end of the text
                    text += ResHelper.Colon;
                }
                else if (ShowRequiredMark && !textEndsWithRequiredMark)
                {
                    // Add required mark to the end of the text
                    text += requiredMarkHTML;
                }
            }

            return text;
        }


        /// <summary>
        /// Reloads the control data
        /// </summary>
        private void LoadData()
        {
            if (dataLoaded)
            {
                return;
            }
            dataLoaded = true;

            ToolTip = ControlsLocalization.GetLocalizedText(this, ToolTipResourceString, Source, ToolTip);
            RefreshText();

            // Ensure associated control client ID attribute
            if (AssociatedControlClientID != null)
            {
                AssociatedControlID = string.Empty;
                Attributes.Add("for", AssociatedControlClientID);
            }

            if (!Display)
            {
                Attributes.Add("style", "display:none;");
            }
        }

        #endregion
    }
}