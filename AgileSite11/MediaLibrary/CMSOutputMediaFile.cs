using System;
using System.Collections.Generic;
using System.Drawing;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class encapsulating MediaFile.
    /// </summary>
    public class CMSOutputMediaFile : AbstractOutputFile, IDataContainer
    {
        #region "Variables"

        private MediaFileInfo mMediaFile;
        private string mSiteName;

        private string mMimeType;
        private string mFileExtension;
        private string mFileName;
        private string mFilePath;
        private string mOriginalFile;
        private string mPreviewFilePath ;

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
                    if (!SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSMediaImageWatermark"))
                    {
                        // Watermark for media files is disabled
                        mUseWatermark = false;
                    }
                    else if (MediaFile != null)
                    {
                        // Check if watermark can be used
                        mUseWatermark = CheckUseWatermark(SiteName, MediaFile.FileImageWidth, MediaFile.FileImageHeight);
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
        /// Media file info object.
        /// </summary>
        public MediaFileInfo MediaFile
        {
            get
            {
                return mMediaFile;
            }
            set
            {
                mMediaFile = value;
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
                DateTime result = mMediaFile != null ? mMediaFile.FileModifiedWhen : mInstantiated;

                // Check if not in the future
                if (result > DateTime.Now)
                {
                    result = DateTime.Now;
                }

                return result;
            }
        }


        /// <summary>
        /// Indicates if file preview should be pused for output file.
        /// </summary>
        public bool UsePreview
        {
            get;
            set;
        }


        /// <summary>
        /// Media file site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                if ((mSiteName == null) && (MediaFile != null))
                {
                    // Get the site name
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(MediaFile.FileSiteID);
                    mSiteName = si != null ? si.SiteName : "";
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
                if ((mMimeType == null) && (MediaFile != null))
                {
                    mMimeType = MediaFile.FileMimeType;
                }
                return mMimeType;
            }
            set
            {
                mMimeType = value;
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public string FileName
        {
            get
            {
                if ((mFileName == null) && (MediaFile != null))
                {
                    mFileName = MediaFile.FileName;
                }
                return mFileName;
            }
            set
            {
                mFileName = value;
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public string FileExtension
        {
            get
            {
                if ((mFileExtension == null) && (MediaFile != null))
                {
                    mFileExtension = MediaFile.FileExtension;
                }
                return mFileExtension;
            }
            set
            {
                mFileExtension = value;
            }
        }

        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath
        {
            get
            {
                if ((mFilePath == null) && (MediaFile != null))
                {
                    mFilePath = MediaFile.FilePath;
                }
                return mFilePath;
            }
            set
            {
                mFilePath = value;
            }
        }


        /// <summary>
        /// Preview file path
        /// </summary>
        public string PreviewFilePath
        {
            get
            {
                return mPreviewFilePath ?? (mPreviewFilePath = GetPreviewPath());
            }
            set
            {
                mPreviewFilePath  = value;
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


        /// <summary>
        /// Gets physical file path to the original file.
        /// </summary>
        public string OriginalFile
        {
            get
            {
                if ((mOriginalFile == null) && (MediaFile != null))
                {
                    MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(MediaFile.FileLibraryID);
                    if (mli != null)
                    {
                        mOriginalFile = MediaFileInfoProvider.GetMediaFilePath(SiteName, mli.LibraryFolder, FilePath);
                    }
                }

                return mOriginalFile;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSOutputMediaFile()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mi">MediaFile info</param>
        /// <param name="data">Output file data</param>
        public CMSOutputMediaFile(MediaFileInfo mi, byte[] data)
        {
            mMediaFile = mi;
            mOutputData = data;

            mDataLoaded = ((mi == null) == (data == null));
        }


        /// <summary>
        /// Gets the file preview path
        /// </summary>
        private string GetPreviewPath()
        {
            // Get file path
            string path = MediaFileInfoProvider.GetMediaFilePath(MediaFile.FileLibraryID, MediaFile.FilePath);
            string pathDirectory = Path.GetDirectoryName(path);
            string hiddenFolderPath = MediaLibraryHelper.GetMediaFileHiddenFolder(SiteName);
            string folderPath = String.Format("{0}\\{1}", pathDirectory, hiddenFolderPath);

            // Ensure hidden folder exists
            DirectoryHelper.EnsureDiskPath(folderPath, pathDirectory);

            string filePath = null;

            // Get preview file
            var files = Directory.GetFiles(folderPath, MediaLibraryHelper.GetPreviewFileName(MediaFile.FileName, MediaFile.FileExtension, ".*", SiteName));
            if (files.Length > 0)
            {
                filePath = files[0];
            }

            return filePath;
        }


        /// <summary>
        /// Ensures that the object contains the output data.
        /// </summary>
        /// <param name="defaultData">Default data which should be loaded if data required</param>
        /// <returns>Returns true if new data has been loaded</returns>
        public bool EnsureData(byte[] defaultData)
        {
            if (!mDataLoaded)
            {
                if (defaultData != null)
                {
                    OutputData = defaultData;
                }
                else
                {
                    // Load preview
                    if (UsePreview)
                    {
                        LoadPreview();
                    }
                    else
                    {
                        // Load the file data
                        if (MediaFile != null)
                        {
                            LoadData();
                        }
                        else
                        {
                            OutputData = null;
                        }
                    }
                }

                mDataLoaded = true;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Loads the data to the object.
        /// </summary>
        public void LoadData()
        {
            if (MediaFile == null)
            {
                throw new Exception("[CMSOutputMediaFile.LoadData]: Cannot load data to the file object, the MediaFile information is missing.");
            }

            if (!File.Exists(OriginalFile))
            {
                return;
            }

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

                byte[] data;

                // Resize the image if necessary
                if (MediaFileInfoProvider.CanResizeImage(MediaFile, Width, Height, MaxSideSize))
                {
                    ImageHelper imgHelper = new ImageHelper(File.ReadAllBytes(OriginalFile), MediaFile.FileImageWidth, MediaFile.FileImageHeight);
                    int[] newDims = imgHelper.EnsureImageDimensions(Width, Height, MaxSideSize);

                    data = imgHelper.GetResizedImageData(newDims[0], newDims[1], MediaFileInfoProvider.ThumbnailQuality);

                    Resized = true;
                }
                else
                {
                    data = File.ReadAllBytes(OriginalFile);
                }

                // Apply the watermark
                if (UseWatermark && ImageHelper.IsImage(MediaFile.FileExtension))
                {
                    ApplyWatermark(ref data);
                }

                mOutputData = data;
                mDataLoaded = true;
            }
        }


        /// <summary>
        /// Loads the preview data to the object.
        /// </summary>
        public void LoadPreview()
        {
            if (MediaFile == null)
            {
                throw new Exception("[CMSOutputMediaFile.LoadData]: Cannot load data to the file object, the MediaFile information is missing.");
            }

            var filePath = PreviewFilePath;
            
            // Check file path
            if (!File.Exists(filePath))
            {
                return;
            }

            // Check if image
            if (!ImageHelper.IsImage(Path.GetExtension(filePath)))
            {
                return;
            }

            // Set preview file name
            FileName = Path.GetFileNameWithoutExtension(filePath);
            using (var str = FileStream.New(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var image = new Bitmap(str))
                {
                    byte[] data;

                    // Resize the image if necessary
                    if (MediaFileInfoProvider.ShouldResize(MaxSideSize, Width, Height, image.Width, image.Height))
                    {
                        int[] newDims = ImageHelper.EnsureImageDimensions(Width, Height, MaxSideSize, image.Width, image.Height);

                        // Resized image - Mimetype = jpeg
                        ImageHelper imgHelper = new ImageHelper(File.ReadAllBytes(filePath), MediaFile.FileImageWidth, MediaFile.FileImageHeight);
                        data = imgHelper.GetResizedImageData(newDims[0], newDims[1], MediaFileInfoProvider.ThumbnailQuality);

                        Width = newDims[0];
                        Height = newDims[1];
                        MimeType = "image/jpeg";
                        FileExtension = ".jpg";
                        Resized = true;
                    }
                    else
                    {
                        // Get extension of preview
                        string ext = Path.GetExtension(filePath);

                        // Set preview information
                        data = File.ReadAllBytes(filePath);
                        
                        MimeType = MimeTypeHelper.GetMimetype(ext);
                        FileExtension = Path.GetExtension(filePath);
                    }

                    // Apply the watermark
                    if (UseWatermark)
                    {
                        ApplyWatermark(ref data);
                    }

                    mOutputData = data;
                    mDataLoaded = true;
                }
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
                    "MediaFile",
                    "SiteName",
                    "OutputData",
                    "DataLoaded",
                    "RedirectTo",
                    "Width",
                    "Height",
                    "MaxSideSize",
                    "MimeType",
                    "PhysicalFile",
                    "OriginalFile",
                    "Resized",
                    "LastModified",
                    "UsePreview"
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
                case "mediafile":
                    value = MediaFile;
                    return true;

                case "sitename":
                    value = SiteName;
                    return true;

                case "outputdata":
                    value = OutputData;
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

                case "originalfile":
                    value = OriginalFile;
                    return true;

                case "resized":
                    value = Resized;
                    return true;

                case "lastmodified":
                    value = LastModified;
                    return true;

                case "usepreview":
                    value = UsePreview;
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