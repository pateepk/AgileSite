using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Culture specific URLs data source control.
    /// </summary>
    [ToolboxData("<{0}:LanguageDataSource runat=server></{0}:LanguageDataSource>"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class LanguageDataSource : CMSBaseDataSource
    {
        private bool mUseCultureSpecificURLs = true;
        private bool mExcludeUntranslatedDocuments = true;

        #region "Properties"

        /// <summary>
        /// URLs for not translated pages will not be generated. Default value is true.
        /// </summary>
        public bool ExcludeUntranslatedDocuments
        {
            get
            {
                return mExcludeUntranslatedDocuments;
            }
            set
            {
                mExcludeUntranslatedDocuments = value;
            }
        }


        /// <summary>
        /// Culture for which URLs should not be generated.
        /// </summary>
        public string ExcludedCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Returns URL for the document aliasPath and document URL path (preferable if document URL path is not wildcard URL). Default value is true.
        /// </summary>
        public bool UseCultureSpecificURLs
        {
            get
            {
                return mUseCultureSpecificURLs;
            }
            set
            {
                mUseCultureSpecificURLs = value;
            }
        }


        /// <summary>
        /// Node for which document culture URLs will be generated.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Defines format of culture specific URLs. Use logical OR operation to define parameters.
        /// </summary>
        public UrlOptionsEnum UrlOptions
        {
            get;
            set;
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <returns>List of DocumentCultureURLs as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (StopProcessing)
            {
                return null;
            }

            object documentCultureURLs = null;

            if (Node != null)
            {
                string url = URLHelper.RemoveQuery(RequestContext.CurrentURL);
                documentCultureURLs = DocumentURLProvider.GetDocumentCultureUrls(Node, url, ExcludedCultureCode, UrlOptions);
            }

            return documentCultureURLs;
        }


        /// <summary>
        /// Gets the default cache key.
        /// </summary>
        /// <returns>Default cache key</returns>
        protected override object[] GetDefaultCacheKey()
        {
            if (Node == null)
            {
                return base.GetDefaultCacheKey();
            }

            return new object[] { "languageselection", Node.NodeSiteName, Node.NodeAliasPath, RequestContext.IsSSL, ExcludeUntranslatedDocuments, UseCultureSpecificURLs, ExcludedCultureCode };
        }


        /// <summary>
        /// Gets the default cache dependencies.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            var result = new StringBuilder();

            if (Node != null)
            {
                result.AppendLine("node|" + Node.NodeSiteName.ToLowerCSafe() + "|" + Node.NodeAliasPath.ToLowerCSafe());

                var siteID = Node.NodeSiteID;

                result.AppendLine("cms.settingskey|" + siteID + "|cmsprocessdomainprefix");
                result.AppendLine("cms.settingskey|" + siteID + "|cmsdefaulpage");
                result.AppendLine("cms.settingskey|" + siteID + "|cmsuselangprefixforurls");
                result.AppendLine("cms.settingskey|" + siteID + "|cmsusedomainforculture");
            }
            
            result.AppendLine("cms.culturesite|all");
            result.AppendLine("cms.culture|all");

            result.AppendLine("cms.settingskey|cmsprocessdomainprefix");
            result.AppendLine("cms.settingskey|cmsdefaulpage");
            result.AppendLine("cms.settingskey|cmsuselangprefixforurls");
            result.Append("cms.settingskey|cmsusedomainforculture");
            
            return result.ToString();
        }

        #endregion
    }
}
