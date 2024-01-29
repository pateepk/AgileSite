using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Settings for search index permission check for documents
    /// </summary>
    public class DocumentFilterSearchResultsParameters
    {
        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">User to check against</param>
        /// <param name="checkPermissions">Check permissions flag</param>
        /// <param name="culture">Culture</param>
        /// <param name="combineWithDefaultCulture">Combine with default culture</param>
        public DocumentFilterSearchResultsParameters(IUserInfo user, bool checkPermissions, string culture, bool combineWithDefaultCulture)
        {
            Culture = culture;

            if (checkPermissions && !user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                GlobalPermissions = new SafeDictionary<string, bool?>();
                ClassIDPermissions = new SafeDictionary<string, SafeDictionary<string, bool>>();
                ACLIDPermissions = new SafeDictionary<string, SafeDictionary<int, object>>();
            }

            // Generate cultures table if it is required and current culture is required
            if (combineWithDefaultCulture && (culture != "##ALL##"))
            {
                Cultures = new SafeDictionary<int, int>();
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary of global permission flags [Sitename -> true/false]
        /// </summary>
        public SafeDictionary<string, bool?> GlobalPermissions
        {
            get;
            protected set;
        }


        /// <summary>
        /// Dictionary of site dictionaries of class permission results [SiteName -> [ClassName -> true/false]]
        /// </summary>
        public SafeDictionary<string, SafeDictionary<string, bool>> ClassIDPermissions
        {
            get;
            protected set;
        }


        /// <summary>
        /// Dictionary of site dictionaries of class permission results [SiteName -> [ACLID -> AuthorizationResultEnum]]
        /// </summary>
        public SafeDictionary<string, SafeDictionary<int, object>> ACLIDPermissions
        {
            get;
            protected set;
        }


        /// <summary>
        /// Dictionary of already found culture versions of the same document [NodeID -> index]
        /// </summary>
        public SafeDictionary<int, int> Cultures
        {
            get;
            protected set;
        }


        /// <summary>
        /// Current culture
        /// </summary>
        public string Culture
        {
            get;
            set;
        }


        /// <summary>
        /// Default culture
        /// </summary>
        public string DefaultCulture
        {
            get;
            set;
        }

        #endregion
    }
}