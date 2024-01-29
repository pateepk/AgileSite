using System;

using CMS.DataEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Repository of the documents without workflow support.
    /// </summary>
    public class TreeNodeRepository : InfoObjectRepository<TreeNodeCollection, TreeNode, TreeNodeCollectionSettings>, ICMSStorage
    {
        #region "Variables"

        /// <summary>
        /// Tree provider object to use for the database access.
        /// </summary>
        private TreeProvider mTreeProvider;


        /// <summary>
        /// Site name.
        /// </summary>
        private string mSiteName;

        /// <summary>
        /// Root document.
        /// </summary>
        private TreeNode mRootDocument;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public virtual TreeProvider TreeProvider
        {
            get
            {
                if (mTreeProvider == null)
                {
                    mTreeProvider = new TreeProvider();
                }
                return mTreeProvider;
            }
            set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// If true, the document is the last version (retrieved using DocumentHelper.GetDocument).
        /// </summary>
        public virtual bool IsLastVersion
        {
            get;
            set;
        }


        /// <summary>
        /// Site name.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return mSiteName ?? (mSiteName = SiteContext.CurrentSiteName);
            }
        }


        /// <summary>
        /// Root document.
        /// </summary>
        public virtual TreeNode RootDocument
        {
            get
            {
                if (mRootDocument != null)
                {
                    return mRootDocument;
                }

                if (SiteName == null)
                {
                    return null;
                }

                // Get the root document
                mRootDocument = GetRootDocument();

                return mRootDocument;
            }
        }


        /// <summary>
        /// Parent document.
        /// </summary>
        public TreeNode ParentDocument
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns true if the repository is disconnected from the data source
        /// </summary>
        public override bool IsDisconnected
        {
            get
            {
                if ((ParentDocument != null) && (ParentDocument.Generalized.IsDisconnected))
                {
                    return true;
                }

                return base.IsDisconnected;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public TreeNodeRepository()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentDocument">Parent document</param>
        public TreeNodeRepository(TreeNode parentDocument)
        {
            ParentDocument = parentDocument;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public TreeNodeRepository(string siteName)
        {
            mSiteName = siteName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the root document from the database.
        /// </summary>
        protected virtual TreeNode GetRootDocument()
        {
            return TreeProvider.SelectSingleNode(SiteName, "/", null, true, SystemDocumentTypes.Root, false);
        }


        /// <summary>
        /// Registers the given collection of objects within the repository.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        public override TreeNodeCollectionSettings AddCollection(TreeNodeCollectionSettings settings)
        {
            var name = settings.Name;
            string niceName = GetNicePropertyName(name);

            if (name == null)
            {
                name = TranslationHelper.GetSafeClassName(settings.ClassName);
            }
            else
            {
                name = TranslationHelper.GetSafeClassName(name);
            }

            string lowerName = name.ToLowerCSafe();

            lock (CollectionSettingsLock)
            {
                // Check if already registered
                if (CollectionSettings.Contains(lowerName))
                {
                    throw new ArgumentException("The collection with name '" + name + "' is already registered.");
                }

                // Add new settings to the table
                settings.Index = Count;
                settings.LowerName = lowerName;
                settings.Name = name;
                settings.NiceName = niceName;

                CollectionSettings[lowerName] = settings;
                CollectionSettingsByIndex[Count] = settings;

                Count++;
            }

            return settings;
        }


        /// <summary>
        /// Loads the given collection.
        /// </summary>
        /// <param name="settings">Collection settings</param>
        protected override TreeNodeCollection LoadCollection(TreeNodeCollectionSettings settings)
        {
            TreeNodeCollection result = null;
            if (settings != null)
            {
                TreeNodeCollectionSettings s = settings;

                // Create a new collection
                result = NewCollection(s.ClassName);

                // Prepare the parameters for the data
                result.SiteName = (s.SiteID > 0) ? SiteInfoProvider.GetSiteName(s.SiteID) : TreeProvider.ALL_SITES;
                result.AliasPath = s.AliasPath;
                result.CultureCode = s.CultureCode;
                result.CombineWithDefaultCulture = s.CombineWithDefaultCulture;
                result.ClassNames = s.ClassNames;
                result.Where = s.WhereCondition;
                result.OrderByColumns = s.OrderBy;
                result.MaxRelativeLevel = s.MaxRelativeLevel;
                result.SelectOnlyPublished = s.SelectOnlyPublished;
                result.TopN = s.TopN;
                result.Columns = s.Columns;
                result.SelectAllData = s.SelectAllData;
                if (s.NameColumn != null)
                {
                    result.NameColumn = s.NameColumn;
                }

                // Save to the indexed tables
                Collections[s.LowerName] = result;

                // Ensure the index and assign
                while (CollectionsList.Count <= s.Index)
                {
                    CollectionsList.Add(null);
                }

                CollectionsList[s.Index] = result;
            }

            return result;
        }


        /// <summary>
        /// Creates new collection for the data.
        /// </summary>
        /// <param name="type">Type of the collection</param>
        public override TreeNodeCollection NewCollection(string type)
        {
            // Create a new collection
            var col = new TreeNodeCollection(type, ParentDocument, this)
                          {
                              IsLastVersion = IsLastVersion
                          };

            return col;
        }


        /// <summary>
        /// Creates new combined collection for the data.
        /// </summary>
        public override CombinedInfoObjectCollection<TreeNodeCollection, TreeNode> NewCombinedCollection()
        {
            return new CombinedTreeNodeCollection();
        }

        #endregion
    }
}