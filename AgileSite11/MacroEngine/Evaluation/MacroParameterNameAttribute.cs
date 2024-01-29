using System;
using System.Linq;
using System.Text;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Assigns macro parameter name to property representing that macro parameter. 
    /// Parameter name is string representation of macro parameter used in macro expressions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class MacroParameterNameAttribute : Attribute
    {
        /// <summary>
        /// Name of macro parameter
        /// </summary>
        public string ParameterName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of macro paramater</param>
        public MacroParameterNameAttribute(string name)
        {
            ParameterName = name;
        }
    }
}
