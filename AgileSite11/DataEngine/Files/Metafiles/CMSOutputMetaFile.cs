using System;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents <see cref="MetaFileInfo"/> object used for response output.
    /// </summary>
    public class CMSOutputMetaFile : AbstractOutputFile, IDataContainer
    {
        #region "Variables"

        private string mRedirectTo = "";

        private MetaFileInfo mMetaFile;
        private string mSiteName;

        private string mMimeType;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the watermark is allowed to be used.
        /// </summary>
        public override bool UseWatermark
        {
            get
            {
                if (mUseWatermark == null)
                {
                    if (!SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSMetaImageWatermark"))
                    {
                        // Watermark for meta files is disabled
                        mUseWatermark = false;
                    }
                    else if (MetaFile != null)
                    {
                        // Check if watermark can be used
                        mUseWatermark = CheckUseWatermark(SiteName, MetaFile.MetaFileImageWidth, MetaFile.MetaFileImageHeight);
                    }
                    else
                    {
                        mUseWatermark = false;
                    }
                }

                return mUseWatermark.Value;
            }
            set
            {
                mUseWatermark = value;
            }
        }


        /// <summary>
        /// Meta file info without AttachmentBinary.
        /// </summary>
        public MetaFileInfo MetaFile
        {
            get
            {
                return mMetaFile;
            }
            set
            {
                mMetaFile = value;
                mMetaFile.MetaFileBinary = null;
                mSiteName = null;
            }
        }


        /// <summary>
        /// Time when the file was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                // Get the value
                DateTime result = mMetaFile != null ? mMetaFile.MetaFileLastModified : mInstantiated;

                // Check if not in the future
                if (result > DateTime.Now)
                {
                    result = DateTime.Now;
                }

                return result;
            }
        }


        /// <summary>
        /// If set, file should be redirected to the file system.
        /// </summary>
        public string RedirectTo
        {
            get
            {
                return mRedirectTo;
            }
            set
            {
                mRedirectTo = value;
            }
        }


        /// <summary>
        /// Meta file site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                if ((mSiteName == null) && (MetaFile != null))
                {
                    // Get the site name
                    mSiteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, MetaFile.MetaFileSiteID);
                }

                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Mime type.
        /// </summary>
        public string MimeType
        {
            get
            {
                if ((mMimeType == null) && (MetaFile != null))
                {
                    return MetaFile.MetaFileMimeType;
                }
                
                return mMimeType;
            }
            set
            {
                mMimeType = value;
            }
        }


        /// <summary>
        /// Physical file path to the data.
        /// </summary>
        public string PhysicalFile
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSOutputMetaFile()
        {
            PhysicalFile = null;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mi">MetaFile info</param>
        /// <param name="data">Output file data</param>
        public CMSOutputMetaFile(MetaFileInfo mi, byte[] data)
        {
            PhysicalFile = null;
            mMetaFile = mi;
            mOutputData = data;

            mDataLoaded = ((mi == null) == (data == null));
        }


        /// <summary>
        /// Ensures that the object contains the output data.
        /// </summary>
        /// <param name="defaultData">Default data which should be loaded if data required</param>
        /// <returns>Returns true if new data has been loaded</returns>
        public bool EnsureData(byte[] defaultData)
        {
            if (mDataLoaded)
            {
                return false;
            }

            if (defaultData != null)
            {
                OutputData = defaultData;
            }
            else
            {
                // Load the file data
                if (MetaFile != null)
                {
                    byte[] data = MetaFileInfoProvider.GetFile(MetaFile, SiteName);
                    LoadData(data);
                }
                else
                {
                    OutputData = null;
                }
            }

            mDataLoaded = true;
            return true;
        }


        /// <summary>
        /// Loads the data to the object.
        /// </summary>
        /// <param name="data">New data</param>
        public void LoadData(byte[] data)
        {
            // Load data only if it was not loaded before
            if (mDataLoaded)
            {
                return;
            }

            // Lock, because GDI+ calls are not thread safe
            lock (this)
            {
                // Load data only if it was not loaded before - second check in lock
                if (mDataLoaded)
                {
                    return;
                }

                if (data != null)
                {
                    if (MetaFile == null)
                    {
                        throw new Exception("[CMSOutputMetaFile.LoadData]: Cannot load data to the file object, the MetaFile information is missing.");
                    }

                    // Resize the image if necessary
                    if (MetaFileInfoProvider.CanResizeImage(MetaFile, Width, Height, MaxSideSize))
                    {
                        MetaFile.MetaFileBinary = data;
                        data = MetaFileInfoProvider.GetImageThumbnail(MetaFile, SiteName, Width, Height, MaxSideSize);
                                
                        // Clear the attachment binary to save the memory
                        MetaFile.MetaFileBinary = null;
                        Resized = true;
                    }

                    // Apply the watermark
                    if (UseWatermark && ImageHelper.IsImage(MetaFile.MetaFileExtension))
                    {
                        ApplyWatermark(ref data);
                    }
                }

                mOutputData = data;
                mDataLoaded = true;
            }
        }

        #endregion


        #region "IDataContainer Members"

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
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return new List<string>
                {
                    "MetaFile",
                    "SiteName",
                    "OutputData",
                    "DataLoaded",
                    "RedirectTo",
                    "Width",
                    "Height",
                    "MaxSideSize",
                    "MimeType",
                    "PhysicalFile",
                    "Resized",
                    "LastModified"
                };
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            switch (columnName.ToLowerCSafe())
            {
                case "metafile":
                    value = MetaFile;
                    return true;

                case "sitename":
                    value = SiteName;
                    return true;

                case "outputdata":
                    value = OutputData;
                    return true;

                case "redirectto":
                    value = RedirectTo;
                    return true;

                case "width":
                    value = Width;
                    return true;

                case "height":
                    value = Height;
                    return true;

                case "maxsidesize":
                    value = MaxSideSize;
                    return true;

                case "mimetype":
                    value = MimeType;
                    return true;

                case "physicalfile":
                    value = PhysicalFile;
                    return true;

                case "resized":
                    value = Resized;
                    return true;

                case "lastmodified":
                    value = LastModified;
                    return true;
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}