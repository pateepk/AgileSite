using System;
using System.Data;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;

namespace NHG_C
{
    public partial class BlueKey_Temporary_ImportOldContent : System.Web.UI.Page
    {
        private string SITE = "grandStrandNewHomeGuide";
        private int SITEID = 5;
        private int NEIGHBORHOODNODEID = 2918;
        /*
            private string SITE = "TheTriadNewHomesGuide";
            private int SITEID = 4;
            private int NEIGHBORHOODNODEID = 16;
        */
        protected void Page_Load(object sender, EventArgs e)
        {
            if (MembershipContext.AuthenticatedUser.IsPublic() == true)
            {
                Response.Redirect("~/Admin");
            }
        }

        public void btnMigratePages_click(object sender, EventArgs e)
        {
            SyncPages();
        }

        public void btnMigrateBlogs_click(object sender, EventArgs e)
        {
            SyncBlogs();
        }

        public void btnMigrateNeighborhoods_click(object sender, EventArgs e)
        {
            SyncNeighborhoods();
        }

        public void btnMigrateDevelopers_click(object sender, EventArgs e)
        {
            SyncDevelopers();
        }

        public void btnMigrateInventory_click(object sender, EventArgs e)
        {
            //UnPublishInventory(); // Triad is not using this
            SyncInventory();
        }

        private void SyncPages()
        {
            DataSet pages = GetOldPages();

            if (!DataHelper.DataSourceIsEmpty(pages))
            {
                foreach (DataRow r in pages.Tables[0].Rows)
                {
                    _d("Looking For: " + r["DocumentName"].ToString());

                    //TreeNode node = GetPageByName(r["DocumentName"].ToString());

                    TreeProvider tp = new TreeProvider();
                    TreeNode node = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeID"], -1));

                    if (node != null)
                    {
                        _d("-- Found Page");

                        node.DocumentName = r["DocumentName"].ToString();
                        node.DocumentCulture = "en-us";
                        //node.SetValue("BlogMonthName", r["BlogMonthName"]);
                        //node.SetValue("BlogMonthStartingDate", r["BlogMonthStartingDate"]);

                        //node.Update();

                        node.SetValue("DocumentContent", r["DocumentContent"]);

                        DocumentHelper.UpdateDocument(node);
                    }
                    else
                    {
                        _d("-- Need to add Page");

                        TreeNode parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeParentID"], -1), "en-us");

                        if (parentPage != null)
                        {
                            _d("----- Adding Page under node id = " + ValidationHelper.GetString(r["NodeParentID"], String.Empty));

                            TreeNode page = TreeNode.New("cms.MenuItem", tp);

                            page.DocumentName = r["DocumentName"].ToString();
                            page.DocumentCulture = "en-us";

                            page.SetValue("DocumentContent", r["DocumentContent"]);

                            page.Insert(parentPage);
                        }
                        else
                        {
                            _d("---- Parent Page Not Found, skipping");
                        }
                    }
                }
            }
        }

        private DataSet GetOldPages()
        {
            DataSet pages = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@SiteID", SITEID);

                string sql = @"select * from View_CMS_Tree_Joined
                            where ClassName = 'CMS.MenuItem'
                            and NodeSiteID = @SiteID;";

                QueryParameters qp = new QueryParameters(sql, p, QueryTypeEnum.SQLQuery);

                pages = gc.ExecuteQuery(qp);
            }

            return pages;
        }

        private TreeNode GetPageByName(string name)
        {
            TreeNode node = null;

            QueryDataParameters p = new QueryDataParameters();
            p.Add("@Name", name);

            var pages = DocumentHelper.GetDocuments("cms.MenuItem")
                                           .Where("MenuItemName = @Name", p)
                                           .OnSite(SITEID);

            if (pages.Count > 0)
            {
                node = pages.FirstObject;
            }

            return node;
        }

