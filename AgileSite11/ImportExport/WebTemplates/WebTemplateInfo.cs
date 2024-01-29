using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.CMSImportExport;

[assembly: RegisterObjectType(typeof(WebTemplateInfo), WebTemplateInfo.OBJECT_TYPE)]

namespace CMS.CMSImportExport
{
    /// <summary>
    /// WebPart info data container class.
    /// </summary>
    public class WebTemplateInfo : AbstractInfo<WebTemplateInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webtemplate";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebTemplateInfoProvider), OBJECT_TYPE, "CMS.WebTemplate", "WebTemplateID", "WebTemplateLastModified", "WebTemplateGUID", "WebTemplateName", "WebTemplateDisplayName", null, null, null, null)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,

            AllowRestore = false,
            ThumbnailGUIDColumn = "WebTemplateThumbnailGUID",
            HasMetaFiles = true,
            OrderColumn = "WebTemplateOrder",
            DefaultData = new DefaultDataSettings
            {
                ExcludedPrefixes = { "old" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// The WebTemplateId.
        /// </summary>
        [DatabaseField("WebTemplateID")]
        public virtual int WebTemplateId
        {
            get
            {
                return GetIntegerValue("WebTemplateID", 0);
            }
            set
            {
                SetValue("WebTemplateID", value);
            }
        }


        /// <summary>
        /// The WebTemplateName.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateName
        {
            get
            {
                return GetStringValue("WebTemplateName", "");
            }
            set
            {
                SetValue("WebTemplateName", value);
            }
        }


        /// <summary>
        /// The WebTemplateDisplayName.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateDisplayName
        {
            get
            {
                return GetStringValue("WebTemplateDisplayName", "");
            }
            set
            {
                SetValue("WebTemplateDisplayName", value);
            }
        }


        /// <summary>
        /// The WebTemplateFileName.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateFileName
        {
            get
            {
                return GetStringValue("WebTemplateFileName", "");
            }
            set
            {
                SetValue("WebTemplateFileName", value);
            }
        }


        /// <summary>
        /// The WebTemplateDescription.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateDescription
        {
            get
            {
                return GetStringValue("WebTemplateDescription", "");
            }
            set
            {
                SetValue("WebTemplateDescription", value);
            }
        }


        /// <summary>
        /// Web template short description.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateShortDescription
        {
            get
            {
                return GetStringValue("WebTemplateShortDescription", "");
            }
            set
            {
                SetValue("WebTemplateShortDescription", value);
            }
        }


        /// <summary>
        /// The WebTemplateLicenses.
        /// </summary>
        [DatabaseField]
        public virtual string WebTemplateLicenses
        {
            get
            {
                return GetStringValue("WebTemplateLicenses", "");
            }
            set
            {
                SetValue("WebTemplateLicenses", value);
            }
        }


        /// <summary>
        /// WebTemplate GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebTemplateGUID
        {
            get
            {
                return GetGuidValue("WebTemplateGUID", Guid.Empty);
            }
            set
            {
                SetValue("WebTemplateGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime WebTemplateLastModified
        {
            get
            {
                return GetDateTimeValue("WebTemplateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WebTemplateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Web template order index.
        /// </summary>
        [DatabaseField]
        public virtual int WebTemplateOrder
        {
            get
            {
                return GetIntegerValue("WebTemplateOrder", 0);
            }
            set
            {
                SetValue("WebTemplateOrder", value);
            }
        }


        /// <summary>
        /// Web template thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebTemplateThumbnailGUID
        {
            get
            {
                return GetGuidValue("WebTemplateThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("WebTemplateThumbnailGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebTemplateInfoProvider.DeleteWebTemplateInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebTemplateInfoProvider.SetWebTemplateInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty WebTemplateInfo structure.
        /// </summary>
        public WebTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty WebTemplateInfo object from the given DataRow data.
        /// </summary>
        public WebTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}