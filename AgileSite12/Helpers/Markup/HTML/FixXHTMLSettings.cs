namespace CMS.Helpers
{
    /// <summary>
    /// Container for the XHTML fixing settings.
    /// </summary>
    public class FixXHTMLSettings
    {
        #region "Properties"

        /// <summary>
        /// Lowercase tags filter, lowercases tag names and attribute names in the HTML
        /// </summary>
        public bool LowerCase = false;

        /// <summary>
        /// Fix attribute names and values to make sure they are in valid format and using correct quotes
        /// </summary>
        public bool Attributes = false;

        /// <summary>
        /// Self close filter, closes all tags that are self-closing, and missing the end tag
        /// </summary>
        public bool SelfClose = false;

        /// <summary>
        /// Resolve URL filter, resolves all relative URLs starting with ~/
        /// </summary>
        public bool ResolveUrl = false;

        /// <summary>
        /// Fix javascript filter, ensures that all javascript tags contain the type attribute
        /// </summary>
        public bool Javascript = false;

        /// <summary>
        /// Fix HTML5 obsolete attributes, converts the attributes to corresponding CSS classes or CSS styles, such as class="cellspacing_0"
        /// </summary>
        public bool HTML5 = false;

        /// <summary>
        /// Converts TABLE tags to DIV tags
        /// </summary>
        public ConvertTableEnum TableToDiv = ConvertTableEnum.None;

        /// <summary>
        /// Ensures proper indentation of the HTML (make it nice).
        /// </summary>
        public bool Indent = false;

        #endregion


        #region "Aggregated properties"

        /// <summary>
        /// Returns true if any of the filters is enabled
        /// </summary>
        public bool AnyEnabled
        {
            get
            {
                return (SelfClose || Indent || ProcessTagContent);
            }
        }


        /// <summary>
        /// Returns true, if the settings indicate that the tag content should be processed
        /// </summary>
        public bool ProcessTagContent
        {
            get
            {
                return (LowerCase || Attributes || Javascript || ResolveUrl || (TableToDiv != ConvertTableEnum.None) || HTML5);
            }
        }


        /// <summary>
        /// Returns true if the filter may modify CSS class
        /// </summary>
        public bool MayModifyCssClass
        {
            get
            {
                return (HTML5 || (TableToDiv != ConvertTableEnum.None));
            }
        }

        #endregion
    }
}
