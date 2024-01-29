using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Settings for search index permission check
    /// </summary>
    public class SearchResults
    {
        #region "Properties"

        /// <summary>
        /// List of the results
        /// </summary>
        public List<ILuceneSearchDocument> Results
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns results belonging to this roles
        /// </summary>
        public List<string> InRoles
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns results not belonging to these roles
        /// </summary>
        public List<string> NotInRoles
        {
            get;
            protected set;
        }


        /// <summary>
        /// Site IDs for search
        /// </summary>
        public List<int> SiteIDs
        {
            get;
            protected set;
        }


        /// <summary>
        /// Current user info
        /// </summary>
        public IUserInfo User
        {
            get;
            protected set;
        }


        /// <summary>
        /// Collection of documents ids which are present in filtered results
        /// </summary>
        public HashSet<string> Documents
        {
            get;
            protected set;
        }


        /// <summary>
        /// Parameters for document search filter results
        /// </summary>
        public DocumentFilterSearchResultsParameters DocumentParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Indexes to search
        /// </summary>
        public List<SearchIndexInfo> Indexes
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">User to check against</param>
        /// <param name="inRoles">Returns results belonging to this roles</param>
        /// <param name="notInRoles">Returns results not belonging to these roles</param>
        /// <param name="siteIds">Site IDs for search</param>
        /// <param name="indexes">Indexes to search</param>
        public SearchResults(IUserInfo user, List<string> inRoles, List<string> notInRoles, List<int> siteIds, List<SearchIndexInfo> indexes)
        {
            User = user;

            // Initialize collections
            if (inRoles == null)
            {
                inRoles = new List<string>();
            }
            InRoles = inRoles;

            if (notInRoles == null)
            {
                notInRoles = new List<string>();
            }
            NotInRoles = notInRoles;

            Documents = new HashSet<string>();
            Results = new List<ILuceneSearchDocument>();
            SiteIDs = siteIds;
            Indexes = indexes;
        }

        #endregion
    }
}
