using System;
using System.Collections;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(DocumentAliasInfo), DocumentAliasInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// DocumentAliasInfo data container class.
    /// </summary>
    public class DocumentAliasInfo : AbstractInfo<DocumentAliasInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.documentalias";


        /// <summary>
        /// Type information.
        /// </summary>      
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentAliasInfoProvider), OBJECT_TYPE, "CMS.DocumentAlias", "AliasID", "AliasLastModified", "AliasGUID", null, "AliasURLPath", null, "AliasSiteID", "AliasNodeID", DocumentNodeDataInfo.OBJECT_TYPE)
        {
            ModuleName = "cms.content",
            AllowRestore = false,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "AliasURLPath" }
            },
            TouchCacheDependencies = true,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the document alias action mode
        /// </summary>
        public virtual AliasActionModeEnum AliasActionMode
        {
            get
            {
                return GetStringValue("AliasActionMode", String.Empty).ToEnum<AliasActionModeEnum>();
            }
            set
            {
                SetValue("AliasActionMode", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Alias GUID.
        /// </summary>
        public virtual Guid AliasGUID
        {
            get
            {
                return GetGuidValue("AliasGUID", Guid.Empty);
            }
            set
            {
                SetValue("AliasGUID", value);
            }
        }


        /// <summary>
        /// Alias last modified.
        /// </summary>
        public virtual DateTime AliasLastModified
        {
            get
            {
                return GetDateTimeValue("AliasLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SiteLastModified", value);
            }
        }


        /// <summary>
        /// Node ID of alias.
        /// </summary>
        public virtual int AliasNodeID
        {
            get
            {
                return GetIntegerValue("AliasNodeID", 0);
            }
            set
            {
                SetValue("AliasNodeID", value);
            }
        }


        /// <summary>
        /// Alias extensions.
        /// </summary>
        public virtual string AliasExtensions
        {
            get
            {
                return GetStringValue("AliasExtensions", "");
            }
            set
            {
                SetValue("AliasExtensions", value);
            }
        }


        /// <summary>
        /// Alias ID.
        /// </summary>
        public virtual int AliasID
        {
            get
            {
                return GetIntegerValue("AliasID", 0);
            }
            set
            {
                SetValue("AliasID", value);
            }
        }


        /// <summary>
        /// Alias culture.
        /// </summary>
        public virtual string AliasCulture
        {
            get
            {
                return GetStringValue("AliasCulture", "");
            }
            set
            {
                SetValue("AliasCulture", value);
            }
        }


        /// <summary>
        /// Alias URL path.
        /// </summary>
        public virtual string AliasURLPath
        {
            get
            {
                return GetStringValue("AliasURLPath", "");
            }
            set
            {
                SetValue("AliasURLPath", value);
            }
        }


        /// <summary>
        /// Alias wildcard rule.
        /// </summary>
        public virtual string AliasWildcardRule
        {
            get
            {
                return GetStringValue("AliasWildcardRule", "");
            }
            set
            {
                SetValue("AliasWildcardRule", value);
            }
        }


        /// <summary>
        /// This number indicates how many URL sections without parameter contains AliasURLPath.
        /// The higher number indicates more specific AliasURLPath.
        /// </summary>
        /// <example>
        /// AliasURLPath = "/section/section/section/{param}", AliasPriority = 3
        /// AliasURLPath = "/{param}/{param2}", AliasPriority = 0 (very general alias)
        /// </example>
        public virtual int AliasPriority
        {
            get
            {
                return GetIntegerValue("AliasPriority", 0);
            }
            set
            {
                SetValue("AliasPriority", value);
            }
        }


        /// <summary>
        /// Alias Site ID.
        /// </summary>
        public virtual int AliasSiteID
        {
            get
            {
                return GetIntegerValue("AliasSiteID", 0);
            }
            set
            {
                SetValue("AliasSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentAliasInfoProvider.DeleteDocumentAliasInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentAliasInfoProvider.SetDocumentAliasInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DocumentAliasInfo object.
        /// </summary>
        public DocumentAliasInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DocumentAliasInfo object from the given DataRow.
        /// </summary>
        public DocumentAliasInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                AliasURLPath = ValidationHelper.GetString(p["cms.documentalias" + ".aliasurlpath"], AliasURLPath);
            }

            Insert();
        }

        #endregion
    }
}