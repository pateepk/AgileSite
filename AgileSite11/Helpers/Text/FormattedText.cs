namespace CMS.Helpers.Markup
{
    /// <summary>
    /// Class for formatting text by HTML tags.
    /// </summary>
    public class FormattedText
    {
        #region "Variables

        string text;
        string openingTags = string.Empty;
        string closingTags = string.Empty;

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormattedText(string text = "")
        {
            this.text = text;
        }


        /// <summary>
        /// Sets the inner text, doesn't change already added tags.
        /// </summary>
        public FormattedText SetText(string text)
        {
            this.text = text;
            return this;
        }


        /// <summary>
        /// Adds tag enclosing the text.
        /// </summary>
        public FormattedText AddTag(string openingTag, string closingTag)
        {
            openingTags += openingTag;
            closingTags = closingTags.Insert(0, closingTag);
            return this;
        }


        /// <summary>
        /// Returns formatted string.
        /// </summary>
        public override string ToString()
        {
            return openingTags + text + closingTags;
        }

        #endregion


        #region "Formatting methods"

        /// <summary>
        /// Adds span HTML tag and uses alert-status-error CSS class.
        /// </summary>
        public FormattedText ColorRed()
        {
            return AddTag("<span class='alert-status-error'>", "</span>");
        }


        /// <summary>
        /// Adds span HTML tag and uses alert-status-success CSS class.
        /// </summary>
        public FormattedText ColorGreen()
        {
            return AddTag("<span class='alert-status-success'>", "</span>");
        }


        /// <summary>
        /// Adds span HTML tag and uses alert-status-warning CSS class.
        /// </summary>
        public FormattedText ColorOrange()
        {
            return AddTag("<span class='alert-status-warning'>", "</span>");
        }


        /// <summary>
        /// Adds span tag with set title attribute.
        /// </summary>
        public FormattedText AddTooltip(string tooltip)
        {
            return AddTag("<span title='" + tooltip + "'>", "</span>");
        }


        /// <summary>
        /// Adds JavaScript that gets called when text is clicked.
        /// </summary>
        public FormattedText OnClientClick(string javascript)
        {
            return AddTag("<span class='SelectableItem' onclick=\"" + javascript + "\">", "</span>");
        }

        #endregion
    }
}
