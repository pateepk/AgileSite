using System;
using System.Web;
using System.Collections;
using System.Data;

using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page template tree structure provider.
    /// </summary>
    public class PageTemplateTreeProvider : StaticSiteMapProvider
    {
        #region "Private items"

        /// <summary>
        /// A table of all the page templates, indexed by PageTemplateID.
        /// </summary>
        private Hashtable mPageTemplates = new Hashtable();

        /// <summary>
        /// A table of all the categories, indexed by CategoryID.
        /// </summary>
        private Hashtable mCategories = new Hashtable();


        /// <summary>
        /// Sitemap root node.
        /// </summary>
        private SiteMapNode mRootNode;

        #endregion


        #region "SiteMapProvider properties"

        /// <summary>
        /// If is set to value greater than zero then only allowed templates will be shown in page template tree.
        /// </summary>
        private static int mSiteId;


        /// <summary>
        /// If is set to value greater than zero then only allowed templates will be shown in page template tree.
        /// </summary>
        public static int ShowOnlyAllowedForSiteId
        {
            set
            {
                if (value > 0)
                {
                    mSiteId = value;
                }
            }
        }


        /// <summary>
        /// True if only categories should be shown.
        /// </summary>
        public bool ShowOnlyCategories
        {
            set;
            get;
        }


        /// <summary>
        /// Property to get root node for the tree.
        /// </summary>
        public override SiteMapNode RootNode
        {
            get
            {
                if (mRootNode == null)
                {
                    BuildSiteMap();
                }
                return mRootNode;
            }
        }


        /// <summary>
        /// Specifies if the item data should be bound to the nodes.
        /// </summary>
        public bool BindItemData
        {
            get;
            set;
        }

        #endregion


        #region "SiteMapProvider methods"

        /// <summary>
        /// Returns the the root sitemap node.
        /// </summary>
        protected override SiteMapNode GetRootNodeCore()
        {
            return RootNode;
        }


        /// <summary>
        /// Clean up any collections or other state that an instance of this may hold.
        /// </summary>
        protected override void Clear()
        {
            mRootNode = null;
            mPageTemplates.Clear();
            mCategories.Clear();
            base.Clear();
        }


        /// <summary>
        /// Reloads the tree data.
        /// </summary>
        public void ReloadData()
        {
            Clear();
        }


        /// <summary>
        /// Performs the tree build.
        /// </summary>
        public override SiteMapNode BuildSiteMap()
        {
            if (mRootNode != null)
            {
                return mRootNode;
            }

            // Get the root node
            PageTemplateTreeNode root = new PageTemplateTreeNode(this, "0", "", "Categories");
            // Add root category record
            PageTemplateCategoryInfo rootCategory = new PageTemplateCategoryInfo();
            rootCategory.DisplayName = "Categories";
            root.ItemData = rootCategory;

            AddNode(root);
            mCategories[0] = root;

            // Get the categories            
            DataSet ds;
            if (mSiteId == 0)
            {
                ds = PageTemplateCategoryInfoProvider.GetCategoriesList(null, "CategoryDisplayName");
            }
            else
            {
                ds = PageTemplateCategoryInfoProvider.GetSiteCategoriesList(mSiteId, null, "CategoryDisplayName");
            }

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ds = RemoveEmptyCategories(ds);
                PopulateCategories(ds, root);
            }

            // If the templates should be shown
            if (!ShowOnlyCategories)
            {
                // Get the templates
                if (mSiteId == 0)
                {
                    ds = PageTemplateInfoProvider.GetTemplates();
                }
                else
                {
                    ds = PageTemplateInfoProvider.GetAllowedTemplates(mSiteId);
                }
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Populate templates parts between categories
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int pageTemplateId = ValidationHelper.GetInteger(dr["PageTemplateID"], 0);
                        if (pageTemplateId > 0)
                        {
                            // Get the parent category and node
                            int categoryId = ValidationHelper.GetInteger(dr["PageTemplateCategoryID"], 0);

                            PageTemplateTreeNode parentNode = null;
                            if (categoryId > 0)
                            {
                                parentNode = (PageTemplateTreeNode)mCategories[categoryId];
                            }
                            if (parentNode == null)
                            {
                                parentNode = root;
                            }
                            // Create a new node
                            PageTemplateTreeNode newNode = new PageTemplateTreeNode(this, "p" + pageTemplateId.ToString(), "", ValidationHelper.GetString(dr["PageTemplateDisplayName"], ""));
                            if (BindItemData)
                            {
                                newNode.ItemData = new PageTemplateInfo(dr);
                            }
                            AddNode(newNode, parentNode);
                            mPageTemplates[pageTemplateId] = newNode;
                        }
                    }
                }
            }

            //Set the root
            mRootNode = root;
            return mRootNode;
        }


        /// <summary>
        /// Removes categories with no page templates.
        /// </summary>
        /// <param name="ds">Dataset with the categories data</param>
        private DataSet RemoveEmptyCategories(DataSet ds)
        {
            bool categoryRemoved;

            do
            {
                categoryRemoved = false;
                ArrayList leafesToRemove = new ArrayList();

                // Find the leafes of the tree
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int categoryId = ValidationHelper.GetInteger(dr["CategoryID"], 0);
                    int templatesCount = ValidationHelper.GetInteger(dr["PageTemplateCount"], 0);
                    DataRow[] children = ds.Tables[0].Select("CategoryParentID=" + categoryId);
                    bool isLeaf = (children.Length == 0);

                    // Find empty categories
                    if (isLeaf && (templatesCount == 0))
                    {
                        leafesToRemove.Add(dr);
                    }
                }

                // Remove empty categories
                foreach (DataRow dr in leafesToRemove)
                {
                    ds.Tables[0].Rows.Remove(dr);
                    categoryRemoved = true;
                }
            } while (categoryRemoved);

            return ds;
        }


        /// <summary>
        /// Populates the categories within the tree (up-down processing).
        /// </summary>
        /// <param name="ds">Dataset with the categories data</param>
        /// <param name="parentNode">Parent category node</param>
        private void PopulateCategories(DataSet ds, SiteMapNode parentNode)
        {
            // Prepare the select condition
            int categoryId = ValidationHelper.GetInteger(parentNode.Key, 0);
            string where;
            if (categoryId <= 0)
            {
                where = "CategoryParentID IS NULL OR CategoryParentID = 0";
            }
            else
            {
                where = "CategoryParentID = " + categoryId;
            }
            // Get the child categories
            DataRow[] childCategories = ds.Tables[0].Select(where);
            if (childCategories != null)
            {
                // Add child categories
                foreach (DataRow dr in childCategories)
                {
                    // Add the new node
                    int childId = ValidationHelper.GetInteger(dr["CategoryID"], 0);
                    if (childId > 0)
                    {
                        PageTemplateTreeNode newNode = new PageTemplateTreeNode(this, childId.ToString(), "", ValidationHelper.GetString(dr["CategoryDisplayName"], ""));
                        if (BindItemData)
                        {
                            newNode.ItemData = new PageTemplateCategoryInfo(dr);
                        }
                        AddNode(newNode, parentNode);
                        mCategories[childId] = newNode;
                        // Populate the child node categories
                        PopulateCategories(ds, newNode);
                    }
                }
            }
        }


        /// <summary>
        /// Returns the PageTemplate specified by given PageTemplate ID.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplate ID to retrieve</param>
        public PageTemplateTreeNode GetPageTemplateById(int pageTemplateId)
        {
            BuildSiteMap();
            return (PageTemplateTreeNode)mPageTemplates[pageTemplateId];
        }


        /// <summary>
        /// Returns the PageTemplateCategory specified by given PageTemplateCategory ID.
        /// </summary>
        /// <param name="pageTemplateCategoryId">PageTemplateCategory ID to retrieve</param>
        public PageTemplateTreeNode GetPageTemplateCategoryById(int pageTemplateCategoryId)
        {
            BuildSiteMap();
            return (PageTemplateTreeNode)mCategories[pageTemplateCategoryId];
        }

        #endregion
    }
}