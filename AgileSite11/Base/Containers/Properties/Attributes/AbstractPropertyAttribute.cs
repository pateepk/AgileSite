using System;

namespace CMS.Base
{
    /// <summary>
    /// Defines a column registered within the object
    /// </summary>
    public abstract class AbstractPropertyAttribute : Attribute
    {
        /// <summary>
        /// Property name, if not set, the original property name is used
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the property is hidden from listing
        /// </summary>
        public bool Hidden
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public AbstractPropertyAttribute()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name, if not set, the original property name is used</param>
        public AbstractPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
