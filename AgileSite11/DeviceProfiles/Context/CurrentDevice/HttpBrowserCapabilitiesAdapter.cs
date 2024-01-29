using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Converts the <see cref="HttpBrowserCapabilities" /> class to fulfill <see cref="ISimpleDataContainer" /> interface. 
    /// </summary>
    internal class HttpBrowserCapabilitiesAdapter : HttpBrowserCapabilities, ISimpleDataContainer
    {
        /// <summary>
        /// Gets specific browser capability.
        /// </summary>
        /// <param name="columnName">Browser capability name.</param>
        object ISimpleDataContainer.this[string columnName]
        {
            get
            {
                return CMSHttpContext.Current.Request.Browser[columnName];
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets specific browser capability.
        /// </summary>
        /// <param name="columnName">Browser capability name.</param>
        public object GetValue(string columnName)
        {
            return CMSHttpContext.Current.Request.Browser[columnName];
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Not implemented.</param>
        /// <param name="value">Not implemented.</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }
    }
}
