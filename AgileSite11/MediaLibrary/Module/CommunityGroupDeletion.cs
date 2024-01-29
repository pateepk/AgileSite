using CMS.DataEngine;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Community group deletion actions
    /// </summary>
    internal class CommunityGroupDeletion
    {
        /// <summary>
        /// Initializes the actions for deletion
        /// </summary>
        public static void Init()
        {
            ObjectEvents.Delete.Before += Delete_Before;
        }


        /// <summary>
        /// Event handler for deleting group media libraries.
        /// </summary>
        private static void Delete_Before(object sender, ObjectEventArgs e)
        {
            var group = e.Object;
            if (group.TypeInfo.ObjectType == PredefinedObjectType.GROUP)
            {
                string siteName = group.Generalized.ObjectSiteName;
                if (!string.IsNullOrEmpty(siteName))
                {
                    var folders = MediaLibraryInfoProvider.DeleteMediaLibrariesInfos(group.Generalized.ObjectID);
                    MediaLibraryInfoProvider.DeleteMediaLibrariesFolders(siteName, folders);
                }
            }
        }
    }
}
