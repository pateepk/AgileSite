using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web farm synchronization for Documents
    /// </summary>
    internal class DocumentSynchronization
    {
        #region "Variables"

        private static bool? mSynchronizeAttachments;

        #endregion


        #region "Properties"

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

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.UpdateAttachment,
                Execute = UpdateAttachment,
                Condition = CheckSynchronizeAttachments
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.DeleteAttachment,
                Execute = DeleteAttachment,
                Condition = CheckSynchronizeDeleteAttachments
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.ClearDocumentFieldsTypeInfos,
                Execute = ClearDocumentFieldsTypeInfos,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.InvalidateDocumentFieldsTypeInfo,
                Execute = InvalidateDocumentFieldsTypeInfo,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.ClearDocumentTypeInfos,
                Execute = ClearDocumentTypeInfos,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DocumentTaskType.InvalidateDocumentTypeInfo,
                Execute = InvalidateDocumentTypeInfo,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = DataTaskType.ClearResolvedClassNames,
                Execute = ClearResolvedClassNames,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the attachments is allowed
        /// </summary>
        private static bool CheckSynchronizeAttachments(IWebFarmTask task)
        {
            return SynchronizeAttachments;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the attachments is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteAttachments(IWebFarmTask task)
        {
            return SynchronizeAttachments && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Updates the attachment
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateAttachment(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 5)
            {
                throw new ArgumentException(String.Format("[DocumentSynchronization.UpdateAttachment]: Expected 5 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            string guid = data[1];
            string fileName = data[2];
            string extension = data[3];
            bool deleteOldFiles = Convert.ToBoolean(data[4]);

            // Save the binary data to the disk
            if (binaryData.Data != null)
            {
                AttachmentBinaryHelper.SaveFileToDisk(siteName, guid, fileName, extension, binaryData, deleteOldFiles, true);
            }
            else
            {
                // Drop the cache dependencies
                CacheHelper.TouchKey("attachment|" + guid.ToLowerCSafe(), false, false);
            }
        }


        /// <summary>
        /// Deletes the attachment
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteAttachment(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[DocumentSynchronization.DeleteAttachment]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            Guid guid = ValidationHelper.GetGuid(data[0], Guid.Empty);
            string siteName = data[1];
            bool deleteDirectory = ValidationHelper.GetBoolean(data[2], false);

            // Delete file
            AttachmentBinaryHelper.DeleteFile(guid, siteName, deleteDirectory, true);
        }


        /// <summary>
        /// Clears the document fields type infos
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearDocumentFieldsTypeInfos(string target, string[] data, BinaryData binaryData)
        {
            DocumentFieldsInfoProvider.Clear(false);
        }


        /// <summary>
        /// Invalidates the document fields type info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateDocumentFieldsTypeInfo(string target, string[] data, BinaryData binaryData)
        {
            DocumentFieldsInfoProvider.InvalidateTypeInfo(data.FirstOrDefault() ?? "", false);
        }


        /// <summary>
        /// Clears the documents type infos
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearDocumentTypeInfos(string target, string[] data, BinaryData binaryData)
        {
            TreeNodeProvider.Clear(false);
        }


        /// <summary>
        /// Invalidates the documents type info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateDocumentTypeInfo(string target, string[] data, BinaryData binaryData)
        {
            TreeNodeProvider.InvalidateTypeInfo(data.FirstOrDefault() ?? "", false);
        }


        /// <summary>
        /// Clears resolved class names
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearResolvedClassNames(string target, string[] data, BinaryData binaryData)
        {
            DocumentTypeHelper.ClearClassNames(false);
        }

        #endregion
    }
}
