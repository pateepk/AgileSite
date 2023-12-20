using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Defined lazy initialized setting
    /// </summary>
    public class AppSetting<TValue> : CMSLazy<TValue>
    {
        /// <summary>
        /// Conversion function
        /// </summary>
        protected Func<object, TValue, TValue> Converter
        {
            get;
            set;
        }


        /// <summary>
        /// Settings key name
        /// </summary>
        public string KeyName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Function to retrieve the default value
        /// </summary>
        public Func<TValue> DefaultValueInitializer
        {
            get;
            set;
        }



        /// <summary>
        /// Defines the master key name which is used in case the key name value is not found
        /// </summary>
        public string MasterKeyName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyName">Settings key name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="converter">Conversion function</param>
        public AppSetting(string keyName, TValue defaultValue, Func<object, TValue, TValue> converter)
            : base(null, defaultValue)
        {
            Converter = converter;
            KeyName = keyName;
        }


        /// <summary>
        /// Converts the value to the correct type
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        protected virtual TValue ConvertValue(object value, TValue defaultValue)
        {
            return Converter(value, defaultValue);
        }


        /// <summary>
        /// Initializes a new value
        /// </summary>
        protected override TValue NewValue()
        {
            var value = CoreServices.AppSettings[KeyName];

            if ((value == null) && !String.IsNullOrEmpty(MasterKeyName))
            {
                value = CoreServices.AppSettings[MasterKeyName];
            }

            return ConvertValue(value, GetDefaultValue());
        }


        /// <summary>
        /// Gets the default value
        /// </summary>
        private TValue GetDefaultValue()
        {
            if (DefaultValueInitializer != null)
            {
                return DefaultValueInitializer();
            }

            return DefaultValue;
        }


        /// <summary>
        /// Returns value converted to string.
        /// </summary>
        public override string ToString()
        {
            return Convert.ToString(Value);
        }
    }
}
