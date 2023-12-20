using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Data context base
    /// </summary>
    public abstract class CMSDataContextBase<ParentType> : AbstractHierarchicalObject<ParentType>, ICMSStorage
        where ParentType : CMSDataContextBase<ParentType>
    {
        #region "Variables"

        private InfoObjectRepository mSiteObjects;
        private bool siteObjectsLoaded;

        private SettingsMacroContainer mSettingsMacroContainer;
        private TreeNode mRootDocument;
        private TreeNodeCollection mDocuments;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the object is cached within the system for later use
        /// </summary>
        public virtual bool IsCachedObject
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if this object is disconnected from the database
        /// </summary>
        public virtual bool IsDisconnected
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Context site name.
        /// </summary>
        public virtual string SiteName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the root document of current site.
        /// </summary>
        public virtual TreeNode RootDocument
        {
            get
            {
                if (String.IsNullOrEmpty(SiteName))
                {
                    return null;
                }

                return mRootDocument ?? (mRootDocument = GetDocument());
            }
        }


        /// <summary>
        /// Gets the collection of all documents on the site
        /// </summary>
        public virtual TreeNodeCollection Documents
        {
            get
            {
                if (String.IsNullOrEmpty(SiteName))
                {
                    return null;
                }

                if (mDocuments == null)
                {
                    // Collection of all documents
                    var col = new TreeNodeCollection(null, null, this);

                    col.SiteName = SiteName;
                    col.NameColumn = "NodeAliasPath";
                    col.OrderByColumns = "NodeLevel, NodeAlias";

                    col.IsLastVersion = (PortalContext.ViewMode != ViewModeEnum.LiveSite);
                    col.SelectOnlyPublished = !col.IsLastVersion;

                    mDocuments = col;
                }

                return mDocuments;
            }
        }


        /// <summary>
        /// Macro container for all settings.
        /// </summary>
        internal SettingsMacroContainer SettingsMacroContainer
        {
            get
            {
                return mSettingsMacroContainer ?? (mSettingsMacroContainer = new SettingsMacroContainer(SiteInfoProvider.GetSiteID(SiteName)));
            }
        }


        /// <summary>
        /// Returns the list of all site objects (entry point to site objects).
        /// </summary>
        public virtual InfoObjectRepository SiteObjects
        {
            get
            {
                if (!siteObjectsLoaded)
                {
                    LoadObjects();
                }
                return mSiteObjects;
            }
        }


        /// <summary>
        /// Gets lock object to synchronize <see cref="LoadObjects"/> execution.
        /// </summary>
        protected object LoadObjectsLock
        {
            get;
        } = new object();

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected CMSDataContextBase()
        {
            SiteName = SiteContext.CurrentSiteName;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected CMSDataContextBase(string siteName)
        {
            SiteName = siteName;
        }


        /// <summary>
        /// Loads <see cref="SiteObjects"/> and possibly other objects in inherited classes.
        /// The method's execution is synchronized by <see cref="LoadObjectsLock"/>.
        /// </summary>
        /// <see cref="LoadObjectsLock"/>
        protected virtual void LoadObjects()
        {
            lock (LoadObjectsLock)
            {
                if (!siteObjectsLoaded)
                {
                    mSiteObjects = new InfoObjectRepository(this);

                    int siteId = SiteInfoProvider.GetSiteID(SiteName);

                    // Process all available object types
                    foreach (string objectType in ObjectTypeManager.MainObjectTypes)
                    {
                        LoadObjectType(objectType, siteId);
                    }

                    siteObjectsLoaded = true;
                }
            }
        }


        /// <summary>
        /// Loads the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Site ID</param>
        private void LoadObjectType(string objectType, int siteId)
        {
            // Get the info object for the object
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
            if ((infoObj != null) && !infoObj.TypeInfo.IsBinding)
            {
                LoadObjectType(infoObj, siteId);
            }
        }


        /// <summary>
        /// Loads the given object type. The method is called during <see cref="LoadObjects"/> execution
        /// and is inherently synchronized by <see cref="LoadObjectsLock"/>.
        /// </summary>
        /// <param name="infoObj">Info object for the given object type</param>
        /// <param name="siteId">Site ID</param>
        /// <remarks>
        /// The implementation is responsible for loading site objects only (i.e. <paramref name="infoObj"/> must represent
        /// a site specific object).
        /// </remarks>
        protected virtual void LoadObjectType(GeneralizedInfo infoObj, int siteId)
        {
            var ti = infoObj.TypeInfo;
            if (ti.IsSiteObject && (siteId > 0))
            {
                // Site object
                mSiteObjects.AddCollection(
                    new InfoCollectionSettings(ti.MacroCollectionName, ti.ObjectType) { SiteID = siteId }
                );
            }
        }


        /// <summary>
        /// Registers the object properties.
        /// </summary>
        protected override void RegisterProperties()
        {
            base.RegisterProperties();

            // Site specific objects
            RegisterProperty("RootDocument", m => m.RootDocument);
            RegisterProperty("Documents", m => m.Documents);
            RegisterProperty("Settings", m => m.SettingsMacroContainer);
            RegisterProperty("SiteObjects", m => m.SiteObjects);
        }

        private TreeNode GetDocument()
        {
            var tree = new TreeProvider();
            var node = tree.SelectSingleNode(SiteName, "/", TreeProvider.ALL_CULTURES, true, SystemDocumentTypes.Root, false);

            if ((node != null) && (PortalContext.ViewMode != ViewModeEnum.LiveSite))
            {
                node = DocumentHelper.GetDocument(node, tree);
            }

            return node;
        }

        #endregion
    }
}