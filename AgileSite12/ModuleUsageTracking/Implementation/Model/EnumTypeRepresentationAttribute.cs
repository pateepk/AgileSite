using System;
using System.Linq;
using System.Text;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Specifies the data type representation for an enum field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class EnumTypeRepresentationAttribute : Attribute
    {
        private readonly Type mTypeRepresentation;


        /// <summary>
        /// Gets the data type representation value.
        /// </summary>
        internal Type TypeRepresentation
        {
            get
            {
                return mTypeRepresentation;
            }
        }


        /// <summary>
        /// Specifies the type representation for an enum field.
        /// </summary>
        /// <param name="typeRepresentation">Type representation of the attributed enum field</param>
        internal EnumTypeRepresentationAttribute(Type typeRepresentation)
        {
            mTypeRepresentation = typeRepresentation;
        }
    }
}
