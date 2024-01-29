using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(SKUFileInfo), SKUFileInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKUFileInfo data container class.
    /// </summary>
    public class SKUFileInfo : AbstractInfo<SKUFileInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.skufile";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SKUFileInfoProvider), OBJECT_TYPE, "ECommerce.SKUFile", "FileID", "FileLastModified", "FileGUID", null, null, null, null, "FileSKUID", SKUInfo.OBJECT_TYPE_SKU)
        {
            // Child object types
            // - None

            // Object dependencies
            // - None

            // Binding object types
            // - None

            // Others
            LogEvents = false,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            RegisterAsChildToObjectTypes = new List<string> { SKUInfo.OBJECT_TYPE_SKU, SKUInfo.OBJECT_TYPE_OPTIONSKU },
            ImportExportSettings = { LogExport = false },
            HasMetaFiles = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "FileGUID",
                ObjectFileNameFields = { "FileName"}
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// File type.
        /// </summary>
        public virtual string FileType
        {
            get
            {
                return GetStringValue("FileType", "");
            }
            set
            {
                SetValue("FileType", value);
            }
        }


        /// <summary>
        /// File name.
        /// </summary>
        public string FileName
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
        /// Application root relative path to the file.
        /// </summary>
        public virtual string FilePath
        {
            get
            {
                return GetStringValue("FilePath", "");
            }
            set
            {
                SetValue("FilePath", value);
            }
        }


        /// <summary>
        /// Date and time when the file object was last modified.
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
        /// File GUID.
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
        /// File ID.
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
        /// Parent SKU ID.
        /// </summary>
        public virtual int FileSKUID
        {
            get
            {
                return GetIntegerValue("FileSKUID", 0);
            }
            set
            {
                SetValue("FileSKUID", value);
            }
        }


        /// <summary>
        /// Associated meta file GUID.
        /// </summary>
        public virtual Guid FileMetaFileGUID
        {
            get
            {
                return GetGuidValue("FileMetaFileGUID", Guid.Empty);
            }
            set
            {
                SetValue("FileMetaFileGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SKUFileInfoProvider.DeleteSKUFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SKUFileInfoProvider.SetSKUFileInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SKUFileInfo object.
        /// </summary>
        public SKUFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SKUFileInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SKUFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return EcommercePermissions.CheckProductsPermissions(permission, siteName, userInfo, exceptionOnFailure, IsGlobal, base.CheckPermissionsInternal);
        }

        #endregion
    }
}