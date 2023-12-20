using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.IO;

[assembly: RegisterModule(typeof(IOModule))]

namespace CMS.IO
{
    /// <summary>
    /// Represents the IO module.
    /// </summary>
    public class IOModule : ModuleEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public IOModule()
            : base(new IOModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            DebugHelper.RegisterDebug(FileDebug.Settings);

            // Register event handlers to dispose open zip files
            IOEvents.DeleteFile.Before += DeleteFile_Before;
            IOEvents.DeleteDirectory.Before += DeleteDirectory_Before;
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Register web farm tasks
            StorageTasks.Init();
        }


        private void DeleteFile_Before(object sender, IOEventArgs e)
        {
            var path = e.Path;
            if (ZipStorageProvider.IsZipFile(path))
            {
                // Dispose all mapped sub paths
                var zipFolder = Path.GetDirectoryName(path) + ZipStorageProvider.GetZipFileName(Path.GetFileName(path));

                ZipStorageProvider.DisposeAll(p => p.StartsWith(zipFolder, StringComparison.OrdinalIgnoreCase));

                // Dispose the zip file itself
                ZipStorageProvider.Dispose(path);
            }
        }


        private void DeleteDirectory_Before(object sender, IOEventArgs e)
        {
            var path = e.Path;

            // Dispose all mapped sub paths
            ZipStorageProvider.DisposeAll(p => p.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}