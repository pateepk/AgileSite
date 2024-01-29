using System;

using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Special field definition.
    /// </summary>
    public class SpecialField
    {
        #region "Public properties"

        /// <summary>
        /// Field value.
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// Field text.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Field visibility macro.
        /// </summary>
        public string VisibilityMacro
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Gets string representation
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0}{1}{2}",
                // Value is always present
                Value.Replace(";", SpecialFieldsDefinition.SEMICOLON_TO_REPLACE),
                // Text is added if differs from Value or if VisibilityMacro is specified, but can be empty
                !Value.EqualsCSafe(Text) || !String.IsNullOrEmpty(VisibilityMacro) ? (";" + (Text + "").Replace(";", SpecialFieldsDefinition.SEMICOLON_TO_REPLACE)) : "",
                // VisibilityMacro is added if specified
                !String.IsNullOrEmpty(VisibilityMacro) ? (";" + VisibilityMacro.Replace(";", SpecialFieldsDefinition.SEMICOLON_TO_REPLACE)) : "");
        }
    }
}