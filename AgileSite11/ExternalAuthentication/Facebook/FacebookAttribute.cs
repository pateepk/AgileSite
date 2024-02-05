using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ExternalAuthentication.Facebook
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class FacebookAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets name of Facebook response's property that has data for property with this attribute.
        /// </summary>
        public string ResponsePropertyName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets name of the Facebook permission scope that is required for getting this property in the response.
        /// </summary>
        public string PermissionScopeName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the type of a value converter.
        /// </summary>
        public Type ValueConverterType
        {
            get;
            set;
        }


        /// <summary>
        /// Creates attribute specifying which Facebook response property has data for property with this attribute.
        /// </summary>
        /// <param name="responsePropertyName">Name of Facebook response's property that has data for property with this attribute.</param>
        public FacebookAttribute(string responsePropertyName)
        {
            ResponsePropertyName = responsePropertyName;
        }
    }
}
