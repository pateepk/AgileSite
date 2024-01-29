using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Localization;
using CMS.DocumentEngine;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Hierarchy page info structure ensures the inheritance for current node.
    /// </summary>
    public class HierarchyPageInfo
    {
        #region "Properties"

        /// <summary>
        /// Visual root page info
        /// </summary>
        public PageInfo VisualRootPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Current page info
        /// </summary>
        public PageInfo CurrentPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Root page info
        /// </summary>
        private PageInfo RootPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Used view mode
        /// </summary>
        private ViewModeEnum ViewMode
        {
            get;
            set;
        }


        /// <summary>
        /// List of logical pages
        /// </summary>
        private List<PageInfo> Pages
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentPageInfo">Current page info</param>
        /// <param name="viewMode">Required view mode</param>
        public HierarchyPageInfo(PageInfo currentPageInfo, ViewModeEnum viewMode)
        {
            if (currentPageInfo != null)
            {
                // Set properties
                CurrentPageInfo = currentPageInfo.CreateInherited();
                ViewMode = viewMode;

                ProcessHierarchy();
            }
        }


        /// <summary>
        /// Process page info hierarchy
        /// </summary>
        private void ProcessHierarchy()
        {
            var pages = new List<PageInfo> { CurrentPageInfo };
            var visualPages = new List<PageInfo> { CurrentPageInfo };

            string path = CurrentPageInfo.NodeAliasPath;

            if (!String.IsNullOrEmpty(path))
            {
                bool masterFound = false;
                List<int> specific = new List<int>();
                InheritanceType inheritType = GetInheritanceType(CurrentPageInfo, ref specific);
                PageTemplateInfo currentTemplate = CurrentPageInfo.UsedPageTemplateInfo;
                bool currentTemplateIsAspx = (currentTemplate != null) && currentTemplate.IsAspx;

                while (path != "/")
                {
                    // Get parent info
                    path = TreePathUtils.GetParentPath(path);
                    PageInfo parentInfo = PageInfoProvider.GetPageInfo(CurrentPageInfo.SiteName, path, LocalizationContext.PreferredCultureCode, null, pages[0].NodeParentID, SiteInfoProvider.CombineWithDefaultCulture(CurrentPageInfo.SiteName));
                    if ((parentInfo == null) || (parentInfo.DocumentID == 0))
                    {
                        break;
                    }

                    // Create inherited info
                    parentInfo = parentInfo.CreateInherited();

                    // Load version for non-livesite view mode
                    if (ViewMode != ViewModeEnum.LiveSite)
                    {
                        parentInfo.LoadVersion();
                    }

                    // Set parent info for ASPX mode
                    pages[0].ParentPageInfo = parentInfo;

                    // Add to the logical tree
                    pages.Insert(0, parentInfo);

                    // Get the parent page template
                    PageTemplateInfo template = parentInfo.UsedPageTemplateInfo;
                    bool parentIsAspx = (template != null) && template.IsAspx;

                    // Process only Portal engine templates in the visual inheritance chain
                    if (!currentTemplateIsAspx && !parentIsAspx)
                    {
                        // Add to the visual tree with dependence on settings
                        switch (inheritType)
                        {
                            case InheritanceType.All:
                                if (!masterFound)
                                {
                                    parentInfo.ChildPageInfo = visualPages[0];
                                    visualPages.Insert(0, parentInfo);
                                    masterFound = IsMasterPage(parentInfo);
                                }
                                break;

                            case InheritanceType.Master:
                                if (!masterFound && IsMasterPage(parentInfo))
                                {
                                    parentInfo.ChildPageInfo = visualPages[0];
                                    visualPages.Insert(0, parentInfo);
                                    masterFound = true;
                                }
                                break;

                            case InheritanceType.Specific:
                                if (specific.Contains(parentInfo.NodeLevel))
                                {
                                    parentInfo.ChildPageInfo = visualPages[0];
                                    visualPages.Insert(0, parentInfo);
                                }
                                break;
                        }
                    }
                }
            }

            // Keep the collections
            Pages = pages;

            // Keep the root items
            RootPageInfo = pages[0];
            VisualRootPageInfo = visualPages[0];
        }


        /// <summary>
        /// Returns true if page info should be considered as master page
        /// </summary>
        /// <param name="pi">Page info</param>
        private bool IsMasterPage(PageInfo pi)
        {
            if (pi.NodeAliasPath == "/")
            {
                return true;
            }

            PageTemplateInfo template = pi.UsedPageTemplateInfo;
            if (template != null)
            {
                return template.IsAspx || template.ShowAsMasterTemplate;
            }

            return false;
        }


        /// <summary>
        /// Returns inheritance type form page info object
        /// </summary>
        /// <param name="pi">Page info</param>
        /// <param name="specific">Specific list info</param>
        private InheritanceType GetInheritanceType(PageInfo pi, ref List<int> specific)
        {
            // Get inherit page levels form document
            string inheritPageLevels = pi.GetUsedInheritPageLevels();

            // empty == Use page template settings
            if (String.IsNullOrEmpty(inheritPageLevels))
            {
                PageTemplateInfo template = CurrentPageInfo.UsedPageTemplateInfo;
                if (template != null)
                {
                    inheritPageLevels = template.InheritPageLevels;
                }
            }

            // Set specific levels if required
            InheritanceType type = GetInheritanceType(inheritPageLevels);
            if (type == InheritanceType.Specific)
            {
                inheritPageLevels = inheritPageLevels.Replace("{", "").Replace("}", "");
                inheritPageLevels = inheritPageLevels.Trim('/');
                specific = inheritPageLevels.Split('/').Select(item => Convert.ToInt32(item)).ToList<int>();
            }

            return type;
        }


        /// <summary>
        /// Returns inheritance type form string representation of inheritance
        /// </summary>
        /// <param name="inheritedLevels">Level string</param>
        private InheritanceType GetInheritanceType(string inheritedLevels)
        {
            if (!String.IsNullOrEmpty(inheritedLevels))
            {
                switch (inheritedLevels)
                {
                    case "/":
                        return InheritanceType.None;

                    case "\\":
                        return InheritanceType.Master;

                    default:
                        return InheritanceType.Specific;
                }
            }
            return InheritanceType.All;
        }


        /// <summary>
        /// Gets the Page info of the specific page level
        /// </summary>
        /// <param name="index">Page level index</param>
        public PageInfo GetPageLevel(int index)
        {
            if (index < Pages.Count)
            {
                return Pages[index];
            }
            return RootPageInfo;
        }
    }


    /// <summary>
    /// Inheritance type enum
    /// </summary>
    internal enum InheritanceType
    {
        /// <summary>
        /// Inherit all
        /// </summary>
        All,

        /// <summary>
        /// Do not inherit
        /// </summary>
        None,

        /// <summary>
        /// Inherit only master
        /// </summary>
        Master,

        /// <summary>
        /// Level specific inheritance
        /// </summary>
        Specific

    }
}
