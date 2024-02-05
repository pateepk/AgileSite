using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Defined lazy initialized string setting
    /// </summary>
    public class StringAppSetting : AppSetting<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Settings key name</param>
        /// <param name="defaultValue">Default value</param>
        public StringAppSetting(string keyName, string defaultValue)
            : base(keyName, defaultValue, null)
        {
        }


        /// <summary>
        /// Converts the value to a proper type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        protected override string ConvertValue(object value, string defaultValue)
        {
            return CoreServices.Conversion.GetString(value, defaultValue);
        }
    }
}
