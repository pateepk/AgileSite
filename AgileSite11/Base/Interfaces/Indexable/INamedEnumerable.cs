using System.Collections;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Sorted enumerable interface
    /// </summary>
    public interface INamedEnumerable<out T> : INamedEnumerable, IEnumerable<T>
    {
    }


    /// <summary>
    /// Sorted enumerable interface
    /// </summary>
    public interface INamedEnumerable : IEnumerable
    {
        /// <summary>
        /// Returns true if the items in the collection have names
        /// </summary>
        bool ItemsHaveNames
        {
            get;
        }


        /// <summary>
        /// Specifies whether the names should be sorted or not
        /// </summary>
        bool SortNames
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the name of the given object
        /// </summary>
        /// <param name="obj">Object for which to get the name</param>
        string GetObjectName(object obj);
    }
}