using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Data context.
    /// </summary>
    public class CMSDataContext : CMSDataContextBase<CMSDataContext>
    {
        #region "Context"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSDataContext()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public CMSDataContext(string siteName)
            : base(siteName)
        {
        }

        #endregion


        #region "Variables"

        private CSSContainer mCSS;
        private BrowserHelper mBrowserHelper;
        private CurrentDevice mCurrentDevice;
        private DeviceProfileInfo mDeviceProfileInfo;
        private String mDeviceProfileInfoName;

        private InfoObjectRepository mGlobalObjects;
        private bool globalObjectsLoaded;

        private IInfoObjectCollection<SiteInfo> mAllSites;

        private FullNameInfoObjectCollection<TransformationInfo> mTransformations;
        private FullNameInfoObjectCollection<QueryInfo> mQueries;
        private FullNameInfoObjectCollection<AlternativeFormInfo> mAlternativeForms;

        private static readonly RequestStockValue<CMSDataContext> mCurrent = new RequestStockValue<CMSDataContext>("CurrentCMSDataContext", () => new CMSDataContext());

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns current data context.
        /// </summary>
        public static CMSDataContext Current
        {
            get
            {
                return mCurrent;
            }
            set
            {
                mCurrent.Value = value;
            }
        }


        /// <summary>
        /// Browser information
        /// </summary>
        public virtual BrowserHelper BrowserHelper
        {
            get
            {
                mBrowserHelper = (BrowserHelper)SessionHelper.GetValue("BrowserHelper");

                if (mBrowserHelper == null)
                {
                    mBrowserHelper = new BrowserHelper();
                    SessionHelper.SetValue("BrowserHelper", mBrowserHelper);
                }

                return mBrowserHelper;
            }
        }


        /// <summary>
        /// Current device info.
        /// </summary>
        public virtual CurrentDevice CurrentDevice
        {
            get
            {
                return mCurrentDevice ?? (mCurrentDevice = Service.Resolve<ICurrentDeviceProvider>().GetCurrentDevice());
            }
        }


        /// <summary>
        /// Current device profile info.
        /// </summary>
        public virtual DeviceProfileInfo DeviceProfileInfo
        {
            get
            {
                return mDeviceProfileInfo ?? (mDeviceProfileInfo = DeviceProfileInfoProvider.GetCurrentDeviceProfileInfo(SiteName));
            }
        }


        /// <summary>
        /// Current device profile name.
        /// </summary>
        public virtual String DeviceProfileInfoName
        {
            get
            {
                return mDeviceProfileInfoName ?? (mDeviceProfileInfoName = DeviceProfileInfo != null ? DeviceProfileInfo.ProfileName : String.Empty);
            }
        }


        /// <summary>
        /// CSS styles
        /// </summary>
        public virtual CSSContainer CSS
        {
            get
            {
                return mCSS ?? (mCSS = new CSSContainer());
            }
        }


        /// <summary>
        /// Gets the collection of all sites.
        /// </summary>
        public virtual IInfoObjectCollection<SiteInfo> AllSites
        {
            get
            {
                if (mAllSites != null)
                {
                    return mAllSites;
                }

                var col = new InfoObjectCollection<SiteInfo>();

                col.ChangeParent(null, this);

                mAllSites = col;

                return mAllSites;
            }
        }


        /// <summary>
        /// Returns the list of all global objects (entry point to global objects).
        /// </summary>
        public virtual InfoObjectRepository GlobalObjects
        {
            get
            {
                if (!globalObjectsLoaded)
                {
                    LoadObjects();
                }
                return mGlobalObjects;
            }
        }


        /// <summary>
        /// Collection of transformations indexed by their full name
        /// </summary>
        public FullNameInfoObjectCollection<TransformationInfo> Transformations
        {
            get
            {
                if (mTransformations == null)
                {
                    var transformations = new FullNameInfoObjectCollection<TransformationInfo>(TransformationInfo.OBJECT_TYPE, t => t.TransformationFullName)
                    {
                        OrderByColumns = "TransformationClassID, TransformationName",
                        SortNames = false
                    };

                    transformations.ChangeParent(null, this);
                    mTransformations = transformations;
                }

                return mTransformations;
            }
        }


        /// <summary>
        /// Collection of queries indexed by their full name
        /// </summary>
        public FullNameInfoObjectCollection<QueryInfo> Queries
        {
            get
            {
                if (mQueries == null)
                {
                    var queries = new FullNameInfoObjectCollection<QueryInfo>(QueryInfo.OBJECT_TYPE, q => q.QueryFullName)
                    {
                        OrderByColumns = "ClassID, QueryName",
                        SortNames = false
                    };

                    queries.ChangeParent(null, this);
                    mQueries = queries;
                }

                return mQueries;
            }
        }


        /// <summary>
        /// Collection of alternative forms indexed by their full name
        /// </summary>
        public FullNameInfoObjectCollection<AlternativeFormInfo> AlternativeForms
        {
            get
            {
                if (mAlternativeForms == null)
                {
                    var forms = new FullNameInfoObjectCollection<AlternativeFormInfo>(AlternativeFormInfo.OBJECT_TYPE, f => f.FullName)
                    {
                        OrderByColumns = "FormClassID, FormName",
                        SortNames = false
                    };

                    forms.ChangeParent(null, this);
                    mAlternativeForms = forms;
                }

                return mAlternativeForms;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads <see cref="GlobalObjects"/> and <see cref="CMSDataContextBase{ParentType}.SiteObjects"/>.
        /// The method's execution is synchronized by <see cref="CMSDataContextBase{ParentType}.LoadObjectsLock"/>.
        /// </summary>
        /// <see cref="CMSDataContextBase{ParentType}.LoadObjectsLock"/>
        protected override void LoadObjects()
        {
            lock (LoadObjectsLock)
            {
                if (!globalObjectsLoaded)
                {
                    mGlobalObjects = new InfoObjectRepository(this);

                    base.LoadObjects();

                    globalObjectsLoaded = true;
                }
            }
        }


        /// <summary>
        /// Loads the given object type. The method is called during <see cref="LoadObjects"/> execution
        /// and is inherently synchronized by <see cref="CMSDataContextBase{ParentType}.LoadObjectsLock"/>.
        /// </summary>
        /// <param name="infoObj">Info object for the given object type</param>
        /// <param name="siteId">Site ID</param>
        /// <remarks>
        /// The implementation is responsible for loading global objects only (i.e. <paramref name="infoObj"/> must represent
        /// a global object) and passing on the call to <see cref="CMSDataContextBase{ParentType}.LoadObjectType(GeneralizedInfo, int)"/>.
        /// </remarks>
        protected override void LoadObjectType(GeneralizedInfo infoObj, int siteId)
        {
            base.LoadObjectType(infoObj, siteId);

            var ti = infoObj.TypeInfo;
            if (!ti.IsSiteObject || ti.SupportsGlobalObjects)
            {
                // Global object
                mGlobalObjects.AddCollection(
                    new InfoCollectionSettings(ti.MacroCollectionName, ti.ObjectType) { SiteID = 0 }
                );
            }
        }


        /// <summary>
        /// Registers the object properties.
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("CurrentBrowser", m => m.BrowserHelper);
            RegisterProperty("CurrentDevice", m => m.CurrentDevice);
            RegisterProperty("CSS", m => m.CSS);

            RegisterProperty<UserInfo>("CurrentUser", m => MembershipContext.AuthenticatedUser);
            RegisterProperty<SiteInfo>("CurrentSite", m => SiteContext.CurrentSite);
            RegisterProperty<TreeNode>("CurrentDocument", m => DocumentContext.CurrentDocument);
            RegisterProperty<PageInfo>("CurrentPageInfo", m => DocumentContext.CurrentPageInfo);

            RegisterProperty<DeviceProfileInfo>("CurrentDeviceProfile", m => m.DeviceProfileInfo);
            RegisterProperty("CurrentDeviceProfileName", m => m.DeviceProfileInfoName);

            RegisterProperty("AllSites", m => m.AllSites);
            RegisterProperty("GlobalObjects", m => m.GlobalObjects);

            RegisterProperty("Transformations", m => m.Transformations);
            RegisterProperty("Queries", m => m.Queries);
            RegisterProperty("AlternativeForms", m => m.AlternativeForms);

            // Get the static properties
            var props = typeof(IContext).GetStaticProperties<IContext>();
            if (props != null)
            {
                foreach (var prop in props.TypedValues)
                {
                    var p = prop;

                    RegisterProperty(p.Name, m => p.Value);
                }
            }
        }

        #endregion
    }
}