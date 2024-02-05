using System;

using CMS.Base;

namespace CMS
{
    /// <summary>
    /// Defines a column registered within the object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class RegisterColumnAttribute : AbstractPropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RegisterColumnAttribute()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name, if not set, the original property name is used</param>
        public RegisterColumnAttribute(string name)
            : base(name)
        {
        }
    }
}
