using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Structure to define the selection parameters for a document nodes selection
    /// </summary>
    public class NodeSelectionParameters
    {
        #region "Variables"

        /// <summary>
        /// Combine with default culture
        /// </summary>
        protected bool? mCombineWithDefaultCulture;

        private bool mCheckLicense = true;
        private bool mSelectOnlyPublished = true;
        private bool mSelectAllData = true;
        private int mMaxRelativeLevel = -1;
        private string mSiteName = TreeProvider.ALL_SITES;
        private Guid? mRelationshipNodeGUID;
        private RelationshipSideEnum? mRelationshipSide;

        #endregion


        #region "Properties"

        /// <summary>
        /// Nodes site name
        /// </summary>
        public string SiteName
        {
            get
            {
                return mSiteName;
            }
            set
            {
                mSiteName = value;
            }
        }


        /// <summary>
        /// Node alias path.
        /// </summary>
        public string AliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Nodes culture code
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies if return the default culture document when specified culture not found
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                if (mCombineWithDefaultCulture == null)
                {
                    // Get the value from the site settings
                    string siteName = SiteName;
                    if (TreeProvider.AllSites(siteName))
                    {
                        siteName = null;
                    }

                    mCombineWithDefaultCulture = SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCombineWithDefaultCulture");
                }

                return mCombineWithDefaultCulture.Value;
            }
            set
            {
                mCombineWithDefaultCulture = value;
            }
        }


        /// <summary>
        /// List of classNames to select separated by semicolon (e.g.: "cms.article;cms.product")
        /// </summary>
        public string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition to use for the data selection
        /// </summary>
        public string Where
        {
            get;
            set;
        }


        /// <summary>
        /// Order by clause to use for the data selection
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Maximal child level of the selected nodes
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel;
            }
            set
            {
                mMaxRelativeLevel = value;
            }
        }


        /// <summary>
        /// Select only published nodes
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return mSelectOnlyPublished;
            }
            set
            {
                mSelectOnlyPublished = value;
            }
        }


        /// <summary>
        /// Select top N rows only
        /// </summary>
        public int TopN
        {
            get;
            set;
        }


        /// <summary>
        /// Columns to be selected. Columns definition must contain mandatory columns (NodeID, NodeLinkedNodeID, DocumentCulture)
        /// </summary>
        public string Columns
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the coupled data are retrieved in case class names are specified
        /// </summary>
        public bool SelectAllData
        {
            get
            {
                return mSelectAllData;
            }
            set
            {
                mSelectAllData = value;
            }
        }


        /// <summary>
        /// If true, single node selection takes place (escapes query like patterns)
        /// </summary>
        public bool SelectSingleNode
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the selection checks license conditions
        /// </summary>
        public bool CheckLicense
        {
            get
            {
                return mCheckLicense;
            }
            internal set
            {
                mCheckLicense = value;
            }
        }


        /// <summary>
        /// Defines node GUID of the related document. Only document in relation with this document will be included in the results.
        /// </summary>
        internal Guid RelationshipNodeGUID
        {
            get
            {
                return mRelationshipNodeGUID.HasValue ? mRelationshipNodeGUID.Value : Guid.Empty;
            }
            set
            {
                mRelationshipNodeGUID = value;
            }
        }


        /// <summary>
        /// Defines name of the relationship. If not provided documents from all relationships will be retrieved.
        /// </summary>
        internal string RelationshipName
        {
            get;
            set;
        }


        /// <summary>
        /// Defines side of the related document within the relation. Both sides are used by default.
        /// </summary>
        internal RelationshipSideEnum RelationshipSide
        {
            get
            {
                return mRelationshipSide.HasValue ? mRelationshipSide.Value : RelationshipSideEnum.Both;
            }
            set
            {
                mRelationshipSide = value;
            }
        }


        /// <summary>
        /// Indicates if latest edited version of the documents should be selected.
        /// </summary>
        internal bool SelectLatestVersion
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public NodeSelectionParameters()
        {
            CultureCode = CultureHelper.GetPreferredCulture();
        }


        /// <summary>
        /// Gets list of resolved class names in context of parameters.
        /// </summary>
        internal List<string> GetResolvedClassNames()
        {
            var classNames = ClassNames;

            // For single document, narrow class name to a specific document to avoid too many queries
            if (SelectAllData && TreeProvider.AllClasses(classNames) && !String.IsNullOrEmpty(AliasPath) && SelectSingleNode && !TreeProvider.AllSites(SiteName))
            {
                classNames = GetClassNameByAliasPath(SiteName, SqlHelper.EscapeLikeText(AliasPath));
            }
            else
            {
                // Get all class names if current class name value is equal to all classes
                classNames = DocumentTypeHelper.GetClassNames(classNames, SiteName);
            }

            return classNames != null ? classNames.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
        }


        private static string GetClassNameByAliasPath(string siteName, string aliasPath)
        {
            // Get the data
            var data = TreePathUtils.GetNodeByAliasPath(siteName, aliasPath, "NodeClassID");
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            var row = data.Tables[0].Rows[0];
            int classId = (int)row["NodeClassID"];

            return DataClassInfoProvider.GetClassName(classId);
        }


        /// <summary>
        /// Gets node alias path for selection. If single document should be retrieved, path is escaped.
        /// </summary>
        internal string GetNodeAliasPathForSelection()
        {
            // Ensure correct path for single node selection
            return SelectSingleNode ? SqlHelper.EscapeLikeText(AliasPath) : AliasPath;
        }
    }
}