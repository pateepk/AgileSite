using System;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(TempFileInfo), TempFileInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// TempFileInfo data container class.
    /// </summary>
    public class TempFileInfo : AbstractInfo<TempFileInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "temp.file";


        private string mFileSiteName;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TempFileInfoProvider), OBJECT_TYPE, "temp.file", "FileID", "FileLastModified", "FileGUID", null, null, "FileBinary", null, null, null)
        {
            SupportsVersioning = false,
            MacroCollectionName = "CMS.TempFile",
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Temporary file site name.
        /// </summary>
        public virtual string FileSiteName
        {
            get
            {
                return mFileSiteName;
            }
            set
            {
                mFileSiteName = value;
            }
        }


        /// <summary>
        /// Temporary file ID.
        /// </summary>
        public virtual int FileID
        {
            get
            {
                return GetIntegerValue("FileID", 0);
            }
            set
            {
                SetValue("FileID", value);
            }
        }


        /// <summary>
        /// Date and time when the temporary file was last modified.
        /// </summary>
        public virtual DateTime FileLastModified
        {
            get
            {
                return GetDateTimeValue("FileLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileLastModified", value);
            }
        }


        /// <summary>
        /// Temporary file scope unique identifier (temporary files are numbered uniquely within this scope).
        /// </summary>
        public virtual Guid FileParentGUID
        {
            get
            {
                return GetGuidValue("FileParentGUID", Guid.Empty);
            }
            set
            {
                SetValue("FileParentGUID", value);
            }
        }


        /// <summary>
        /// Temporary file mime type.
        /// </summary>
        public virtual string FileMimeType
        {
            get
            {
                return GetStringValue("FileMimeType", "");
            }
            set
            {
                SetValue("FileMimeType", value);
            }
        }


        /// <summary>
        /// Temporary file unique identifier.
        /// </summary>
        public virtual Guid FileGUID
        {
            get
            {
                return GetGuidValue("FileGUID", Guid.Empty);
            }
            set
            {
                SetValue("FileGUID", value);
            }
        }


        /// <summary>
        /// Temporary file binary.
        /// </summary>
        public virtual byte[] FileBinary
        {
            get
            {
                object val = GetValue("FileBinary");
                return (byte[])val;
            }
            set
            {
                SetValue("FileBinary", value);
            }
        }


        /// <summary>
        /// Name of the temporary file.
        /// </summary>
        public virtual int FileNumber
        {
            get
            {
                return GetIntegerValue("FileNumber", 0);
            }
            set
            {
                SetValue("FileNumber", value);
            }
        }


        /// <summary>
        /// Name of the original file.
        /// </summary>
        public virtual string FileName
        {
            get
            {
                return GetStringValue("FileName", "");
            }
            set
            {
                SetValue("FileName", value);
            }
        }


        /// <summary>
        /// Temporary file extension.
        /// </summary>
        public virtual string FileExtension
        {
            get
            {
                return GetStringValue("FileExtension", "");
            }
            set
            {
                SetValue("FileExtension", value);
            }
        }


        /// <summary>
        /// Folder within temporary files folder.
        /// </summary>
        public virtual string FileDirectory
        {
            get
            {
                return GetStringValue("FileDirectory", "");
            }
            set
            {
                SetValue("FileDirectory", value);
            }
        }


        /// <summary>
        /// Temporary file width (makes sense for images only).
        /// </summary>
        public virtual int FileImageWidth
        {
            get
            {
                return GetIntegerValue("FileImageWidth", 0);
            }
            set
            {
                SetValue("FileImageWidth", value);
            }
        }


        /// <summary>
        /// Temporary files size.
        /// </summary>
        public virtual long FileSize
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("FileSize"), 0);
            }
            set
            {
                SetValue("FileSize", value);
            }
        }


        /// <summary>
        /// Temporary file height (makes sense for images only).
        /// </summary>
        public virtual int FileImageHeight
        {
            get
            {
                return GetIntegerValue("FileImageHeight", 0);
            }
            set
            {
                SetValue("FileImageHeight", value);
            }
        }


        /// <summary>
        /// Temporary file title.
        /// </summary>
        public virtual string FileTitle
        {
            get
            {
                return GetStringValue("FileTitle", string.Empty);
            }
            set
            {
                SetValue("FileTitle", value);
            }
        }


        /// <summary>
        /// Temporary file description.
        /// </summary>
        public virtual string FileDescription
        {
            get
            {
                return GetStringValue("FileDescription", string.Empty);
            }
            set
            {
                SetValue("FileDescription", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TempFileInfoProvider.DeleteTempFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TempFileInfoProvider.SetTempFileInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TempFileInfo object.
        /// </summary>
        public TempFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TempFileInfo object from the given DataRow.
        /// </summary>
        public TempFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the binary data - loads the binary data from file if available.
        /// </summary>
        /// <param name="forceLoadFromDB">If true, the data are loaded even from DB</param>
        protected internal override byte[] EnsureBinaryData(bool forceLoadFromDB)
        {
            if (FileBinary == null)
            {
                byte[] data = base.EnsureBinaryData(forceLoadFromDB);
                if (data == null)
                {
                    FileBinary = TempFileInfoProvider.GetTempFileBinary(FileDirectory, FileParentGUID, FileNumber, FileExtension);
                }
                else
                {
                    FileBinary = data;
                }
            }
            return FileBinary;
        }

        #endregion
    }
}