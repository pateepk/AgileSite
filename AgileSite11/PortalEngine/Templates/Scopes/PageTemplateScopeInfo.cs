using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.Localization;
using CMS.DataEngine;
using CMS.Core;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(PageTemplateScopeInfo), PageTemplateScopeInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// PageTemplateScopeInfo data container class.
    /// </summary>
    public class PageTemplateScopeInfo : AbstractInfo<PageTemplateScopeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.PAGETEMPLATESCOPE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateScopeInfoProvider), OBJECT_TYPE, "CMS.PageTemplateScope", "PageTemplateScopeID", "PageTemplateScopeLastModified", "PageTemplateScopeGUID", null, null, null, "PageTemplateScopeSiteID", "PageTemplateScopeTemplateID", PageTemplateInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("PageTemplateScopeCultureID", CultureInfo.OBJECT_TYPE), 
                new ObjectDependency("PageTemplateScopeClassID", PredefinedObjectType.DOCUMENTTYPE)
            },
            ModuleName = ModuleName.DESIGN,
            ImportExportSettings = { IsExportable = true, IsAutomaticallySelected = true, LogExport = false, IncludeToExportParentDataSet = IncludeToParentEnum.None },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsGlobalObjects = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "PageTemplateScopeGUID",
                ObjectFileNameFields = { "PageTemplateScopePath" }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Page template scope id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateScopeID
        {
            get
            {
                return GetIntegerValue("PageTemplateScopeID", 0);
            }
            set
            {
                SetValue("PageTemplateScopeID", value);
            }
        }


        /// <summary>
        /// Page template scope culture id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateScopeCultureID
        {
            get
            {
                return GetIntegerValue("PageTemplateScopeCultureID", 0);
            }
            set
            {
                SetValue("PageTemplateScopeCultureID", value);
            }
        }


        /// <summary>
        /// Page template scope path.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateScopePath
        {
            get
            {
                return GetStringValue("PageTemplateScopePath", "");
            }
            set
            {
                SetValue("PageTemplateScopePath", value);
            }
        }


        /// <summary>
        /// Page template scope class id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateScopeClassID
        {
            get
            {
                return GetIntegerValue("PageTemplateScopeClassID", 0);
            }
            set
            {
                SetValue("PageTemplateScopeClassID", value);
            }
        }


        /// <summary>
        /// Page template scope levels.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateScopeLevels
        {
            get
            {
                return GetStringValue("PageTemplateScopeLevels", "");
            }
            set
            {
                SetValue("PageTemplateScopeLevels", value);
            }
        }


        /// <summary>
        /// Page template scope site id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateScopeSiteID
        {
            get
            {
                return GetIntegerValue("PageTemplateScopeSiteID", 0);
            }
            set
            {
                SetValue("PageTemplateScopeSiteID", value);
            }
        }


        /// <summary>
        /// Page template scope template id.
        /// </summary>
        [DatabaseField]
        public virtual int PageTemplateScopeTemplateID
        {
            get
            {
                return GetIntegerValue("PageTemplateScopeTemplateID", 0);
            }
            set
            {
                SetValue("PageTemplateScopeTemplateID", value);
            }
        }


        /// <summary>
        /// Scope step GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageTemplateScopeGUID
        {
            get
            {
                return GetGuidValue("PageTemplateScopeGUID", Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateScopeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageTemplateScopeLastModified
        {
            get
            {
                return GetDateTimeValue("PageTemplateScopeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageTemplateScopeLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageTemplateScopeInfoProvider.DeletePageTemplateScopeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageTemplateScopeInfoProvider.SetPageTemplateScopeInfo(this);
        }


        /// <summary>
        /// Updates the object.
        /// </summary>
        protected override void UpdateData()
        {
            base.UpdateData();

            TouchParent();
        }


        /// <summary>
        /// Inserts the object.
        /// </summary>
        protected override void InsertData()
        {
            base.InsertData();

            TouchParent();
        }


        /// <summary>
        /// Deletes the object.
        /// </summary>
        protected override void DeleteData()
        {
            base.DeleteData();

            TouchParent();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PageTemplateScopeInfo object.
        /// </summary>
        public PageTemplateScopeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PageTemplateScopeInfo object from the given DataRow.
        /// </summary>
        public PageTemplateScopeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Used to actualize time stamp of parent object for incremental deployment
        /// It's necessary to do not create parent object version, staging task or event log
        /// </summary>
        protected override void TouchParent()
        {
            GeneralizedInfo parent = ObjectParent;
            if (parent != null)
            {
                // Do not log events when the parent is just touched
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.LogSynchronization = false;
                    context.CreateVersion = false;
                    context.LogEvents = false;

                    // Set object
                    parent.SetObject();
                }
            }
        }

        #endregion
    }
}