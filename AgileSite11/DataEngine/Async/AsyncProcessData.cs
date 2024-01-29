using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Control messages
    /// </summary>
    [Serializable]
    public class AsyncProcessData : ISerializable
    {
        #region "Variables"

        /// <summary>
        /// Table of the worker messages.
        /// </summary>
        private static readonly SafeDictionary<Guid, AsyncProcessData> mData = new SafeDictionary<Guid, AsyncProcessData>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Process GUID
        /// </summary>
        public Guid ProcessGUID;

        /// <summary>
        /// Error message
        /// </summary>
        public string Error;

        /// <summary>
        /// Information message
        /// </summary>
        public string Information;

        /// <summary>
        /// Warning message
        /// </summary>
        public string Warning;

        /// <summary>
        /// Canceled information message
        /// </summary>
        public string CancelledInfo;

        /// <summary>
        /// General process data
        /// </summary>
        /// <remarks>
        /// Use serializable objects marked with <see cref="SerializableAttribute" /> or implementing <see cref=" ISerializable" /> interface
        /// Otherwise set <para>AllowUpdateThroughPersistentMedium</para> to false
        /// </remarks>
        public object Data;

        /// <summary>
        /// Async process parameter
        /// </summary>
        public string Parameter;


        /// <summary>
        /// If true, data can be serialized to persistent medium (e.g. shared storage on Azure).
        /// If false, data will be not shared across non-sticky instances.
        /// </summary>
        public bool AllowUpdateThroughPersistentMedium;


        /// <summary>
        /// Worker status
        /// </summary>
        public AsyncWorkerStatusEnum Status = AsyncWorkerStatusEnum.Unknown;

        /// <summary>
        /// Worker status file, not serialized, keeps the reference to the database storage
        /// </summary>
        private TempFileInfo mStatusFile;

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the properties from another data object
        /// </summary>
        /// <param name="data">Data to load</param>
        private void LoadPropertiesFrom(AsyncProcessData data)
        {
            Error = data.Error;
            Warning = data.Warning;
            CancelledInfo = data.CancelledInfo;
            Information = data.Information;
            Data = data.Data;
            Status = data.Status;
            Parameter = data.Parameter;
        }


        /// <summary>
        /// Gets the object data for serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Error", Error);
            info.AddValue("Information", Information);
            info.AddValue("CancelledInfo", CancelledInfo);
            info.AddValue("Warning", Warning);
            info.AddValue("Status", Status);
            info.AddValue("Parameter", Parameter);

            info.AddValue("DataType", Data?.GetType());
            info.AddValue("Data", Data);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public AsyncProcessData()
        {
            AllowUpdateThroughPersistentMedium = true;
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public AsyncProcessData(SerializationInfo info, StreamingContext context)
            : this()
        {
            Error = (string)info.GetValue("Error", typeof(string));
            Information = (string)info.GetValue("Information", typeof(string));
            CancelledInfo = (string)info.GetValue("CancelledInfo", typeof(string));
            Warning = (string)info.GetValue("Warning", typeof(string));
            Status = (AsyncWorkerStatusEnum)info.GetValue("Status", typeof(AsyncWorkerStatusEnum));
            Parameter = (string)info.GetValue("Parameter", typeof(string));

            var dataType = (Type)info.GetValue("DataType", typeof(Type));
            if (dataType != null)
            {
                Data = info.GetValue("Data", dataType);
            }
        }


        /// <summary>
        /// Gets the data for process with the given process GUID
        /// </summary>
        /// <param name="processGuid">Process GUID</param>
        public static AsyncProcessData GetDataForProcess(Guid processGuid)
        {
            var data = mData[processGuid];
            if (data == null)
            {
                data = new AsyncProcessData();
                mData[processGuid] = data;
            }

            return data;
        }


        /// <summary>
        /// Saves the process data to a persistent storage if the application runs on web farm
        /// </summary>
        public void SaveToPersistentMedium()
        {
            if (!CanSynchronize())
            {
                // Only save status to storage if server has name (it is a web farm)
                return;
            }

            if (mStatusFile == null)
            {
                var processGuid = ProcessGUID;

                // Create new status file if not yet created
                mStatusFile = TempFileInfoProvider.GetTempFileInfo(processGuid, 0) ?? new TempFileInfo();
                mStatusFile.FileParentGUID = processGuid;
                mStatusFile.FileNumber = 0;
                mStatusFile.FileDirectory = "AsyncProcessData";
                mStatusFile.FileName = "AsyncProcess_" + processGuid;
                mStatusFile.FileExtension = ".txt";
                mStatusFile.FileSize = 0;
                mStatusFile.FileMimeType = "text/plain";
            }

            // Store status and parameter in file description for more efficient update of status
            mStatusFile.FileTitle = Status.ToString();
            mStatusFile.FileDescription = Parameter;

            // Store full data in file binary
            mStatusFile.FileBinary = Serialize();

            mStatusFile.SubmitChanges(false);
        }


        private bool CanSynchronize()
        {
            return AllowUpdateThroughPersistentMedium && !String.IsNullOrEmpty(SystemContext.ServerName)
                && ConnectionHelper.ConnectionAvailable && WebFarmHelper.WebFarmEnabled && IsSharedStorageEnabled();
        }


        /// <summary>
        /// Returns true when shared storage is enabled
        /// </summary>
        private static bool IsSharedStorageEnabled()
        {
            var tempRoot = TempFileInfoProvider.GetTempFilesRootFolderPath();
            return StorageHelper.IsSharedStorage(tempRoot);
        }


        /// <summary>
        /// Updates the process status from a persistent storage if the application runs on web farm
        /// </summary>
        public void UpdateStatusFromPersistentMedium()
        {
            if (!CanSynchronize())
            {
                // Only load status from storage if server has name (it is a web farm)
                return;
            }

            var file =
                TempFileInfoProvider.GetTempFiles()
                                    .TopN(1)
                                    .WhereEquals("FileParentGUID", ProcessGUID)
                                    .Columns("FileTitle", "FileDescription");

            var data = file.Result;
            if (DataHelper.DataSourceIsEmpty(data))
            {
                // No status found at persistent medium
                return;
            }

            var dataRow = data.Tables[0].Rows[0];

            // Update status
            var statusString = ValidationHelper.GetString(dataRow["FileTitle"], "");

            Enum.TryParse(statusString, true, out Status);

            // Update parameter
            Parameter = ValidationHelper.GetString(dataRow["FileDescription"], "");
        }


        /// <summary>
        /// Updates the process data from a persistent storage if the application runs on web farm
        /// </summary>
        public void UpdateFromPersistentMedium()
        {
            if (String.IsNullOrEmpty(SystemContext.ServerName))
            {
                // Only load data from storage if server has name (it is a web farm)
                return;
            }
            var fileInfo = TempFileInfoProvider.GetTempFileInfo(ProcessGUID, 0);
            if (fileInfo == null)
            {
                // No data found at persistent medium
                return;
            }

            // Get status from file description
            var data = fileInfo.FileBinary;
            if (data != null)
            {
                var stream = new SystemIO.MemoryStream(data);
                var formatter = new BinaryFormatter();
                var loaded = (AsyncProcessData)formatter.Deserialize(stream);

                LoadPropertiesFrom(loaded);
            }

            Parameter = fileInfo.FileDescription;
        }


        /// <summary>
        /// Serializes the object
        /// </summary>
        private byte[] Serialize()
        {
            using (var stream = new SystemIO.MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);

                return stream.ToArray();
            }
        }

        #endregion
    }
}