using System;

using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class containing settings to be used for site root creation.
    /// </summary>
    internal class SiteRootCreationSettings
    {
        private static readonly Guid DEFAULT_NODE_GUID = new Guid("ACDD2058-BDE0-4C9D-8332-45F417220571");
        private static readonly Guid DEFAULT_DOCUMENT_GUID = new Guid("AA0FC9AF-C321-41B2-872A-A92A9153E6C4");
        private static readonly Guid DEFAULT_ACL_GUID = new Guid("0072D163-58B7-495F-BBCD-1F15EAADF7E3");


        /// <summary>
        /// Site name of the site for which the root document is created
        /// </summary>
        public string SiteName
        {
            get;
            private set;
        }


        /// <summary>
        /// Code of a culture in which the root document is created
        /// </summary>
        public string Culture
        {
            get;
            private set;
        }


        /// <summary>
        /// Node GUID to be used for site root document
        /// </summary>
        public Guid NodeGUID
        {
            get;
            private set;
        }


        /// <summary>
        /// Document GUID to be used for site root document
        /// </summary>
        public Guid DocumentGUID
        {
            get;
            private set;
        }


        /// <summary>
        /// ACL GUID to be used for ACL of the site root document
        /// </summary>
        public Guid ACLGUID
        {
            get
            {
                return DEFAULT_ACL_GUID;
            }
        }


        /// <summary>
        /// Creates instance of <see cref="SiteRootCreationSettings"/> class.
        /// </summary>
        /// <param name="siteName">Site name of the site to create the root document for</param>
        public SiteRootCreationSettings(string siteName) :
            this(siteName, DEFAULT_NODE_GUID, DEFAULT_DOCUMENT_GUID)
        {
        }


        /// <summary>
        /// Creates instance of <see cref="SiteRootCreationSettings"/> class.
        /// </summary>
        /// <param name="siteName">Site name of the site to create the root document for</param>
        /// <param name="nodeGuid">Existing root node GUID</param>
        /// <param name="documentGuid">Existing root document GUID</param>
        /// <param name="culture">Culture code in which the root document is created; site's default culture is used if not specified</param>
        public SiteRootCreationSettings(string siteName, Guid nodeGuid, Guid documentGuid, string culture = null)
        {
            if (nodeGuid == Guid.Empty)
            {
                throw new ArgumentException("Node GUID needs to be provided.", nameof(nodeGuid));
            }

            if (documentGuid == Guid.Empty)
            {
                throw new ArgumentException("Document GUID needs to be provided.", nameof(documentGuid));
            }

            SiteName = siteName;
            NodeGUID = nodeGuid;
            DocumentGUID = documentGuid;
            Culture = string.IsNullOrEmpty(culture) ? CultureHelper.GetDefaultCultureCode(siteName) : culture;
        }
    }
}