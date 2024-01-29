using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Defined lazy initialized boolean setting
    /// </summary>
    public class BoolAppSetting : AppSetting<bool>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Settings key name</param>
        /// <param name="defaultValue">Default value</param>
        public BoolAppSetting(string keyName, bool defaultValue = false)
            : base(keyName, defaultValue, null)
        {
        }


        /// <summary>
        /// Converts the value to a proper type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        protected override bool ConvertValue(object value, bool defaultValue)
        {
            return CoreServices.Conversion.GetBoolean(value, defaultValue);
        }
    }
}
