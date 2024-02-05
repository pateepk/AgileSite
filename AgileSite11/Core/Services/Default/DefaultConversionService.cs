using System;

namespace CMS.Core
{
    /// <summary>
    /// Default service to provide conversion
    /// </summary>
    internal class DefaultConversionService : IConversionService
    {
        /// <summary>
        /// Returns the integer representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public int GetInteger(object value, int defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            string stringValue = value.ToString();
            int result;

            if (!Int32.TryParse(stringValue, out result))
            {
                result = defaultValue;
            }

            return result;
        }


        /// <summary>
        /// Returns the boolean representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public bool GetBoolean(object value, bool defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            switch (value.ToString().ToLowerInvariant())
            {
                case "0":
                case "false":
                    return false;

                case "1":
                case "true":
                    return true;

                default:
                    return defaultValue;
            }
        }


        /// <summary>
        /// Returns the double representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public double GetDouble(object value, double defaultValue, string culture)
        {
            if (value == null)
            {
                return defaultValue;
            }
            else if (value is Double)
            {
                return (Double)value;
            }
            else
            {
                Double result;

                if (Double.TryParse(value.ToString(), out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Returns the guid representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public Guid GetGuid(object value, Guid defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }
            else if (value is Guid)
            {
                return (Guid)value;
            }
            else
            {
                Guid result;
                
                if (Guid.TryParse(value.ToString(), out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Returns the string representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public string GetString(object value, string defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }
            
            return value.ToString();
        }


        /// <summary>
        /// Gets the identifier created from the given name
        /// </summary>
        /// <param name="name">Display name</param>
        public string GetIdentifier(string name)
        {
            return name;
        }


        /// <summary>
        /// Returns the DateTime representation of an object or default value if not.
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="culture">Culture</param>
        public DateTime GetDateTime(object value, DateTime defaultValue, string culture)
        {
            if (value == null)
            {
                return defaultValue;
            }
            else if (value is DateTime)
            {
                return (DateTime)value;
            }
            else
            {
                DateTime result;

                if (DateTime.TryParse(value.ToString(), out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Converts the value to specified type. If the value is null, default value is used.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default value</param>
        public T GetValue<T>(object value, T defaultValue)
        {
            // Default value for null
            if ((value == null) || (value == DBNull.Value))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }


        /// <summary>
        /// Gets the code name created from the given string.
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="replacement">Replacement string for invalid characters</param>
        public string GetCodeName(object name, string replacement = null)
        {
            return name.ToString();
        }
    }
}
