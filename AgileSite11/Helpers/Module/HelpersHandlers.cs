using System;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Event handlers for the module Helpers
    /// </summary>
    internal class HelpersHandlers
    {
        /// <summary>
        /// Pre-initializes the handlers
        /// </summary>
        public static void PreInit()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.PreInitialized.Execute += InitZippedStorage;
            }

            FileSystemStorage.File.GetFileUrlForPath.Execute += GetFileUrlForPath;
        }
      

        /// <summary>
        /// Initialize the handlers
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsWebSite)
            {
                if (SystemContext.IsCMSRunningAsMainApplication)
                {
                    RequestEvents.Begin.Execute += BeginRequest;
                    RequestEvents.PostAuthorize.Execute += HandleClickjacking;
                    RequestEvents.AcquireRequestState.Execute += InitSessionTimeout;
                    RequestEvents.Finalize.Execute += DecrementPendingRequests;
                    ApplicationEvents.Initialized.Execute += RegisterManagedCompilationPath;
                }

                RequestEvents.RunEndRequestTasks.Execute += LogDebugs;
            }
        }


        /// <summary>
        /// Gets the URL from path for default file system storage
        /// </summary>
        private static void GetFileUrlForPath(object sender, CMSEventArgs<FileSystemStorage.GetFileUrlEventArgs> e)
        {
            var path = AbstractStorageProvider.GetTargetPhysicalPath(e.Parameter.Path);
            e.Parameter.Url = URLHelper.GetAbsoluteUrl(path);
        }


        /// <summary>
        ///  Register managed compilation folder
        /// </summary>
        private static void RegisterManagedCompilationPath(object sender, EventArgs e)
        {
            VirtualPathHelper.RegisterManagedCompilationPath("~/cms");
        }


        /// <summary>
        /// Initializes the project file system
        /// </summary>
        private static void InitZippedStorage(object sender, EventArgs e)
        {
            MapZippedImages();
        }


        /// <summary>
        /// Handles click jacking in current request
        /// </summary>
        private static void HandleClickjacking(object sender, EventArgs eventArgs)
        {
            SecurityHelper.HandleClickjacking();
        }


        /// <summary>
        /// Runs the actions for the begin request event
        /// </summary>
        private static void BeginRequest(object sender, EventArgs eventArgs)
        {
            RequestHelper.PendingRequests.Increment(null);

            RequestHelper.EnsureScriptTimeout();
        }


        /// <summary>
        /// Actions executed when the request is finalized
        /// </summary>
        private static void LogDebugs(object sender, EventArgs e)
        {
            // Register the debug logs
            if (DebugHelper.AnyDebugEnabled)
            {
                RequestDebug.LogRequestValues(true, true, true);

                DebugHelper.RegisterLogs();
            }

            // Write debug request logs
            if (DebugHelper.AnyDebugEnabled)
            {
                DebugHelper.WriteRequestLogs();
            }
        }


        /// <summary>
        /// Actions executed when the request is finalized
        /// </summary>
        private static void DecrementPendingRequests(object sender, EventArgs e)
        {
            // Decrement the counter for pending requests
            RequestHelper.PendingRequests.Decrement(null);
        }


        /// <summary>
        /// Initializes the session timeout
        /// </summary>
        private static void InitSessionTimeout(object sender, EventArgs e)
        {
            SessionHelper.InitSessionTimeout();
        }
        

        /// <summary>
        /// Maps the zipped images folder
        /// </summary>
        private static void MapZippedImages()
        {
            // Register mapped paths
            if (StorageHelper.UseZippedResources)
            {
                Path.RegisterMappedZippedFolder("~/App_Themes/Default/Images", "Images.zip");
            }
            else
            {
                // Map files to the zipped images folder
                Path.RegisterMappedPath("~/App_Themes/Default/Images", "~/App_Themes/Default/ZippedImages");
            }
        }
    }
}
