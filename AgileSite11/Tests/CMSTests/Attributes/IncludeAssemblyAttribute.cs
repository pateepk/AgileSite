using System;

namespace CMS.Tests
{
    /// <summary>
    /// Includes the given assembly to the test. 
    /// Use this attribute for assemblies that are referenced by project but not used in the code, so they are included into assembly discovery.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IncludeAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Assembly name, e.g. CMS.Core
        /// </summary>
        /// <remarks>The name provided is supposed to be the name of the referenced library without .DLL extension.</remarks>
        public string AssemblyName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">Assembly name, e.g. CMS.Core</param>
        /// <remarks>The <paramref name="assemblyName"/> provided is supposed to be the name of the referenced library without .DLL extension.</remarks>
        public IncludeAssemblyAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}
