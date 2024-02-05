using System;

namespace CMS
{
    /// <summary>
    /// Registers the module within assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterModuleAttribute : Attribute
    {
        /// <summary>
        /// Module type
        /// </summary>
        public Type Type
        {
            get;
            set;
        }       

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Module type</param>
        public RegisterModuleAttribute(Type type)
        {
            Type = type;
        }
    }
}