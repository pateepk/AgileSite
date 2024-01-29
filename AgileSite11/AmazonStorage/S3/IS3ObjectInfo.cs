namespace CMS.AmazonStorage
{
    /// <summary>
    /// Interface of the S3ObjectInfo object.
    /// </summary>
    public interface IS3ObjectInfo
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets object key.
        /// </summary>
        string Key
        {
            get;
            set;
        }


        ///<summary>
        ///Returns whether current object is locked.
        ///</summary>
        bool IsLocked
        {
            get;
        }


        /// <summary>
        /// Gets or sets E-tag from the object.
        /// </summary>
        string ETag
        {
            get;
            set;
        }


        /// <summary>
        /// Gets whether current object is directory.
        /// </summary>
        bool IsDirectory
        {
            get;
        }


        /// <summary>
        /// Gets or sets content length of the object.
        /// </summary>
        long Length
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        void SetMetadata(string key, string value);


        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>        
        /// <param name="update">Indicates whether data are updated in S3 storage.</param>
        void SetMetadata(string key, string value, bool update);


        /// <summary>
        /// Sets meta data to object.
        /// </summary>
        /// <param name="key">MetaData key.</param>
        /// <param name="value">Metadata value.</param>
        /// <param name="update">Indicates whether data are updated in S3 storage.</param>
        /// <param name="log">Indicates whether is operation logged.</param>
        void SetMetadata(string key, string value, bool update, bool log);


        /// <summary>
        /// Returns object meta data.  
        /// </summary>
        /// <param name="key">Metadata key.</param>        
        string GetMetadata(string key);


        /// <summary>
        /// Deletes metadata file.
        /// </summary>        
        void DeleteMetadataFile();


        /// <summary>
        /// Locks current object.
        /// </summary>
        void Lock();


        /// <summary>
        /// Unlocks current object.
        /// </summary>
        void UnLock();


        /// <summary>
        /// Returns whether object exists.
        /// </summary>        
        bool Exists();

        #endregion
    }
}
