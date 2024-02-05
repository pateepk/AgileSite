using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Class representing key-value. Used for initializing properties of control loaded by UILayoutPane.
    /// </summary>
    public class UILayoutValue
    {
        /// <summary>
        /// Identifying key.
        /// </summary>
        public string Key
        {
            get;
            private set;
        }


        /// <summary>
        /// Value related to the key.
        /// </summary>
        public object Value
        {
            get;
            private set;
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="key">Identifying key.</param>
        /// <param name="value">Value related to the key.</param>
        public UILayoutValue(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
