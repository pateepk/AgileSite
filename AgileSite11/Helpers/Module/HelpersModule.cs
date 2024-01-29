using System;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

[assembly: RegisterModule(typeof(HelpersModule))]

namespace CMS.Helpers
{
    /// <summary>
    /// Represents the helpers module.
    /// </summary>
    public class HelpersModule : ModuleEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public HelpersModule()
            : base(new HelpersModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            FileDebugOperation.RegisterReadOnlyOperation(PersistentStorageManager.OPERATION_FILE_READ);

            // Init the core localization service
            Service.Use<IConversionService, ConversionService>();

            // Register debugs
            DebugHelper.RegisterDebug(SecurityDebug.Settings);
            DebugHelper.RegisterDebug(CacheDebug.Settings);
            DebugHelper.RegisterDebug(RequestDebug.Settings);

            HelpersHandlers.PreInit();
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            // Register web farm tasks
            CacheTasks.Init();

            HelpersHandlers.Init();
        }
    }
}