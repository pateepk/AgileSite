using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Specifies the string representation for an enum field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnumStringRepresentationAttribute : Attribute
    {
        private readonly string mStringRepresentation;


        /// <summary>
        /// Gets the string representation value.
        /// </summary>
        public string StringRepresentation
        {
            get
            {
                return mStringRepresentation;
            }
        }


        /// <summary>
        /// Specifies the string representation for an enum field.
        /// </summary>
        /// <param name="stringRepresentation">String representation of the attributed enum field</param>
        public EnumStringRepresentationAttribute(string stringRepresentation)
        {
            mStringRepresentation = stringRepresentation;
        }
    }
}
