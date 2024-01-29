using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MediaLibrary;

[assembly: RegisterModule(typeof(MediaLibraryModule))]

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Represents the Media Library module.
    /// </summary>
    public class MediaLibraryModule : Module
    {
        internal const string MEDIALIBRARY = "##MEDIALIBRARY##";


        /// <summary>
        /// Default constructor
        /// </summary>
        public MediaLibraryModule()
            : base(new MediaLibraryModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // Synchronization handlers
            MediaSynchronization.Init();
            
            // Delete handlers
            SiteDeletion.Init();
            CommunityGroupDeletion.Init();
           
            // Import export handlers
            MediaLibraryExport.Init();
            MediaLibraryImport.Init();
            ImportSpecialActions.Init();

            RegisterContext<MediaLibraryContext>("MediaLibraryContext");
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();
            
            RegisterCommand("GetMediaFileInfo", GetMediaFileInfo);
            RegisterCommand("GetMediaLibraryInfo", GetMediaLibraryInfo);
            RegisterCommand("GetMediaFileUrl", GetMediaFileUrl);
            RegisterCommand("GetMediaFileUrlByName", GetMediaFileUrlByName);
            RegisterCommand("DeleteMediaFile", DeleteMediaFile);
            RegisterCommand("DeleteMediaFilePreview", DeleteMediaFilePreview);
        }


        #region "Module commands methods"

        /// <summary>
        /// Get media file object
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static MediaFileInfo GetMediaFileInfo(object[] parameters)
        {
            DataRow data = (DataRow)parameters[0];

            return new MediaFileInfo(data);
        }


        /// <summary>
        /// Get media library object
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static MediaLibraryInfo GetMediaLibraryInfo(object[] parameters)
        {
            int libraryId = (int)parameters[0];
            return MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
        }


        /// <summary>
        /// Get media file URL
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetMediaFileUrl(object[] parameters)
        {
            string fileGuidString = (string)parameters[0];
            string siteName = (string)parameters[1];

            return ValidationHelper.IsGuid(fileGuidString) ? MediaLibraryHelper.GetMediaFileUrl(new Guid(fileGuidString), siteName) : null;
        }


        /// <summary>
        /// Gets the media file URL
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetMediaFileUrlByName(object[] parameters)
        {
            Guid fileGuidString = (Guid)parameters[0];
            string fileName = (string)parameters[1];
            return MediaFileURLProvider.GetMediaFileUrl(fileGuidString, fileName);

        }

        
        /// <summary>
        /// Delete media file
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object DeleteMediaFile(object[] parameters)
        {
            int siteId = (int)parameters[0];
            int libraryId = (int)parameters[1];
            string filePath = (string)parameters[2];
            bool onlyFile = (bool)parameters[3];
            bool synchronization = (bool)parameters[4];
            MediaFileInfoProvider.DeleteMediaFile(siteId, libraryId, filePath, onlyFile, synchronization);

            return null;
        }


        /// <summary>
        /// Delete media file preview
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object DeleteMediaFilePreview(object[] parameters)
        {
            string siteName = (string)parameters[0];
            int libraryId = (int)parameters[1];
            string filePath = (string)parameters[2];
            bool synchronization = (bool)parameters[3];
            MediaFileInfoProvider.DeleteMediaFilePreview(siteName, libraryId, filePath, synchronization);

            return null;
        }

        #endregion
    }
}