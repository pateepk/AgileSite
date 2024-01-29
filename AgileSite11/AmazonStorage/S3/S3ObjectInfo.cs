using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Amazon.S3;
using Amazon.S3.Model;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Represents S3 object.
    /// </summary>
    public class S3ObjectInfo : IS3ObjectInfo
    {
        #region "Constants"

        /// <summary>
        /// Extension for metadata objects.
        /// </summary>
        public const string METADATA_EXT = ".meta";

        /// <summary>
        /// Metadata folder.
        /// </summary>
        public const string METADATA_FOLDER = "__metadata/";

        /// <summary>
        /// Storage key for Amazon storage objects.
        /// </summary>
        public const string STORAGE_KEY = "AmazonStorage|S3ObjectInfo|";

        #endregion


        #region "Variables"

        private Dictionary<string, string> mMetadata = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new instance of S3 object info with specified bucket name.
        /// </summary>        
        /// <param name="path">Path to the file name.</param>
        public S3ObjectInfo(string path)
            : this(path, false)
        {
        }


        /// <summary>
        /// Initializes new instance of S3 object info with specified bucket name.
        /// </summary>        
        /// <param name="path">Path with file name.</param>
        /// <param name="key">Specifies that given path is already object key.</param>
        public S3ObjectInfo(string path, bool key)
        {
            Key = key ? path : PathHelper.GetObjectKeyFromPath(path);

            // End slash for directories
            if ((path != null) && path.EndsWith("\\", StringComparison.Ordinal) && !Key.EndsWith("/", StringComparison.Ordinal))
            {
                Key += "/";
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Returns bucket name.
        /// </summary>
        private string BucketName
        {
            get
            {
                return S3ObjectInfoProvider.GetBucketName(PathHelper.GetPathFromObjectKey(Key, true));
            }
        }


        /// <summary>
        /// Returns S3 client instance.
        /// </summary>
        private AmazonS3Client S3Client
        {
            get
            {
                return AccountInfo.Current.S3Client;
            }
        }


        /// <summary>
        /// Returns provider object.
        /// </summary>
        private IS3ObjectInfoProvider Provider
        {
            get
            {
                return S3ObjectInfoProvider.Current;
            }
        }


        /// <summary>
        /// Gets or sets collection with metadata.
        /// </summary>
        private Dictionary<string, string> Metadata
        {
            get
            {
                if (mMetadata == null)
                {
                    mMetadata = RequestStockHelper.GetItem(STORAGE_KEY, Key + "|Metadata", false) as Dictionary<string, string>;
                    if (mMetadata == null)
                    {
                        mMetadata = new Dictionary<string, string>();
                    }
                }

                return mMetadata;
            }
            set
            {
                mMetadata = value;
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|Metadata", mMetadata, false);
            }
        }

        #endregion


        #region "Public properties - IS3ObjectInfo members"

        /// <summary>
        /// Gets or sets object key.
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        ///<summary>
        ///Returns whether current object is locked.
        ///</summary>
        public bool IsLocked
        {
            get
            {
                return ValidationHelper.GetBoolean(GetMetadata(S3ObjectInfoProvider.LOCK), false);
            }
        }


        /// <summary>
        /// Returns E-tag from the object.
        /// </summary>
        public string ETag
        {
            get
            {
                return GetETag();
            }
            set
            {
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|ETag", value, false);
            }
        }


        /// <summary>
        /// Gets whether current object is directory.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                if (Key.EndsWith("/", StringComparison.Ordinal))
                {
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Gets or sets content length of the object.
        /// </summary>
        public long Length
        {
            get
            {
                return GetLength();
            }
            set
            {
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|Length", value, false);
            }
        }

        #endregion


        #region "Public methods - IS3ObjectInfo members"

        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        public void SetMetadata(string key, string value)
        {
            SetMetadata(key, value, true);
        }

        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        /// <param name="update">Indicates whether data are updated in S3 storage.</param>        
        public void SetMetadata(string key, string value, bool update)
        {
            SetMetadata(key, value, update, true);
        }


        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        /// <param name="update">Indicates whether data are updated in S3 storage.</param>
        /// <param name="log">Indicates whether is operation logged.</param>
        public void SetMetadata(string key, string value, bool update, bool log)
        {
            if (Metadata.ContainsKey(key))
            {
                Metadata[key] = value;
            }
            else
            {
                Metadata.Add(key, value);
            }

            if (update)
            {
                SaveMetadata(PathHelper.GetPathFromObjectKey(Key, true), Metadata);
            }

            if (log)
            {
                // Log SetMetadata operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(Key, true), "SetMetadata", IOProviderName.Amazon);
            }
        }


        /// <summary>
        /// Returns object meta data.  
        /// </summary>
        /// <param name="key">Metadata key.</param>        
        public string GetMetadata(string key)
        {
            FetchMetadata();
            if (Metadata.ContainsKey(key))
            {
                return Metadata[key];
            }

            return null;
        }


        /// <summary>
        /// Deletes metadata file.
        /// </summary>        
        public void DeleteMetadataFile()
        {
            DeleteObjectRequest request = new DeleteObjectRequest();
            request.BucketName = BucketName;
            request.Key = METADATA_FOLDER + Key + METADATA_EXT;

            DeleteObjectResponse response = S3Client.DeleteObject(request);
        }


        /// <summary>
        /// Locks current object.
        /// </summary>
        public void Lock()
        {
            if (Provider.ObjectExists(this))
            {
                // Log Lock operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(Key, true), "Lock", IOProviderName.Amazon);
                SetMetadata(S3ObjectInfoProvider.LOCK, "True", true, false);
            }
        }


        /// <summary>
        /// Unlocks current object.
        /// </summary>
        public void UnLock()
        {
            if (Provider.ObjectExists(this))
            {
                // Log Unlock operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(Key, true), "Unlock", IOProviderName.Amazon);
                SetMetadata(S3ObjectInfoProvider.LOCK, "False", true, false);
            }
        }


        /// <summary>
        /// Returns whether object exists.
        /// </summary>        
        public bool Exists()
        {
            if (string.IsNullOrEmpty(Key))
            {
                return false;
            }

            FetchMetadata();

            return ValidationHelper.GetBoolean(RequestStockHelper.GetItem(STORAGE_KEY, Key + "|Exists", false), false);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns E-tag of the current object. 
        /// </summary>
        private string GetETag()
        {
            string cacheKey = Key + "|ETag";

            // Check if data was already fetched
            if (!RequestStockHelper.Contains(STORAGE_KEY, cacheKey, false))
            {
                FetchMetadata();
            }

            return ValidationHelper.GetString(RequestStockHelper.GetItem(STORAGE_KEY, cacheKey, false), string.Empty);
        }


        /// <summary>
        /// Returns content length of the current object.
        /// </summary>
        private long GetLength()
        {
            string cacheKey = Key + "|Length";

            // Check if data was already fetched
            if (!RequestStockHelper.Contains(STORAGE_KEY, cacheKey, false))
            {
                FetchMetadata();
            }

            return ValidationHelper.GetLong(RequestStockHelper.GetItem(STORAGE_KEY, cacheKey, false), 0);
        }


        /// <summary>
        /// Downloads metadata from the cloud. It ensures that RequestStockHelper always contains the "Exists" key.
        /// </summary>
        private void FetchMetadata()
        {
            // First try to find if key exists
            if (RequestStockHelper.Contains(STORAGE_KEY, Key + "|Exists", false))
            {
                return;
            }

            // Get metadata from cloud
            GetObjectMetadataRequest request = new GetObjectMetadataRequest();
            request.BucketName = BucketName;
            request.Key = Key;

            try
            {
                GetObjectMetadataResponse response = S3Client.GetObjectMetadata(request);

                // Cache metadata information
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|Length", response.ContentLength, false);
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|ETag", response.ETag, false);
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|Exists", true, false);

                // Get metadata from special file
                Metadata = LoadMetadata(PathHelper.GetPathFromObjectKey(Key, true));

                if (!Metadata.ContainsKey(S3ObjectInfoProvider.LAST_WRITE_TIME))
                {
                    Metadata.Add(S3ObjectInfoProvider.LAST_WRITE_TIME, ValidationHelper.GetString(response.LastModified, string.Empty, "en-us"));
                }

                // Log operation
                FileDebug.LogFileOperation(PathHelper.GetPathFromObjectKey(Key, true), "FetchMetadata", IOProviderName.Amazon);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Cache information that key is not exist
                RequestStockHelper.AddToStorage(STORAGE_KEY, Key + "|Exists", false, false);
            }
        }


        /// <summary>
        /// Saves metadata to special file into S3 storage.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="metadata">Metadata.</param>
        private void SaveMetadata(string path, Dictionary<string, string> metadata)
        {
            string metadataFile = path + METADATA_EXT;
            string serialized = string.Empty;

            // Serialize into one string
            foreach (var record in metadata)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(record.Key, ";", record.Value, "#");
                serialized += sb.ToString();
            }

            var request = new PutObjectRequest
            {
                BucketName = BucketName,
                ContentBody = serialized,
                Key = METADATA_FOLDER + PathHelper.GetObjectKeyFromPath(metadataFile)
            };
            S3Client.PutObject(request);
        }


        /// <summary>
        /// Loads metadata from special file from S3 storage.
        /// </summary>
        /// <param name="path">Path.</param>        
        private Dictionary<string, string> LoadMetadata(string path)
        {
            // Get data
            string metadataFile = path + METADATA_EXT;
            Dictionary<string, string> metadata = new Dictionary<string, string>();

            // Get serialized data 
            S3ObjectInfo obj = new S3ObjectInfo(metadataFile);

            // Download object
            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = BucketName;
            request.Key = METADATA_FOLDER + PathHelper.GetObjectKeyFromPath(metadataFile);

            // Delete begin of absolute path
            if (metadataFile.StartsWith(Directory.CurrentDirectory, StringComparison.Ordinal))
            {
                metadataFile = metadataFile.Substring(Directory.CurrentDirectory.Length);
            }

            string tempPath = Path.Combine(PathHelper.TempPath, "__metadata" + metadataFile);

            try
            {
                // Save it to the temp
                using (GetObjectResponse response = S3Client.GetObject(request))
                {
                    response.WriteResponseStreamToFile(tempPath);
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return metadata;
                }
            }

            string serialized = System.IO.File.ReadAllText(tempPath);

            // Deserialize it
            string[] records = serialized.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string record in records)
            {
                string[] items = record.Split(';');
                metadata.Add(items[0], items[1]);
            }

            return metadata;
        }

        #endregion
    }
}
