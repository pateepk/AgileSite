using System;

namespace CMS.Core
{
    /// <summary>
    /// Represents a module info.
    /// </summary>
    public sealed class ModuleInfo
    {

        #region "Variables"

        /// <summary>
        /// A module associated with this module info.
        /// </summary>
        private readonly ModuleEntry mModule;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a module name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a directory with module files.
        /// </summary>
        public string ModuleRootPath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a module associated with this module info.
        /// </summary>
        public ModuleEntry Module
        {
            get
            {
                return mModule;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Core.ModuleInfo"/> class using the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        public ModuleInfo(ModuleEntry module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            if (module.ModuleMetadata == null)
            {
                throw new ArgumentException("Module metadata is not available.", "module");

            }

            mModule = module;
            Name = module.ModuleMetadata.Name;
            ModuleRootPath = module.ModuleMetadata.RootPath;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Core.ModuleInfo"/> class using the specified virtual module properties.
        /// </summary>
        /// <param name="name">The name of the virtual module.</param>
        /// <param name="rootPath">The directory with the virtual module files.</param>
        public ModuleInfo(string name, string rootPath)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (rootPath == null)
            {
                throw new ArgumentNullException("rootPath");
            }

            Name = name;
            ModuleRootPath = rootPath;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Converts the value of this module info to a string.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }

}