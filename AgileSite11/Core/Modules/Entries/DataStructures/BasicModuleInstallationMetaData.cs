using System;
using System.Linq;
using System.Text;


namespace CMS.Core
{
    /// <summary>
    /// Basic module installation meta data (can be retrieved from the name of module installation meta file).
    /// The basic meta data is read only.
    /// </summary>
    public class BasicModuleInstallationMetaData
    {
        /// <summary>
        /// Name of the module.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Version of the module.
        /// </summary>
        public string Version
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates new basic module installation meta data.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="version">Module version.</param>
        public BasicModuleInstallationMetaData(string name, string version)
        {
            Name = name;
            Version = version;
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return String.Format("'{0}' ({1})", Name, Version);
        }
    }
}
