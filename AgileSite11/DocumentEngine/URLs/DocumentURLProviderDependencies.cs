using System;
using System.Linq;
using System.Text;

using CMS.PortalEngine;

namespace CMS.DocumentEngine
{
    internal class DocumentURLProviderDependencies
    {
        /// <summary>
        /// Gets the current URL language prefix
        /// </summary>
        public string CurrentURLLangPrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current URL path prefix.
        /// </summary>
        public string CurrentURLPathPrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current domain
        /// </summary>
        public string CurrentDomain
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current view mode
        /// </summary>
        public ViewModeEnum ViewMode
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current site name
        /// </summary>
        public string CurrentSiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public DocumentURLProviderDependencies(string currentURLLangPrefix, string currentURLPathPrefix, string currentDomain, ViewModeEnum viewMode, string currentSiteName)
        {
            CurrentURLLangPrefix = currentURLLangPrefix;
            CurrentURLPathPrefix = currentURLPathPrefix;
            CurrentDomain = currentDomain;
            ViewMode = viewMode;
            CurrentSiteName = currentSiteName;
        }
    }
}
