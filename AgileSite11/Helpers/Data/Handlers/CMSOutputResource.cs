using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Output container for cached text resources. 
    /// Supports minification and compression.
    /// </summary>
    public class CMSOutputResource : IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Minified data.
        /// </summary>
        private string mMinifiedData;


        /// <summary>
        /// Compressed data.
        /// </summary>
        private byte[] mCompressedData;


        /// <summary>
        /// Minified and compressed data.
        /// </summary>
        private byte[] mMinifiedCompressedData;


        /// <summary>
        /// Timestamp of the last modification of data.
        /// </summary>
        private DateTime mLastModified = DateTime.MinValue;


        /// <summary>
        /// Entity tag for the data in the container. 
        /// Can be used to track versions when using caching.
        /// </summary>
        private string mEtag;

        /// <summary>
        /// List of component files
        /// </summary>
        private List<string> mComponentFiles;

        #endregion


        #region "Properties"

        
        /// <summary>
        /// Gets or sets the list of component files
        /// </summary>
        public List<string> ComponentFiles
        {
            get
            {
                if (mComponentFiles == null)
                {
                    mComponentFiles = new List<string>();
                }
                return mComponentFiles;
            }
            set
            {
                mComponentFiles = value;
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                SetValue(columnName, value);
            }
        }


        /// <summary>
        /// Gets the names of the columns this container contains.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return new List<string> { "Name", "Data", "BinaryData", "MinifiedData", "CompressedData", "MinifiedCompressedData", "LastModified", "VersionGuid", "Etag", "ContentType" };
            }
        }


        /// <summary>
        /// Cache dependency for the output
        /// </summary>
        public CMSCacheDependency CacheDependency
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the data in the container.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the data in the container.
        /// </summary>
        public string Data
        {
            get;
            set;
        }


        /// <summary>
        /// File binary data
        /// </summary>
        public byte[] BinaryData 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Content type of the resource
        /// </summary>
        public string ContentType 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Gets or sets the minified version of the data in the container.
        /// </summary>
        public string MinifiedData
        {
            get
            {
                return mMinifiedData;
            }
            set
            {
                mMinifiedData = value;
            }
        }


        /// <summary>
        /// Gets or sets the compressed version of the data in the container.
        /// </summary>
        public byte[] CompressedData
        {
            get
            {
                return mCompressedData;
            }
            set
            {
                mCompressedData = value;
            }
        }


        /// <summary>
        /// Gets or sets the minified and compressed version of the data in the container.
        /// </summary>
        public byte[] MinifiedCompressedData
        {
            get
            {
                return mMinifiedCompressedData;
            }
            set
            {
                mMinifiedCompressedData = value;
            }
        }


        /// <summary>
        /// Gets or sets the timestamp for last modification of the data.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                return mLastModified;
            }
            set
            {
                mLastModified = value;
            }
        }


        /// <summary>
        /// Gets or sets the E-tag of the data.
        /// </summary>
        public string Etag
        {
            get
            {
                string result = null;
                if (mEtag != null)
                {
                    result = mEtag;
                }
                else if (!string.IsNullOrEmpty(Name))
                {
                    result = Name;
                }

                if (!String.IsNullOrEmpty(result))
                {
                    // Put etag into quotes ("")
                    result = string.Format("\"{0}\"", result.Trim(new Char[] { '"' }));
                }

                return result;
            }
            set
            {
                mEtag = value;
            }
        }


        /// <summary>
        /// Gets if the container contains minified version of the data.
        /// </summary>
        public bool ContainsMinifiedData
        {
            get
            {
                return mMinifiedData != null;
            }
        }


        /// <summary>
        /// Gets if the container contains compressed version of the data.
        /// </summary>
        public bool ContainsCompressedData
        {
            get
            {
                return mCompressedData != null;
            }
        }


        /// <summary>
        /// Gets the approximate size of the data in the container.
        /// </summary>
        public int Size
        {
            get
            {
                // may be a bit off, but at this point we're mostly concerned about the approximate resource
                // size which consists of strings and arrays that hold the original and transformed data
                return (24 + 24 +
                        (Name != null ? Name.Length * 2 + 4 : 0) +
                        (Etag != null ? Etag.Length * 2 + 4 : 0) +
                        (Data != null ? Data.Length * 2 + 4 : 0) +
                        (MinifiedData != null ? MinifiedData.Length * 2 + 4 : 0) +
                        (CompressedData != null ? CompressedData.Length + 4 : 0) +
                        (MinifiedCompressedData != null ? MinifiedCompressedData.Length + 4 : 0));
            }
        }


        /// <summary>
        /// Output file name
        /// </summary>
        public string FileName 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Output file extension
        /// </summary>
        public string Extension
        {
            get;
            set;
        }

        #endregion

        
        #region "Methods"

        /// <summary>
        /// Checks if a container contains specific column.
        /// </summary>
        /// <param name="columnName">Name of the column to check</param>
        /// <returns>true, if a container contains given column, otherwise false</returns>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName);
        }


        /// <summary>
        /// Attempts to retrieve a value of a column.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="value">If successful, this object will stored the retrieved value</param>
        /// <returns>true, if object was successfully retrieved, otherwise false</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = null;

            switch (columnName.ToLowerCSafe())
            {
                case "name":
                    value = Name;
                    return true;

                case "data":
                    value = Data;
                    return true;

                case "binarydata":
                    value = BinaryData;
                    return true;

                case "minifieddata":
                    value = MinifiedData;
                    return true;

                case "compresseddata":
                    value = CompressedData;
                    return true;

                case "minifiedcompresseddata":
                    value = mMinifiedCompressedData;
                    return true;

                case "lastmodified":
                    value = LastModified;
                    return true;

                case "contenttype":
                    value = ContentType;
                    return true;

                case "etag":
                    value = Etag;
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Gets the value of a column.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <returns>The value in a column</returns>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);
            return value;
        }


        /// <summary>
        /// Sets the value of a column.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="value">The value to set</param>
        /// <returns>true, if value was successfully retrieved, otherwise false</returns>
        public bool SetValue(string columnName, object value)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}