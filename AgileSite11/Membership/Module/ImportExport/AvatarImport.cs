using System;

using CMS.CMSImportExport;

namespace CMS.Membership
{
    /// <summary>
    /// Handles special actions during the Avatar import process.
    /// </summary>
    public static class AvatarImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.GetBinaryDataSourcePath.Execute += GetBinaryDataSourcePath_Execute;
        }


        private static void GetBinaryDataSourcePath_Execute(object sender, GetBinaryDataSourcePathEventArgs e)
        {
            var settings = e.Settings;
            var infoObj = e.Object;

            if (infoObj.TypeInfo.ObjectType == AvatarInfo.OBJECT_TYPE)
            {
                var avatar = (AvatarInfo)infoObj;

                // Get path
                e.Path = ImportProvider.GetBinaryDataSourcePath(settings, infoObj, "cms_avatar\\CMSFiles", avatar.AvatarGUID.ToString(), avatar.AvatarFileExtension);
            }
        }

        #endregion
    }
}