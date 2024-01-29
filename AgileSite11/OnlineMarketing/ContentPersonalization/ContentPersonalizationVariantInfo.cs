using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(ContentPersonalizationVariantInfo), ContentPersonalizationVariantInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(ContentPersonalizationVariantInfo), ContentPersonalizationVariantInfo.OBJECT_TYPE_DOCUMENTVARIANT)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// ContentPersonalizationVariant data container class.
    /// </summary>
    public class ContentPersonalizationVariantInfo : AbstractInfo<ContentPersonalizationVariantInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CONTENTPERSONALIZATIONVARIANT;

        /// <summary>
        /// Object type for document variant
        /// </summary>
        public const string OBJECT_TYPE_DOCUMENTVARIANT = PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContentPersonalizationVariantInfoProvider), OBJECT_TYPE, "OM.PersonalizationVariant", "VariantID", "VariantLastModified", "VariantGUID", "VariantName", "VariantDisplayName", null, null, "VariantPageTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNull("VariantDocumentID"),
            OrderColumn = "VariantPosition",
            EnabledColumn = "VariantEnabled",
            ImportExportSettings = { LogExport = false },
            Feature = FeatureEnum.ContentPersonalization,
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<WebPartsStructuredData>("VariantWebParts")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_DOCUMENT = new ObjectTypeInfo(typeof(ContentPersonalizationVariantInfoProvider), OBJECT_TYPE_DOCUMENTVARIANT, "OM.PersonalizationVariant", "VariantID", "VariantLastModified", "VariantGUID", "VariantName", "VariantDisplayName", null, null, "VariantDocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("VariantPageTemplateID", PageTemplateInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None
            },
            SupportsVersioning = false,
            OriginalTypeInfo = TYPEINFO,
            ModuleName = ModuleName.ONLINEMARKETING,
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsCloning = false,
            TypeCondition = new TypeCondition().WhereIsNotNull("VariantDocumentID"),
            OrderColumn = "VariantPosition",
            EnabledColumn = "VariantEnabled",
            ImportExportSettings = { LogExport = false },
            Feature = FeatureEnum.ContentPersonalization,
            AllowTouchParent = false,
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<WebPartsStructuredData>("VariantWebParts")
                }       
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Personalization variant instance GUID.
        /// </summary>
        public virtual Guid VariantInstanceGUID
        {
            get
            {
                return GetGuidValue("VariantInstanceGUID", Guid.Empty);
            }
            set
            {
                SetValue("VariantInstanceGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Personalization variant position.
        /// </summary>
        public virtual int VariantPosition
        {
            get
            {
                return GetIntegerValue("VariantPosition", 0);
            }
            set
            {
                SetValue("VariantPosition", value);
            }
        }


        /// <summary>
        /// Personalization variant display condition.
        /// </summary>
        public virtual string VariantDisplayCondition
        {
            get
            {
                return GetStringValue("VariantDisplayCondition", "");
            }
            set
            {
                SetValue("VariantDisplayCondition", value);
            }
        }


        /// <summary>
        /// Personalization variant web parts.
        /// </summary>
        public virtual string VariantWebParts
        {
            get
            {
                return GetStringValue("VariantWebParts", "");
            }
            set
            {
                SetValue("VariantWebParts", value);
            }
        }


        /// <summary>
        /// Personalization variant last modified.
        /// </summary>
        public virtual DateTime VariantLastModified
        {
            get
            {
                return GetDateTimeValue("VariantLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VariantLastModified", value);
            }
        }


        /// <summary>
        /// Personalization variant code name.
        /// </summary>
        public virtual string VariantName
        {
            get
            {
                return GetStringValue("VariantName", "");
            }
            set
            {
                SetValue("VariantName", value);
            }
        }


        /// <summary>
        /// Personalization variant id.
        /// </summary>
        public virtual int VariantID
        {
            get
            {
                return GetIntegerValue("VariantID", 0);
            }
            set
            {
                SetValue("VariantID", value);
            }
        }


        /// <summary>
        /// Personalization variant description.
        /// </summary>
        public virtual string VariantDescription
        {
            get
            {
                return GetStringValue("VariantDescription", "");
            }
            set
            {
                SetValue("VariantDescription", value);
            }
        }


        /// <summary>
        /// Personalization variant display name.
        /// </summary>
        public virtual string VariantDisplayName
        {
            get
            {
                return GetStringValue("VariantDisplayName", "");
            }
            set
            {
                SetValue("VariantDisplayName", value);
            }
        }


        /// <summary>
        /// Personalization variant enabled.
        /// </summary>
        public virtual bool VariantEnabled
        {
            get
            {
                return GetBooleanValue("VariantEnabled", false);
            }
            set
            {
                SetValue("VariantEnabled", value);
            }
        }


        /// <summary>
        /// Personalization variant document id.
        /// </summary>
        public virtual int VariantDocumentID
        {
            get
            {
                return GetIntegerValue("VariantDocumentID", 0);
            }
            set
            {
                SetValue("VariantDocumentID", value, (value > 0));
            }
        }


        /// <summary>
        /// Personalization variant page template id.
        /// </summary>
        public virtual int VariantPageTemplateID
        {
            get
            {
                return GetIntegerValue("VariantPageTemplateID", 0);
            }
            set
            {
                SetValue("VariantPageTemplateID", value);
            }
        }


        /// <summary>
        /// Personalization variant zone id.
        /// </summary>
        public virtual string VariantZoneID
        {
            get
            {
                return GetStringValue("VariantZoneID", "");
            }
            set
            {
                SetValue("VariantZoneID", value);
            }
        }


        /// <summary>
        /// Personalization variant GUID.
        /// </summary>
        public virtual Guid VariantGUID
        {
            get
            {
                return GetGuidValue("VariantGUID", Guid.Empty);
            }
            set
            {
                SetValue("VariantGUID", value);
            }
        }


        /// <summary>
        /// Variant web part/widget instance containing data built from this info object.
        /// </summary>
        public virtual WebPartInstance WebPartInstance
        {
            get;
            set;
        }


        /// <summary>
        /// Variant zone instance containing data built from this info object.
        /// </summary>
        public virtual WebPartZoneInstance WebPartZoneInstance
        {
            get;
            set;
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (VariantDocumentID > 0)
                {
                    return TYPEINFO_DOCUMENT;
                }
                else
                {
                    return TYPEINFO;
                }
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContentPersonalizationVariantInfoProvider.DeleteContentPersonalizationVariant(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContentPersonalizationVariantInfoProvider.SetContentPersonalizationVariant(this);
        }


        /// <summary>
        /// Gets the automatic code name for the object.
        /// </summary>
        /// <remarks>
        /// There are many cases where the codename has to or has not to be unique. To avoid necessary complexity and prevent further defects, GUID is used for the code name instead.
        /// </remarks>
        protected override string GetAutomaticCodeName()
        {
            var value = ValidationHelper.GetCodeName(ResHelper.LocalizeString(ObjectDisplayName), useUnicode: false);
            return value + "_" + Guid.NewGuid();
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            // Clear the cache
            CacheHelper.TouchKey("om.personalizationvariant|bytemplateid|" + VariantPageTemplateID);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContentPersonalizationVariant object.
        /// </summary>
        public ContentPersonalizationVariantInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContentPersonalizationVariant object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ContentPersonalizationVariantInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}