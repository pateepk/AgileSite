using System;

using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents a parameter in macro rule.
    /// </summary>
    [Serializable]
    public class MacroRuleParameter
    {
        #region "Variables"

        private string mValueType = FieldDataType.Text;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the rule parameter value (value inserted to the K# condition instead of the macro).
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the rule parameter text (text displayed in the sentence).
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the default text displayed when the value is empty.
        /// </summary>
        public string DefaultText
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the value of the parameter is required (does not allow empty).
        /// </summary>
        public bool Required
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the text of the parameter was taken from the value.
        /// </summary>
        public bool ApplyValueTypeConversion
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the value.
        /// </summary>
        public string ValueType
        {
            get
            {
                return mValueType;
            }
            set
            {
                mValueType = value;
            }
        }

        #endregion
    }
}