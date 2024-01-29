using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(DocumentTypeScopeInfo), DocumentTypeScopeInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// DocumentTypeScopeInfo data container class.
    /// </summary>
    public class DocumentTypeScopeInfo : AbstractInfo<DocumentTypeScopeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.documenttypescope";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentTypeScopeInfoProvider), OBJECT_TYPE, "CMS.DocumentTypeScope", "ScopeID", "ScopeLastModified", "ScopeGUID", null, "ScopePath", null, "ScopeSiteID", null, null)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT)
                }
            },

            LogEvents = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = false,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                AllowSingleExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT)
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "ScopeGUID",
                ObjectFileNameFields = { "ScopePath" }
            },
            SupportsCloning = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Document type scope GUID.
        /// </summary>
        public virtual Guid ScopeGUID
        {
            get
            {
                return GetGuidValue("ScopeGUID", Guid.Empty);
            }
            set
            {
                SetValue("ScopeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Document type scope last modified date.
        /// </summary>
        public virtual DateTime ScopeLastModified
        {
            get
            {
                return GetDateTimeValue(" ScopeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ScopeLastModified", value);
            }
        }


        /// <summary>
        /// Document type scope identifier.
        /// </summary>
        public virtual int ScopeID
        {
            get
            {
                return GetIntegerValue("ScopeID", 0);
            }
            set
            {
                SetValue("ScopeID", value);
            }
        }


        /// <summary>
        /// Document type scope site identifier.
        /// </summary>
        public virtual int ScopeSiteID
        {
            get
            {
                return GetIntegerValue("ScopeSiteID", 0);
            }
            set
            {
                SetValue("ScopeSiteID", value);
            }
        }


        /// <summary>
        /// Document type scope starting path.
        /// </summary>
        public virtual string ScopePath
        {
            get
            {
                return GetStringValue("ScopePath", "");
            }
            set
            {
                SetValue("ScopePath", value);
            }
        }


        /// <summary>
        /// Indicates if scope is applied to children documents.
        /// </summary>
        public virtual bool ScopeIncludeChildren
        {
            get
            {
                return GetBooleanValue("ScopeIncludeChildren", false);
            }
            set
            {
                SetValue("ScopeIncludeChildren", value);
            }
        }


        /// <summary>
        /// Indicates if scope allows all document types.
        /// </summary>
        public virtual bool ScopeAllowAllTypes
        {
            get
            {
                return GetBooleanValue("ScopeAllowAllTypes", false);
            }
            set
            {
                SetValue("ScopeAllowAllTypes", value);
            }
        }


        /// <summary>
        /// Indicates if scope allows creating linked documents.
        /// </summary>
        public virtual bool ScopeAllowLinks
        {
            get
            {
                return GetBooleanValue("ScopeAllowLinks", false);
            }
            set
            {
                SetValue("ScopeAllowLinks", value);
            }
        }


        /// <summary>
        /// Indicates if scope allows creating A/B test variants.
        /// </summary>
        public virtual bool ScopeAllowABVariant
        {
            get
            {
                return GetBooleanValue("ScopeAllowABVariant", false);
            }
            set
            {
                SetValue("ScopeAllowABVariant", value);
            }
        }


        /// <summary>
        /// Document type scope macro condition.
        /// </summary>
        public virtual string ScopeMacroCondition
        {
            get
            {
                return GetStringValue("ScopeMacroCondition", "");
            }
            set
            {
                SetValue("ScopeMacroCondition", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentTypeScopeInfoProvider.DeleteScopeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentTypeScopeInfoProvider.SetScopeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DocumentTypeScopeInfo object.
        /// </summary>
        public DocumentTypeScopeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DocumentTypeScopeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DocumentTypeScopeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
