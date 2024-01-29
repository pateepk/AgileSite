using System;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Interface for the hierarchically accessible object. This object provides the properties to access its connected objects.
    /// </summary>
    public interface IHierarchicalObject : INameIndexable
    {
        /// <summary>
        /// Properties of the object available through GetProperty.
        /// </summary>
        List<String> Properties
        {
            get;
        }


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        object GetProperty(string columnName);


        /// <summary>
        /// Returns property with given name (either object or property value).
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        bool TryGetProperty(string columnName, out object value);
    }
}