using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Defined lazy initialized integer setting
    /// </summary>
    public class IntAppSetting : AppSetting<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Settings key name</param>
        /// <param name="defaultValue">Default value</param>
        public IntAppSetting(string keyName, int defaultValue)
            : base(keyName, defaultValue, null)
        {
        }


        /// <summary>
        /// Converts the value to a proper type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        protected override int ConvertValue(object value, int defaultValue)
        {
            return CoreServices.Conversion.GetInteger(value, defaultValue);
        }
    }
}
