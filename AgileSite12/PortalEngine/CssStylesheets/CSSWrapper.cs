using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Wrapper for the CSS object property
    /// </summary>
    public class CSSWrapper : CollectionPropertyWrapper<BaseInfo>, INamedEnumerable
    {
        /// <summary>
        /// If true, the macros are resolved within the CSS stylesheets
        /// </summary>        
        public static bool ResolveMacrosInCSS
        {
            get
            {
                return CoreServices.Settings["CMSResolveMacrosInCSS"].ToBoolean(false);
            }
        }


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Collection to wrap</param>
        /// <param name="propertyName">Property to extract</param>
        public CSSWrapper(IInfoObjectCollection<BaseInfo> collection, string propertyName)
            : base(collection, propertyName)
        {
            if (collection == null)
            {
                return;
            }

            Collection = collection.GetSubsetWhereNotEmpty(propertyName);
        }


        /// <summary>
        /// Gets the property value for the given object
        /// </summary>
        /// <param name="obj">Obj from which to take the property</param>
        protected override ObjectProperty GetProperty(BaseInfo obj)
        {
            ObjectProperty result = base.GetProperty(obj);

            // Process the value
            string value = (string)result.Value;
            if (String.IsNullOrEmpty(value))
            {
                return result;
            }

            // Resolve the macros if necessary
            if (ResolveMacrosInCSS)
            {
                value = MacroResolver.Resolve(value);
            }

            // Trim the charset from the CSS
            value = TrimCharset(value);

            // Always ensure new line at the end of the CSS
            if (!value.EndsWithCSafe("\n"))
            {
                value += "\r\n";
            }

            result.InjectValue(value);

            return result;
        }


        /// <summary>
        /// Trims the charset from the CSS style
        /// </summary>
        /// <param name="data">CSS text where to trim the charset</param>
        public static string TrimCharset(string data)
        {
            // Trim the charset declaration
            if (data.StartsWithCSafe("@charset", true))
            {
                int nextLine = data.IndexOfCSafe('\n');
                if (nextLine < 0)
                {
                    return "";
                }

                data = data.Substring(nextLine + 1).Trim();
            }

            return data;
        }

        #endregion


        #region "INamedEnumerable Members"

        /// <summary>
        /// Returns true if the items in the collection have names
        /// </summary>
        public bool ItemsHaveNames
        {
            get
            {
                return Collection.ItemsHaveNames;
            }
        }


        /// <summary>
        /// If true, the names in enumeration are sorted
        /// </summary>
        public bool SortNames
        {
            get
            {
                return Collection.SortNames;
            }
            set
            {
                Collection.SortNames = value;
            }
        }


        /// <summary>
        /// Gets the object name for the given object
        /// </summary>
        /// <param name="obj">Object for which to get the name</param>
        public string GetObjectName(object obj)
        {
            var prop = (ObjectProperty)obj;

            return Collection.GetObjectName(prop.Object);
        }

        #endregion
    }
}