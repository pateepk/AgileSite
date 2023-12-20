using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents <see cref="AttachmentInfo"/> object used for response output.
    /// </summary>
    public class CMSOutputAttachment : AbstractOutputFile, IDataContainer
    {
        #region "Variables"

        private bool mIsPublished = true;

        private string mAliasPath = "/";
        private string mCultureCode;
        private string mSiteName;

        private string mRedirectTo = "";

        private DocumentAttachment mAttachment;

        private string mMimeType;

        private DateTime mValidFrom = DateTime.MinValue;
        private DateTime mValidTo = DateTime.MaxValue;

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
                    if (!SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSContentImageWatermark"))
                    {
                        // Watermark for documents is disabled
                        mUseWatermark = false;
                    }
                    else if (Attachment != null)
                    {
                        // Check if watermark can be used
                        mUseWatermark = CheckUseWatermark(SiteName, Attachment.AttachmentImageWidth, Attachment.AttachmentImageHeight);
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
        /// Attachment without AttachmentBinary.
        /// </summary>
        public DocumentAttachment Attachment
        {
            get
            {
                return mAttachment;
            }
            set
            {
                mAttachment = value;
                mAttachment.AttachmentBinary = null;
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
                DateTime result = mAttachment != null ? mAttachment.AttachmentLastModified : mInstantiated;

                // Check if not in the future
                if (result > DateTime.Now)
                {
                    result = DateTime.Now;
                }

                return result;
            }
        }


        /// <summary>
        /// File is secured (located within the secured area document).
        /// </summary>
        public bool IsSecured
        {
            get;
            set;
        }


        /// <summary>
        /// Document alias path.
        /// </summary>
        public string AliasPath
        {
            get
            {
                return mAliasPath;
            }
            set
            {
                mAliasPath = value;
            }
        }


        /// <summary>
        /// Document site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                return mSiteName ?? (mSiteName = SiteContext.CurrentSiteName);
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Document culture.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return mCultureCode ?? (mCultureCode = LocalizationContext.PreferredCultureCode);
            }
            set
            {
                mCultureCode = value;
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
        /// File document node.
        /// </summary>
        public TreeNode FileNode
        {
            get;
            set;
        }


        /// <summary>
        /// Mime type.
        /// </summary>
        public string MimeType
        {
            get
            {
                if ((mMimeType == null) && (Attachment != null))
                {
                    return Attachment.AttachmentMimeType;
                }

                return mMimeType;
            }
            set
            {
                mMimeType = value;
            }
        }


        /// <summary>
        /// Time to which the file is valid.
        /// </summary>
        public DateTime ValidTo
        {
            get
            {
                return mValidTo;
            }
            set
            {
                mValidTo = value;
            }
        }


        /// <summary>
        /// Time from which the file is valid.
        /// </summary>
        public DateTime ValidFrom
        {
            get
            {
                return mValidFrom;
            }
            set
            {
                mValidFrom = value;
            }
        }


        /// <summary>
        /// Returns true if the file is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (DateTime.Now >= ValidFrom) && (DateTime.Now <= ValidTo) && IsPublished;
            }
        }


        /// <summary>
        /// Returns true if the file is published.
        /// </summary>
        public bool IsPublished
        {
            get
            {
                return mIsPublished;
            }
            set
            {
                mIsPublished = value;
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


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSOutputAttachment()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ai">AttachmentInfo</param>
        /// <param name="data">Output file data</param>
        public CMSOutputAttachment(DocumentAttachment ai, byte[] data)
        {
            mAttachment = ai;
            mOutputData = data;

            mDataLoaded = ((ai == null) == (data == null));
        }

        #endregion


        #region "Methods"

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
                // Load the file data, only for the published attachment
                if (Attachment != null)
                {
                    var data = AttachmentBinaryHelper.GetAttachmentBinary(Attachment);
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
                if (!mDataLoaded)
                {
                    if (data != null)
                    {
                        if (Attachment == null)
                        {
                            throw new Exception("[CMSOutputFile.LoadData]: Cannot load data to the file object, the Attachment information is missing.");
                        }

                        // Resize the image if necessary
                        if (AttachmentBinaryHelper.CanResizeImage(Attachment, Width, Height, MaxSideSize))
                        {
                            Attachment.AttachmentBinary = data;
                            data = AttachmentBinaryHelper.GetImageThumbnailBinary(Attachment, Width, Height, MaxSideSize);

                            // Clear the attachment binary to save the memory
                            Attachment.AttachmentBinary = null;
                            Resized = true;
                        }

                        // Apply the watermark
                        if (UseWatermark && ImageHelper.IsImage(Attachment.AttachmentExtension))
                        {
                            ApplyWatermark(ref data);
                        }
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
                        "Attachment",
                        "OutputData",
                        "AliasPath",
                        "DataLoaded",
                        "SiteName",
                        "CultureCode",
                        "RedirectTo",
                        "Width",
                        "Height",
                        "MaxSideSize",
                        "FileNode",
                        "IsPublished",
                        "IsSecured",
                        "IsValid",
                        "MimeType",
                        "PhysicalFile",
                        "Resized",
                        "ValidFrom",
                        "ValidTo",
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
                case "attachment":
                    value = Attachment;
                    return true;

                case "outputdata":
                    value = OutputData;
                    return true;

                case "aliaspath":
                    value = AliasPath;
                    return true;

                case "sitename":
                    value = SiteName;
                    return true;

                case "culturecode":
                    value = CultureCode;
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

                case "filenode":
                    value = FileNode;
                    return true;

                case "ispublished":
                    value = IsPublished;
                    return true;

                case "issecured":
                    value = IsSecured;
                    return true;

                case "isvalid":
                    value = IsValid;
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

                case "validfrom":
                    value = ValidFrom;
                    return true;

                case "validto":
                    value = ValidTo;
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