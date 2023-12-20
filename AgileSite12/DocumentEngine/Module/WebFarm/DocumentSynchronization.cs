using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm synchronization for Documents.
    /// </summary>
    internal class DocumentSynchronization
    {
        private static bool? mSynchronizeAttachments;


        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for document attachments is enabled.
        /// </summary>
        public static bool SynchronizeAttachments
        {
            get
            {
                if (mSynchronizeAttachments == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeAttachments", "CMSWebFarmSynchronizeAttachments", true));
                }

                return mSynchronizeAttachments.Value;
            }
            set
            {
                mSynchronizeAttachments = value;
            }
        }


        /// <summary>
        /// Initializes the tasks for media files synchronization.
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<UpdateAttachmentWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteAttachmentWebFarmTask>();
            WebFarmHelper.RegisterTask<ClearDocumentFieldsTypeInfosWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearDocumentTypeInfosWebFarmTask>(true);
            WebFarmHelper.RegisterTask<InvalidateDocumentFieldsTypeInfoWebFarmTask>(true);
            WebFarmHelper.RegisterTask<InvalidateDocumentTypeInfoWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearResolvedClassNamesWebFarmTask>(true);
        }
    }
}