        private void SyncBlogs()
        {
            DataSet blogMonths = GetOldBlogMonths();

            if (!DataHelper.DataSourceIsEmpty(blogMonths))
            {
                foreach (DataRow r in blogMonths.Tables[0].Rows)
                {
                    _d("Looking For: " + r["BlogMonthName"].ToString());

                    TreeNode node = GetBlogMonthByName(r["BlogMonthName"].ToString());

                    if (node != null)
                    {
                        _d("----- Found this Month");

                        node.DocumentName = r["BlogMonthName"].ToString();
                        node.DocumentCulture = "en-us";
                        node.SetValue("BlogMonthName", r["BlogMonthName"]);
                        node.SetValue("BlogMonthStartingDate", r["BlogMonthStartingDate"]);

                        node.Update();
                    }
                    else
                    {
                        _d("----- Need to add this Month");

                        TreeProvider tp = new TreeProvider();

                        NodeSelectionParameters nsp = new NodeSelectionParameters();

                        nsp.SiteName = SITE;
                        nsp.AliasPath = "/Resources/New-Home-Guide-Blog";

                        TreeNode parentPage = tp.SelectSingleNode(nsp);

                        if (parentPage != null)
                        {
                            TreeNode newBlogMonth = TreeNode.New("cms.blogmonth", tp);

                            newBlogMonth.DocumentName = r["BlogMonthName"].ToString();
                            newBlogMonth.DocumentCulture = "en-us";
                            newBlogMonth.SetValue("BlogMonthName", r["BlogMonthName"]);
                            newBlogMonth.SetValue("BlogMonthStartingDate", r["BlogMonthStartingDate"]);

                            newBlogMonth.Insert(parentPage);
                        }
                    }
                }
            }

            DataSet blogPosts = GetOldBlogPosts();

            if (!DataHelper.DataSourceIsEmpty(blogPosts))
            {
                foreach (DataRow r in blogPosts.Tables[0].Rows)
                {
                    _d("Looking For: " + r["BlogPostTitle"].ToString());

                    TreeNode node = GetBlogPostByName(r["BlogPostTitle"].ToString());

                    if (node != null)
                    {
                        _d("----- Found Blog Post");

                        node.DocumentName = r["BlogPostTitle"].ToString();
                        node.DocumentCulture = "en-us";
                        node.SetValue("BlogPostTitle", r["BlogPostTitle"]);
                        node.SetValue("BlogPostDate", r["BlogPostDate"]);
                        node.SetValue("BlogPostSummary", r["BlogPostSummary"]);
                        node.SetValue("BlogPostBody", r["BlogPostBody"]);
                        node.SetValue("BlogPostTeaser", r["BlogPostTeaser"]);
                        node.SetValue("BlogPostAllowComments", r["BlogPostAllowComments"]);
                        node.SetValue("BlogPostPingedUrls", r["BlogPostPingedUrls"]);
                        node.SetValue("BlogPostNotPingedUrls", r["BlogPostNotPingedUrls"]);
                        //node.SetValue("BlogLogActivity", r["BlogLogActivity"]);

                        node.Update();
                    }
                    else
                    {
                        _d("----- Need to Add This Post");

                        TreeProvider tp = new TreeProvider();

                        TreeNode parentPage = GetBlogMonthByName(GetOldNodeName(Convert.ToInt32(r["NodeParentID"])));

                        if (parentPage != null)
                        {
                            TreeNode newBlogPost = TreeNode.New("cms.blogpost", tp);

                            newBlogPost.DocumentName = r["BlogPostTitle"].ToString();
                            newBlogPost.DocumentCulture = "en-us";
                            newBlogPost.SetValue("BlogPostTitle", r["BlogPostTitle"]);
                            newBlogPost.SetValue("BlogPostDate", r["BlogPostDate"]);
                            newBlogPost.SetValue("BlogPostSummary", r["BlogPostSummary"]);
                            newBlogPost.SetValue("BlogPostBody", r["BlogPostBody"]);
                            newBlogPost.SetValue("BlogPostTeaser", r["BlogPostTeaser"]);
                            newBlogPost.SetValue("BlogPostAllowComments", r["BlogPostAllowComments"]);
                            //newBlogPost.SetValue("BlogPostPingedUrls", r["BlogPostPingedUrls"]);
                            //newBlogPost.SetValue("BlogPostNotPingedUrls", r["BlogPostNotPingedUrls"]);
                            //newBlogPost.SetValue("BlogLogActivity", r["BlogLogActivity"]);

                            newBlogPost.Insert(parentPage);
                        }
                    }
                }
            }
        }

