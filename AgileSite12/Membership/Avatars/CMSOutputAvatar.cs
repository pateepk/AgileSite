using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Summary description for CMSOutputAvatar.
    /// </summary>
    public class CMSOutputAvatar : IDataContainer
    {
        #region "Private fields"

        private readonly DateTime mInstantiated = DateTime.Now;

        private string mRedirectTo = "";

        private AvatarInfo mAvatar;

        private byte[] mOutputData;
        private string mMimeType;
        private bool mDataLoaded;

        #endregion


        #region "Properties"

        /// <summary>
        /// Avatar file info without AvatarBinary.
        /// </summary>
        public AvatarInfo Avatar
        {
            get
            {
                return mAvatar;
            }
            set
            {
                mAvatar = value;
                mAvatar.AvatarBinary = null;
            }
        }


        /// <summary>
        /// Output file data.
        /// </summary>
        public byte[] OutputData
        {
            get
            {
                return mOutputData;
            }
            set
            {
                mOutputData = value;
                mDataLoaded = true;
            }
        }


        /// <summary>
        /// Time when the avatar file was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                DateTime result;

                // Get the value
                if (mAvatar != null)
                {
                    result = mAvatar.AvatarLastModified;
                }
                else
                {
                    result = mInstantiated;
                }

                // Check if not in the future
                if (result > DateTime.Now)
                {
                    result = DateTime.Now;
                }

                return result;
            }
        }


        /// <summary>
        /// Requested output width.
        /// </summary>
        public int Width
        {
            get;
            set;
        }


        /// <summary>
        /// Requested output Height.
        /// </summary>
        public int Height
        {
            get;
            set;
        }


        /// <summary>
        /// Requested output MaxSideSize.
        /// </summary>
        public int MaxSideSize
        {
            get;
            set;
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
        /// Returns true if the data is loaded to the object.
        /// </summary>
        public bool DataLoaded
        {
            get
            {
                return mDataLoaded;
            }
        }


        /// <summary>
        /// Mime type.
        /// </summary>
        public string MimeType
        {
            get
            {
                if ((mMimeType == null) && (Avatar != null))
                {
                    return Avatar.AvatarMimeType;
                }
                else
                {
                    return mMimeType;
                }
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


        /// <summary>
        /// If true, the file is resized version of the file.
        /// </summary>
        public bool Resized
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CMSOutputAvatar()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ai">Avatar info</param>
        /// <param name="data">Output file data</param>
        public CMSOutputAvatar(AvatarInfo ai, byte[] data)
        {
            mAvatar = ai;
            mOutputData = data;

            mDataLoaded = ((ai == null) == (data == null));
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
                    // Load the file data
                    if (Avatar != null)
                    {
                        byte[] data = AvatarInfoProvider.GetAvatarFile(Avatar);
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

            return false;
        }


        /// <summary>
        /// Loads the data to the object.
        /// </summary>
        /// <param name="data">New data</param>
        public void LoadData(byte[] data)
        {
            if (data != null)
            {
                if (Avatar == null)
                {
                    throw new Exception("[CMSOutputAvatar.LoadData]: Cannot load data to the file object, the Avatar information is missing.");
                }

                // Resize the image if necessary
                if (AvatarInfoProvider.CanResizeImage(Avatar, Width, Height, MaxSideSize))
                {
                    Avatar.AvatarBinary = data;
                    data = AvatarInfoProvider.GetImageThumbnail(Avatar, Width, Height, MaxSideSize);

                    // Clear the attachment binary to save the memory
                    Avatar.AvatarBinary = null;
                    Resized = true;
                }
            }

            mOutputData = data;
            mDataLoaded = true;
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
                    "Avatar",
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
                case "avatar":
                    value = Avatar;
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