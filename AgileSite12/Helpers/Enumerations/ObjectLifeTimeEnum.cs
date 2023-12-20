using System;

using CMS.Helpers;

namespace CMS.Base
{
    #region "Enum"

    /// <summary>
    /// Enumeration of object lifetime.
    /// </summary>
    public enum ObjectLifeTimeEnum
    {
        /// <summary>
        /// Selected mode is used only for current request.
        /// </summary>
        Request = 0,

        /// <summary>
        /// Selected mode is stored to cookies.
        /// </summary>
        Cookies = 1,
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Class transformation functions for ObjectLifeTimeEnum
    /// </summary>
    public static class ObjectLifeTimeFunctions
    {
        /// <summary>
        /// Query string name/suffix used to retrieve object lifetime value.
        /// </summary>
        public static string OBJECT_LIFE_TIME_KEY = "ObjectLifeTime";


        /// <summary>
        /// Gets the current life time value
        /// </summary>
        /// <param name="prefix">Allows use specific lifetime value</param>
        public static ObjectLifeTimeEnum GetCurrentObjectLifeTime(string prefix)
        {

            var request = CMSHttpContext.Current?.Request;
            // Check whether context is available
            if (request != null)
            {
                // Try get general object lifetime value
                string value = request.QueryString[OBJECT_LIFE_TIME_KEY];

                // If general object lifetime is not defined and specific prefix is defined, try get specific lifetime value
                if ((String.IsNullOrEmpty(value)) && (!String.IsNullOrEmpty(prefix)))
                {
                    value = request.QueryString[prefix + OBJECT_LIFE_TIME_KEY];
                }

                if (!String.IsNullOrEmpty(value))
                {
                    return ObjectLifeTimeFromString(value);
                }
            }

            // Return cookies lifetime by default 
            return ObjectLifeTimeEnum.Cookies;
        }


        /// <summary>
        /// Converts enum to string 
        /// </summary>
        /// <param name="mode">Enum representation</param>
        public static String ObjectLifeTimeToString(ObjectLifeTimeEnum mode)
        {
            switch (mode)
            {
                case ObjectLifeTimeEnum.Request:
                    return "request";

                default:
                    return "cookies";
            }
        }


        /// <summary>
        /// Converts string to enum representation
        /// </summary>
        /// <param name="mode">String mode</param>
        public static ObjectLifeTimeEnum ObjectLifeTimeFromString(String mode)
        {
            switch (mode.ToLowerCSafe())
            {
                case "request":
                    return ObjectLifeTimeEnum.Request;

                default:
                    return ObjectLifeTimeEnum.Cookies;
            }
        }
    }

    #endregion
}
