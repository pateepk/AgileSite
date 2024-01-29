using System;
using System.Data;
using System.Runtime.Serialization;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// ClassInfo data container class.
    /// </summary>
    public abstract class DataClassInfoBase<TInfo> : AbstractInfo<TInfo>
        where TInfo : DataClassInfoBase<TInfo>, new()
    {
        #region "Properties"

        /// <summary>
        /// Class ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassID"), 0);
            }
            set
            {
                SetValue("ClassID", value);
            }
        }


        /// <summary>
        /// Class display name
        /// </summary>
        [DatabaseField]
        public virtual string ClassDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassDisplayName"), "");
            }
            set
            {
                SetValue("ClassDisplayName", value);
            }
        }


        /// <summary>
        /// Class name
        /// </summary>
        [DatabaseField]
        public new virtual string ClassName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassName"), "");
            }
            set
            {
                SetValue("ClassName", value);
            }
        }


        /// <summary>
        /// Class uses versioning
        /// </summary>
        [DatabaseField]
        public virtual bool ClassUsesVersioning
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassUsesVersioning"), false);
            }
            set
            {
                SetValue("ClassUsesVersioning", value);
            }
        }


        /// <summary>
        /// Class is document type
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsDocumentType
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsDocumentType"), false);
            }
            set
            {
                SetValue("ClassIsDocumentType", value);
            }
        }


        /// <summary>
        /// Class is coupled class
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsCoupledClass
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsCoupledClass"), false);
            }
            set
            {
                SetValue("ClassIsCoupledClass", value);
            }
        }


        /// <summary>
        /// Class xml schema
        /// </summary>
        [DatabaseField]
        public virtual string ClassXmlSchema
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassXmlSchema"), "");
            }
            set
            {
                SetValue("ClassXmlSchema", value);
            }
        }


        /// <summary>
        /// Class form definition
        /// </summary>
        [DatabaseField]
        public virtual string ClassFormDefinition
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassFormDefinition"), "");
            }
            set
            {
                SetValue("ClassFormDefinition", value);
            }
        }


        /// <summary>
        /// Class editing page url
        /// </summary>
        [DatabaseField]
        public virtual string ClassEditingPageURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassEditingPageUrl"), "");
            }
            set
            {
                SetValue("ClassEditingPageUrl", value);
            }
        }


        /// <summary>
        /// Class list page url
        /// </summary>
        [DatabaseField]
        public virtual string ClassListPageURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassListPageUrl"), "");
            }
            set
            {
                SetValue("ClassListPageUrl", value);
            }
        }


        /// <summary>
        /// Class node name source
        /// </summary>
        [DatabaseField]
        public virtual string ClassNodeNameSource
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassNodeNameSource"), "");
            }
            set
            {
                SetValue("ClassNodeNameSource", value);
            }
        }


        /// <summary>
        /// Class table name
        /// </summary>
        [DatabaseField]
        public virtual string ClassTableName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassTableName"), "");
            }
            set
            {
                SetValue("ClassTableName", value);
            }
        }


        /// <summary>
        /// Class view page url
        /// </summary>
        [DatabaseField]
        public virtual string ClassViewPageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassViewPageUrl"), "");
            }
            set
            {
                SetValue("ClassViewPageUrl", value);
            }
        }


        /// <summary>
        /// Class preview page url
        /// </summary>
        [DatabaseField]
        public virtual string ClassPreviewPageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassPreviewPageUrl"), "");
            }
            set
            {
                SetValue("ClassPreviewPageUrl", value);
            }
        }


        /// <summary>
        /// Class form layout
        /// </summary>
        [DatabaseField]
        public virtual string ClassFormLayout
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassFormLayout"), "");
            }
            set
            {
                SetValue("ClassFormLayout", value);
            }
        }


        /// <summary>
        /// Class new page url
        /// </summary>
        [DatabaseField]
        public virtual string ClassNewPageURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassNewPageUrl"), "");
            }
            set
            {
                SetValue("ClassNewPageUrl", value);
            }
        }


        /// <summary>
        /// Class show as system table
        /// </summary>
        [DatabaseField]
        public virtual bool ClassShowAsSystemTable
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassShowAsSystemTable"), false);
            }
            set
            {
                SetValue("ClassShowAsSystemTable", value);
            }
        }


        /// <summary>
        /// Class use publish from to
        /// </summary>
        [DatabaseField]
        public virtual bool ClassUsePublishFromTo
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassUsePublishFromTo"), false);
            }
            set
            {
                SetValue("ClassUsePublishFromTo", value);
            }
        }


        /// <summary>
        /// Class show template selection
        /// </summary>
        [DatabaseField]
        public virtual bool ClassShowTemplateSelection
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassShowTemplateSelection"), false);
            }
            set
            {
                SetValue("ClassShowTemplateSelection", value);
            }
        }

        /// <summary>
        /// Class is menu item type
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsMenuItemType
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsMenuItemType"), false);
            }
            set
            {
                SetValue("ClassIsMenuItemType", value);
            }
        }


        /// <summary>
        /// Class node alias source
        /// </summary>
        [DatabaseField]
        public virtual string ClassNodeAliasSource
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassNodeAliasSource"), "");
            }
            set
            {
                SetValue("ClassNodeAliasSource", value);
            }
        }


        /// <summary>
        /// Class default page template ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassDefaultPageTemplateID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassDefaultPageTemplateID"), 0);
            }
            set
            {
                SetValue("ClassDefaultPageTemplateID", value, 0);
            }
        }


        /// <summary>
        /// Class last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime ClassLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ClassLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ClassLastModified", value);
            }
        }


        /// <summary>
        /// Class GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid ClassGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ClassGUID"), Guid.Empty);
            }
            set
            {
                SetValue("ClassGUID", value);
            }
        }


        /// <summary>
        /// Class is product
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsProduct
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsProduct"), false);
            }
            set
            {
                SetValue("ClassIsProduct", value);
            }
        }


        /// <summary>
        /// Class SKU default department name
        /// </summary>
        [DatabaseField]
        public virtual string ClassSKUDefaultDepartmentName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSKUDefaultDepartmentName"), "");
            }
            set
            {
                SetValue("ClassSKUDefaultDepartmentName", value);
            }
        }


        /// <summary>
        /// Class SKU default department ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassSKUDefaultDepartmentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassSKUDefaultDepartmentID"), 0);
            }
            set
            {
                SetValue("ClassSKUDefaultDepartmentID", value, 0);
            }
        }


        /// <summary>
        /// Class SKU default product type
        /// </summary>
        [DatabaseField]
        public virtual string ClassSKUDefaultProductType
        {
            get
            {
                return DataHelper.GetNotEmpty(GetValue("ClassSKUDefaultProductType"), "PRODUCT");
            }
            set
            {
                SetValue("ClassSKUDefaultProductType", value);
            }
        }


        /// <summary>
        /// Class is custom table
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsCustomTable
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsCustomTable"), false);
            }
            set
            {
                SetValue("ClassIsCustomTable", value);
            }
        }


        /// <summary>
        /// Class show columns
        /// </summary>
        [DatabaseField]
        public virtual string ClassShowColumns
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassShowColumns"), "");
            }
            set
            {
                SetValue("ClassShowColumns", value);
            }
        }


        /// <summary>
        /// Class search title column
        /// </summary>
        [DatabaseField]
        public virtual string ClassSearchTitleColumn
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSearchTitleColumn"), "");
            }
            set
            {
                SetValue("ClassSearchTitleColumn", value);
            }
        }


        /// <summary>
        /// Class search content column
        /// </summary>
        [DatabaseField]
        public virtual string ClassSearchContentColumn
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSearchContentColumn"), "");
            }
            set
            {
                SetValue("ClassSearchContentColumn", value);
            }
        }


        /// <summary>
        /// Class search image column
        /// </summary>
        [DatabaseField]
        public virtual string ClassSearchImageColumn
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSearchImageColumn"), "");
            }
            set
            {
                SetValue("ClassSearchImageColumn", value);
            }
        }


        /// <summary>
        /// Class search creation date column
        /// </summary>
        [DatabaseField]
        public virtual string ClassSearchCreationDateColumn
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSearchCreationDateColumn"), "");
            }
            set
            {
                SetValue("ClassSearchCreationDateColumn", value);
            }
        }


        /// <summary>
        /// Class search settings
        /// </summary>
        [DatabaseField]
        public virtual string ClassSearchSettings
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassSearchSettings"), "");
            }
            set
            {
                SetValue("ClassSearchSettings", value);
            }
        }


        /// <summary>
        /// Class inherits from class ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassInheritsFromClassID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassInheritsFromClassID"), 0);
            }
            set
            {
                SetValue("ClassInheritsFromClassID", value, 0);
            }
        }


        /// <summary>
        /// Class connection string
        /// </summary>
        [DatabaseField]
        public virtual string ClassConnectionString
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassConnectionString"), "");
            }
            set
            {
                SetValue("ClassConnectionString", value);
            }
        }


        /// <summary>
        /// Class search enabled
        /// </summary>
        [DatabaseField]
        public virtual bool ClassSearchEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassSearchEnabled"), false);
            }
            set
            {
                SetValue("ClassSearchEnabled", value);
            }
        }


        /// <summary>
        /// Class contact mapping
        /// </summary>
        [DatabaseField]
        public virtual string ClassContactMapping
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassContactMapping"), "");
            }
            set
            {
                SetValue("ClassContactMapping", value);
            }
        }


        /// <summary>
        /// Class contact overwrite enabled
        /// </summary>
        [DatabaseField]
        public virtual bool ClassContactOverwriteEnabled
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassContactOverwriteEnabled"), false);
            }
            set
            {
                SetValue("ClassContactOverwriteEnabled", value);
            }
        }


        /// <summary>
        /// Class is product section
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsProductSection
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsProductSection"), false);
            }
            set
            {
                SetValue("ClassIsProductSection", value);
            }
        }


        /// <summary>
        /// Class page template category ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassPageTemplateCategoryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassPageTemplateCategoryID"), 0);
            }
            set
            {
                SetValue("ClassPageTemplateCategoryID", value, 0);
            }
        }


        /// <summary>
        /// Class form layout type
        /// </summary>
        [DatabaseField]
        public virtual string ClassFormLayoutType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassFormLayoutType"), "");
            }
            set
            {
                SetValue("ClassFormLayoutType", value);
            }
        }


        /// <summary>
        /// Class version GUID
        /// </summary>
        [DatabaseField]
        public virtual string ClassVersionGUID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassVersionGUID"), "");
            }
            set
            {
                SetValue("ClassVersionGUID", value);
            }
        }


        /// <summary>
        /// Class default object type
        /// </summary>
        [DatabaseField]
        public virtual string ClassDefaultObjectType
        {
            get
            {
                if (ClassIsCustomTable)
                {
                    return PredefinedObjectType.CUSTOM_TABLE_ITEM_PREFIX + ClassName.ToLowerCSafe();
                }

                if (ClassIsForm)
                {
                    return PredefinedObjectType.BIZFORM_ITEM_PREFIX + ClassName.ToLowerCSafe();
                }

                return ValidationHelper.GetString(GetValue("ClassDefaultObjectType"), "");
            }
            set
            {
                SetValue("ClassDefaultObjectType", value);
            }
        }


        /// <summary>
        /// Class is form
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsForm
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsForm"), false);
            }
            set
            {
                SetValue("ClassIsForm", value);
            }
        }


        /// <summary>
        /// Class resource ID
        /// </summary>
        [DatabaseField]
        public virtual int ClassResourceID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClassResourceID"), 0);
            }
            set
            {
                SetValue("ClassResourceID", value, 0);
            }
        }


        /// <summary>
        /// Class customized columns
        /// </summary>
        [DatabaseField]
        public virtual string ClassCustomizedColumns
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassCustomizedColumns"), "");
            }
            set
            {
                SetValue("ClassCustomizedColumns", value);
            }
        }


        /// <summary>
        /// Class code generation settings
        /// </summary>
        [DatabaseField]
        public virtual string ClassCodeGenerationSettings
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassCodeGenerationSettings"), "");
            }
            set
            {
                SetValue("ClassCodeGenerationSettings", value);
            }
        }


        /// <summary>
        /// Class is content-only. Used for page types.
        /// </summary>
        [DatabaseField]
        public virtual bool ClassIsContentOnly
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ClassIsContentOnly"), false);
            }
            set
            {
                SetValue("ClassIsContentOnly", value);
            }
        }


        /// <summary>
        /// URL pattern used for page types.
        /// </summary>
        [DatabaseField]
        public virtual string ClassURLPattern
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClassURLPattern"), "");
            }
            set
            {
                SetValue("ClassURLPattern", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ClassInfo object.
        /// </summary>
        protected DataClassInfoBase(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ClassInfo object from the given DataRow.
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dr">DataRow with the object data</param>
        protected DataClassInfoBase(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo, dr)
        {
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected DataClassInfoBase(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
            : base(info, context, typeInfos)
        {
        }

        #endregion
    }
}