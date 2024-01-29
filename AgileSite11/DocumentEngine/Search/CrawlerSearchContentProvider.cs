using System;
using System.Net;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;

namespace CMS.DocumentEngine.Search
{
    /// <summary>
    /// Encapsulates functionality that is responsible for downloading a content of the particular 
    /// page for the purpose of searching.
    /// </summary>
    internal class CrawlerSearchContentProvider
    {
        private readonly TreeNode mTreeNode;
        private readonly ISearchCrawler mCrawler;


        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlerSearchContentProvider"/> class.
        /// </summary>
        /// <param name="treeNode">Tree node.</param>
        /// <param name="crawler">Crawler used for downloading of the page content.</param>
        /// <exception cref="ArgumentNullException">Is thrown when parameters <paramref name="treeNode"/> or <paramref name="crawler"/> is <c>null</c>.</exception>
        public CrawlerSearchContentProvider(TreeNode treeNode, ISearchCrawler crawler)
        {
            if (treeNode == null)
            {
                throw new ArgumentNullException("treeNode");
            }

            if (crawler == null)
            {
                throw new ArgumentNullException("crawler");
            }

            mTreeNode = treeNode;
            mCrawler = crawler;
        }


        /// <summary>
        /// Download and process HTML content and returns parsed content.
        /// </summary>
        /// <param name="index">Search index info.</param>     
        public string GetContent(ISearchIndexInfo index)
        {
            // Crawler search
            if (mTreeNode.DocumentSearchExcluded || string.Equals(mTreeNode.NodeClassName, SystemDocumentTypes.File, StringComparison.InvariantCultureIgnoreCase) || !mTreeNode.IsPublished)
            {
                return string.Empty;
            }

            // Try get domain name specified in app keys
            string domainName = ValidationHelper.GetString(index.GetValue("IndexCrawlerDomain"), String.Empty);

            // If is not defined use value from web.config
            if (String.IsNullOrEmpty(domainName))
            {
                domainName = SearchCrawler.CrawlerDomainName;
            }

            // If non of the above defined, use default domain name for the site
            if (String.IsNullOrEmpty(domainName))
            {
                domainName = mTreeNode.Site.DomainName;
            }

            // Get page URL
            string url = DocumentURLProvider.GetPresentationUrl(mTreeNode, domainName);

            // Check whether URL is defined
            if (!String.IsNullOrEmpty(url))
            {
                if (!mTreeNode.Site.SiteIsContentOnly)
                {
                    // Set correct language
                    url = URLHelper.UpdateParameterInUrl(url, URLHelper.LanguageParameterName, mTreeNode.DocumentCulture);
                }

                // Set crawler user password
                var indexUserPassword = ValidationHelper.GetString(index.GetValue("IndexCrawlerUserPassword"), String.Empty);
                if (!string.IsNullOrEmpty(indexUserPassword))
                {
                    mCrawler.CrawlerPassword = EncryptionHelper.DecryptData(indexUserPassword);
                }

                // Set crawler domain user name
                var indexUserName = ValidationHelper.GetString(index.GetValue("IndexCrawlerUserName"), String.Empty);
                if (!string.IsNullOrEmpty(indexUserName))
                {
                    mCrawler.CrawlerUserName = indexUserName;
                }

                // Make the crawler request use admin account unless the index says otherwise to make the data available
                var userName = ValidationHelper.GetString(index.GetValue("IndexCrawlerFormsUserName"), String.Empty);
                if (string.IsNullOrEmpty(userName))
                {
                    userName = UserInfoProvider.AdministratorUserName;
                }

                mCrawler.CrawlerFormsUserName = userName;

                // Try download html code
                string html = String.Empty;
                try
                {
                    html = mCrawler.DownloadHtmlContent(url);
                }
                catch (WebException)
                {
                    // Ignore WebException on purpose, request errors should be logged by the request itself not the crawler
                }
                catch (Exception ex)
                {
                    throw new SearchIndexException(index, "Unable to download document: " + url, ex);
                }

                // Check whether html version exists
                if (!String.IsNullOrEmpty(html))
                {
                    // Try get plain text version
                    string plain = SearchCrawler.HtmlToPlainText(html);
                    // Check whether plain text version exists
                    if (!String.IsNullOrEmpty(plain))
                    {
                        return plain;
                    }
                }
            }

            return String.Empty;
        }
    }
}
