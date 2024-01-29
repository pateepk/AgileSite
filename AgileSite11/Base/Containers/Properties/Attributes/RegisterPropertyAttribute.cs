using System;

using CMS.Base;

namespace CMS
{
    /// <summary>
    /// Defines a property registered within the object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class RegisterPropertyAttribute : AbstractPropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RegisterPropertyAttribute()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name, if not set, the original property name is used</param>
        public RegisterPropertyAttribute(string name)
            : base(name)
        {
        }
    }
}
