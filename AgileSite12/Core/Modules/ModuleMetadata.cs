using System;

namespace CMS.Core
{

    /// <summary>
    /// Represents the module metadata.
    /// </summary>
    public class ModuleMetadata
    {

        #region "Properties"

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the directory with module files.
        /// </summary>
        public string RootPath
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Core.ModuleMetadata"/> class.
        /// </summary>
        /// <param name="name">The module name.</param>
        public ModuleMetadata(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
        }

        #endregion

    }

}