        private DataSet GetOldBlogMonths()
        {
            DataSet blogMonths = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                QueryParameters qp = new QueryParameters("select * from content_blogmonth", null, QueryTypeEnum.SQLQuery);

                blogMonths = gc.ExecuteQuery(qp);
            }

            return blogMonths;
        }

        private TreeNode GetBlogMonthByName(string name)
        {
            TreeNode node = null;
            QueryDataParameters p = new QueryDataParameters();
            p.Add("@BlogMonthName", name);

            var blogmonths = DocumentHelper.GetDocuments("cms.blogmonth")
                                           .Where("BlogMonthName = @BlogMonthName", p);

            if (blogmonths.Count > 0)
            {
                node = blogmonths.FirstObject;
            }

            return node;
        }

        private DataSet GetOldBlogPosts()
        {
            DataSet blogPosts = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@SiteID", SITEID);

                QueryParameters qp = new QueryParameters("select * from view_content_blogpost_joined where NodeSiteID = @SiteID", p, QueryTypeEnum.SQLQuery);

                blogPosts = gc.ExecuteQuery(qp);
            }

            return blogPosts;
        }

        private TreeNode GetBlogPostByName(string name)
        {
            TreeNode node = null;
            QueryDataParameters p = new QueryDataParameters();
            p.Add("@BlogPostTitle", name);

            var blogposts = DocumentHelper.GetDocuments("cms.blogpost")
                                           .Where("BlogPostTitle = @BlogPostTitle", p)
                                           .OnSite(SITEID);

            if (blogposts.Count > 0)
            {
                node = blogposts.FirstObject;
            }

            return node;
        }

        /*
            private void UnPublishInventory()
            {
                var inventory = DocumentHelper.GetDocuments("custom.listing").Published();

                foreach(TreeNode n in inventory)
                {
                    n.CheckOut();

                    //_d(n.DocumentName + " - " + n.DocumentNamePath);
                    n.SetValue("DocumentPublishTo", DateTime.Now.AddDays(-5));
                    n.Update();

                    n.CheckIn();

                }

                _d("All Inventory Unpublished");
            }
        */
        private void SyncInventory()
        {
            DataSet inventory = GetOldInventory();

            if (!DataHelper.DataSourceIsEmpty(inventory))
            {
                foreach (DataRow r in inventory.Tables[0].Rows)
                {
                    _d("Looking For: " + r["ListingTitle"].ToString());

                    //TreeNode node = GetInventoryByNameAndParentName(r["ListingTitle"].ToString(), GetOldNodeName(Convert.ToInt32(r["NodeParentID"])));

                    TreeProvider tp = new TreeProvider();
                    TreeNode node = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeID"], -1));

                    if (node != null)
                    {
                        _d("-- Found Listing");

                        node.DocumentName = r["ListingTitle"].ToString();
                        node.DocumentCulture = "en-us";
                        node.SetValue("ListingLotNumber", r["ListingLotNumber"]);
                        node.SetValue("ListingDateReady", r["ListingDateReady"]);
                        node.SetValue("ListingStory", r["ListingStory"]);
                        node.SetValue("ListingBedrooms", r["ListingBedrooms"]);
                        node.SetValue("ListingBathrooms", r["ListingBathrooms"]);
                        node.SetValue("ListingSquareFootage", r["ListingSquareFootage"]);
                        node.SetValue("ListingPlan", r["ListingPlan"]);
                        node.SetValue("ListingPrice", r["ListingPrice"]);
                        node.SetValue("ListingDescription", r["ListingDescription"]);
                        node.SetValue("ListingTitle", r["ListingTitle"]);
                        node.SetValue("ListingDeveloper", r["ListingDeveloper"]);
                        node.SetValue("ListingMoveInSpecial", r["ListingMoveInSpecial"]);
                        node.SetValue("ListingMoveInSpecialContent", r["ListingMoveInSpecialContent"]);
                        node.SetValue("ListingType", r["ListingType"]);
                        node.SetValue("ListingMoveInStatus", r["ListingMoveInStatus"]);
                        node.SetValue("ListingMLSLink", r["ListingMLSLink"]);
                        node.SetValue("ListingReadyText", r["ListingReadyText"]);
                        node.SetValue("ListingCity", r["ListingCity"]);
                        node.SetValue("DocumentPublishTo", null);

                        //node.Update();

                        node.SetValue("NodeParentID", ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        DocumentHelper.UpdateDocument(node);
                    }
                    else
                    {
                        _d("-- Need to add Listing");

                        TreeNode parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        if (parentPage != null)
                        {
                            _d("---- Listing Parent Found");

                            InsertListing(parentPage, r);
                        }
                        else
                        {
                            _d("---- Listing Parent Not Found");

                            parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(NEIGHBORHOODNODEID, -1));

                            if (parentPage != null)
                            {
                                _d("------ Putting under Neighborhoods folder (node id = " + NEIGHBORHOODNODEID + ")");

                                InsertListing(parentPage, r);
                            }
                            else
                            {
                                _d("------ Neighborhoods folder not found, skipping...");
                            }
                        }
                    }
                }
            }
        }

        private void InsertListing(TreeNode parentPage, DataRow r)
        {
            TreeNode newListing = TreeNode.New("custom.listing", new TreeProvider());

            newListing.DocumentName = r["ListingTitle"].ToString();
            newListing.DocumentCulture = "en-us";
            newListing.SetValue("ListingLotNumber", r["ListingLotNumber"]);
            newListing.SetValue("ListingDateReady", r["ListingDateReady"]);
            newListing.SetValue("ListingStory", r["ListingStory"]);
            newListing.SetValue("ListingBedrooms", r["ListingBedrooms"]);
            newListing.SetValue("ListingBathrooms", r["ListingBathrooms"]);
            newListing.SetValue("ListingSquareFootage", r["ListingSquareFootage"]);
            newListing.SetValue("ListingPlan", r["ListingPlan"]);
            newListing.SetValue("ListingPrice", r["ListingPrice"]);
            newListing.SetValue("ListingDescription", r["ListingDescription"]);
            newListing.SetValue("ListingTitle", r["ListingTitle"]);
            newListing.SetValue("ListingDeveloper", r["ListingDeveloper"]);
            newListing.SetValue("ListingMoveInSpecial", r["ListingMoveInSpecial"]);
            newListing.SetValue("ListingMoveInSpecialContent", r["ListingMoveInSpecialContent"]);
            newListing.SetValue("ListingType", r["ListingType"]);
            newListing.SetValue("ListingMoveInStatus", r["ListingMoveInStatus"]);
            newListing.SetValue("ListingMLSLink", r["ListingMLSLink"]);
            newListing.SetValue("ListingReadyText", r["ListingReadyText"]);
            newListing.SetValue("ListingCity", r["ListingCity"]);

            newListing.Insert(parentPage);

            _d("------ Listing Added");
        }

        private string GetOldNodeName(int NodeID)
        {
            DataSet nodes = null;
            string nodeName = String.Empty;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@NodeID", NodeID);

                QueryParameters qp = new QueryParameters("select NodeName from cms_tree WHERE NodeID = @NodeID", p, QueryTypeEnum.SQLQuery);

                nodes = gc.ExecuteQuery(qp);

                if (!DataHelper.DataSourceIsEmpty(nodes))
                {
                    nodeName = nodes.Tables[0].Rows[0]["NodeName"].ToString();
                }

            }

            return nodeName;
        }

        private TreeNode GetInventoryByNameAndParentName(string name, string parentName)
        {
            TreeNode node = null;

            QueryDataParameters p = new QueryDataParameters();
            p.Add("@ListingTitle", name);

            var inventory = DocumentHelper.GetDocuments("custom.listing")
                                              .Where("ListingTitle = @ListingTitle", p);

            if (inventory.Count > 0)
            {
                node = inventory.FirstObject;

                if (node.Parent.DocumentName != parentName)
                {
                    node = null;
                }
            }

            return node;
        }

        private DataSet GetOldInventory()
        {
            DataSet inventory = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@SiteID", SITEID);

                QueryParameters qp = new QueryParameters("select * from view_custom_listing_joined where NodeSiteID = @SiteID", p, QueryTypeEnum.SQLQuery);

                inventory = gc.ExecuteQuery(qp);
            }

            return inventory;
        }

        private void SyncDevelopers()
        {
            DataSet developers = GetOldDevelopers();

            if (!DataHelper.DataSourceIsEmpty(developers))
            {
                foreach (DataRow r in developers.Tables[0].Rows)
                {
                    _d("Looking For: " + r["DeveloperName"].ToString());

                    //TreeNode node = GetDeveloperByName(r["DeveloperName"].ToString());

                    TreeProvider tp = new TreeProvider();
                    TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(r["NodeID"], -1), "en-us", tp);

                    if (node != null)
                    {
                        _d("-- Found Developer");

                        node.DocumentName = r["DeveloperName"].ToString();
                        node.DocumentCulture = "en-us";
                        node.SetValue("DeveloperEmails", r["DeveloperEmails"]);
                        node.SetValue("DeveloperActive", r["DeveloperActive"]);
                        node.SetValue("DeveloperWebsite", r["DeveloperWebsite"]);
                        node.SetValue("DeveloperCustom", r["DeveloperCustom"]);
                        node.SetValue("DeveloperPriceRangeLow", r["DeveloperPriceRangeLow"]);
                        node.SetValue("DeveloperPriceRangeHigh", r["DeveloperPriceRangeHigh"]);
                        node.SetValue("DeveloperPhone", r["DeveloperPhone"]);
                        node.SetValue("DeveloperDescription", r["DeveloperDescription"]);
                        node.SetValue("DeveloperMedia", r["DeveloperMedia"]);
                        node.SetValue("DeveloperAddress", r["DeveloperAddress"]);
                        node.SetValue("DeveloperPhoto", r["DeveloperPhoto"]);
                        //node.SetValue("DeveloperProfileLink", r["DeveloperProfileLink"]);
                        node.SetValue("DeveloperPriceRangeText", r["DeveloperPriceRangeText"]);
                        node.SetValue("DeveloperSiteClicks", r["DeveloperSiteClicks"]);
                        //node.SetValue("DeveloperAdvertisements", r["DeveloperAdvertisements"]);

                        //node.Update();

                        node.SetValue("NodeParentID", ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        DocumentHelper.UpdateDocument(node);
                    }
                    else
                    {
                        _d("-- Need to Add This Developer");

                        /*
                                            TreeNode parentPage = tp.SelectNodes()
                                                                    .Path("/Builders/New-Builders")
                                                                    .FirstObject;
                        */

                        TreeNode parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        if (parentPage != null)
                        {
                            _d("---- Developer Parent Found");

                            TreeNode d = TreeNode.New("custom.developers", tp);

                            d.DocumentName = r["DeveloperName"].ToString();
                            d.DocumentCulture = "en-us";
                            d.SetValue("DeveloperEmails", r["DeveloperEmails"]);
                            d.SetValue("DeveloperActive", r["DeveloperActive"]);
                            d.SetValue("DeveloperWebsite", r["DeveloperWebsite"]);
                            d.SetValue("DeveloperCustom", r["DeveloperCustom"]);
                            d.SetValue("DeveloperPriceRangeLow", r["DeveloperPriceRangeLow"]);
                            d.SetValue("DeveloperPriceRangeHigh", r["DeveloperPriceRangeHigh"]);
                            d.SetValue("DeveloperPhone", r["DeveloperPhone"]);
                            d.SetValue("DeveloperDescription", r["DeveloperDescription"]);
                            d.SetValue("DeveloperMedia", r["DeveloperMedia"]);
                            d.SetValue("DeveloperAddress", r["DeveloperAddress"]);
                            d.SetValue("DeveloperPhoto", r["DeveloperPhoto"]);
                            //d.SetValue("DeveloperProfileLink", r["DeveloperProfileLink"]);
                            d.SetValue("DeveloperPriceRangeText", r["DeveloperPriceRangeText"]);
                            d.SetValue("DeveloperSiteClicks", r["DeveloperSiteClicks"]);
                            //d.SetValue("DeveloperAdvertisements", r["DeveloperAdvertisements"]);

                            d.SetValue("NodeSiteID", SITEID);

                            d.Insert(parentPage);

                            _d("------ Developer Added");
                        }
                        else
                        {
                            _d("---- Developer Parent Not Found");
                        }
                    }
                }
            }
        }
        /*
            private TreeNode GetDeveloperByName(string name)
            {
                TreeNode node = null;

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@DeveloperName", name);

                var developers = DocumentHelper.GetDocuments("custom.developers")
                                                  .Where("DeveloperName = @DeveloperName", p);

                if (developers.Count > 0)
                {
                    node = developers.FirstObject;
                }

                return node;
            }
        */
        private DataSet GetOldDevelopers()
        {
            DataSet developers = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@SiteID", SITEID);

                string sql = "select* from View_custom_Developers_Joined where NodeSiteID = @SiteID";

                QueryParameters qp = new QueryParameters(sql, p, QueryTypeEnum.SQLQuery);

                developers = gc.ExecuteQuery(qp);
            }

            return developers;
        }

        private void SyncNeighborhoods()
        {
            DataSet neighborhoods = GetOldNeighborhoods();

            if (!DataHelper.DataSourceIsEmpty(neighborhoods))
            {
                foreach (DataRow r in neighborhoods.Tables[0].Rows)
                {
                    _d("Looking For: " + r["NeighborhoodName"].ToString());

                    //TreeNode node = GetNeighborhoodByName(r["NeighborhoodName"].ToString());

                    TreeProvider tp = new TreeProvider();
                    TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(r["NodeID"], -1), "en-us", tp);

                    if (node != null)
                    {
                        _d("-- Found Neighborhood");

                        node.DocumentName = r["NeighborhoodName"].ToString();
                        node.DocumentCulture = "en-us";
                        node.SetValue("NeighborhoodPriceRangeLow", r["NeighborhoodPriceRangeLow"]);
                        node.SetValue("NeighborhoodPriceRangeHigh", r["NeighborhoodPriceRangeHigh"]);
                        node.SetValue("NeighborhoodPhoneNumber", r["NeighborhoodPhoneNumber"]);
                        node.SetValue("NeighborhoodDescription", r["NeighborhoodDescription"]);
                        node.SetValue("NeighborhoodWebsite", r["NeighborhoodWebsite"]);
                        node.SetValue("NeighborhoodAddress", r["NeighborhoodAddress"]);
                        node.SetValue("NeighborhoodDevelopers", r["NeighborhoodDevelopers"]);
                        node.SetValue("NeighborhoodTypes", r["NeighborhoodTypes"]);
                        node.SetValue("NeighborhoodLifestyle", r["NeighborhoodLifestyle"]);
                        node.SetValue("NeighborhoodEmails", r["NeighborhoodEmails"]);
                        node.SetValue("NeighborhoodActive", r["NeighborhoodActive"]);
                        node.SetValue("NeighborhoodMediaDescription", r["NeighborhoodMediaDescription"]);
                        node.SetValue("NeighborhoodCustom", r["NeighborhoodCustom"]);
                        node.SetValue("NeighborhoodLattitude", r["NeighborhoodLattitude"]);
                        node.SetValue("NeighborhoodDirectionsText", r["NeighborhoodDirectionsText"]);
                        node.SetValue("NeighborhoodPriceRangeText", r["NeighborhoodPriceRangeText"]);
                        node.SetValue("NeighborhoodSiteClicks", r["NeighborhoodSiteClicks"]);
                        node.SetValue("NeighborhoodCounty", r["NeighborhoodCounty"]);
                        node.SetValue("NeighborhoodCity", r["NeighborhoodCity"]);
                        //node.SetValue("NeighborhoodAdvertisements", r["NeighborhoodAdvertisements"]);
                        //node.SetValue("FloorplanSiteClicks", r["FloorplanSiteClicks"]);

                        //node.Update();

                        TreeNode parentNode = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        if (parentNode != null)
                        {
                            node.SetValue("NodeParentID", ValidationHelper.GetInteger(r["NodeParentID"], -1));
                        }
                        else
                        {
                            // if parent doesn't exist, set parent to Neighborhoods
                            node.SetValue("NodeParentID", NEIGHBORHOODNODEID);
                        }

                        DocumentHelper.UpdateDocument(node);
                    }
                    else
                    {
                        _d("-- Need to Add This Neighborhood");

                        /*
                                            TreeNode parentPage = tp.SelectNodes()
                                                                    .Path("/Neighborhoods/New-Neighborhoods")
                                                                    .FirstObject;
                        */

                        TreeNode parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(r["NodeParentID"], -1));

                        if (parentPage != null)
                        {
                            _d("---- Neighborhood Parent Found");

                            InsertNeighborhood(parentPage, r);
                        }
                        else
                        {
                            _d("---- Neighborhood Parent Not Found");

                            parentPage = tp.SelectSingleNode(ValidationHelper.GetInteger(NEIGHBORHOODNODEID, -1));

                            if (parentPage != null)
                            {
                                _d("------ Putting under Neighborhoods folder (node id = " + NEIGHBORHOODNODEID + ")");

                                InsertNeighborhood(parentPage, r);
                            }
                            else
                            {
                                _d("------ Neighborhoods folder not found, skipping...");
                            }
                        }
                    }
                }
            }
        }

        private void InsertNeighborhood(TreeNode parentPage, DataRow r)
        {
            TreeNode n = TreeNode.New("custom.neighborhood", new TreeProvider());

            n.DocumentName = r["NeighborhoodName"].ToString();
            n.DocumentCulture = "en-us";
            n.SetValue("NeighborhoodPriceRangeLow", r["NeighborhoodPriceRangeLow"]);
            n.SetValue("NeighborhoodPriceRangeHigh", r["NeighborhoodPriceRangeHigh"]);
            n.SetValue("NeighborhoodPhoneNumber", r["NeighborhoodPhoneNumber"]);
            n.SetValue("NeighborhoodDescription", r["NeighborhoodDescription"]);
            n.SetValue("NeighborhoodWebsite", r["NeighborhoodWebsite"]);
            n.SetValue("NeighborhoodAddress", r["NeighborhoodAddress"]);
            n.SetValue("NeighborhoodDevelopers", r["NeighborhoodDevelopers"]);
            n.SetValue("NeighborhoodTypes", r["NeighborhoodTypes"]);
            n.SetValue("NeighborhoodLifestyle", r["NeighborhoodLifestyle"]);
            n.SetValue("NeighborhoodEmails", r["NeighborhoodEmails"]);
            n.SetValue("NeighborhoodActive", r["NeighborhoodActive"]);
            n.SetValue("NeighborhoodMediaDescription", r["NeighborhoodMediaDescription"]);
            n.SetValue("NeighborhoodCustom", r["NeighborhoodCustom"]);
            n.SetValue("NeighborhoodLattitude", r["NeighborhoodLattitude"]);
            n.SetValue("NeighborhoodDirectionsText", r["NeighborhoodDirectionsText"]);
            n.SetValue("NeighborhoodPriceRangeText", r["NeighborhoodPriceRangeText"]);
            n.SetValue("NeighborhoodSiteClicks", r["NeighborhoodSiteClicks"]);
            n.SetValue("NeighborhoodCounty", r["NeighborhoodCounty"]);
            n.SetValue("NeighborhoodCity", r["NeighborhoodCity"]);
            //n.SetValue("NeighborhoodAdvertisements", r["NeighborhoodAdvertisements"]);
            //n.SetValue("FloorplanSiteClicks", r["FloorplanSiteClicks"]);

            n.SetValue("NodeSiteID", SITEID);

            n.Insert(parentPage);

            _d("------ Neighborhood Added");
        }

        private TreeNode GetNeighborhoodByName(string name)
        {
            TreeNode node = null;

            QueryDataParameters p = new QueryDataParameters();
            p.Add("@NeighborhoodName", name);

            var neighborhoods = DocumentHelper.GetDocuments("custom.neighborhood")
                                              .Where("NeighborhoodName = @NeighborhoodName", p);

            if (neighborhoods.Count > 0)
            {
                node = neighborhoods.FirstObject;
            }

            return node;
        }

        private DataSet GetOldNeighborhoods()
        {
            DataSet neighborhoods = null;

            using (CMSConnectionScope cs = new CMSConnectionScope("ProdConnectionString", false))
            {
                GeneralConnection gc = ConnectionHelper.GetConnection();

                QueryDataParameters p = new QueryDataParameters();
                p.Add("@SiteID", SITEID);

                QueryParameters qp = new QueryParameters("select * from View_custom_Neighborhood_Joined where NodeSiteID = @SiteID;", p, QueryTypeEnum.SQLQuery);

                neighborhoods = gc.ExecuteQuery(qp);
            }

            return neighborhoods;
        }

        private void _d(string msg)
        {
            dbg.Text += msg + "<br />";
        }
    }
}
