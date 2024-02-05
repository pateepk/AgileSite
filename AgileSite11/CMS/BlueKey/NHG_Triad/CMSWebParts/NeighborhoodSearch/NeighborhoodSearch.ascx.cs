using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_NeighborhoodSearch_NeighborhoodSearch : CMSAbstractWebPart
    {
        #region app_code classes

        /// <summary>
        /// Summary description for NewHomesGuideFunctions
        /// </summary>
        public static class NewHomesGuideFunctions
        {
            public static string GetDeveloperProfileLink(string documentId)
            {
                string result = string.Empty;
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                CMS.DocumentEngine.TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(documentId, -1), tree);
                if (node != null)
                {
                    string profileLink = string.Empty;
                    profileLink = node.GetValue("DeveloperProfileLink", string.Empty);
                    if (!String.IsNullOrEmpty(profileLink))
                    {
                        CMS.DocumentEngine.TreeNode profileNode = tree.SelectSingleNode(new Guid(profileLink), "en-us", SiteContext.CurrentSiteName);
                        if (profileNode != null)
                        {
                            result = "<a href =\"" + profileNode.NodeAliasPath + "\">Builder Profile</a>";
                        }
                    }
                }

                return result;
            }

            public static string GetDeveloperImageForIntro(object neighborhoodId)
            {
                string defaultString = "<a href=\"\"><img src='/BlueKey/Templates/images/sampleBuilderLogo.png' alt=''></a>";
                string returnString = string.Empty;
                if (neighborhoodId == null)
                {
                    return defaultString;
                }

                int id = ValidationHelper.GetInteger(neighborhoodId, -1);
                if (id < 1)
                {
                    return defaultString;
                }

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);
                DataSet ds = tree.SelectNodes()
                    .OnCurrentSite()
                    .Type("custom.neighborhood", q => q.Columns("NeighborhoodDevelopers"))
                    .Where("NeighborhoodID = " + id);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    string developers = ValidationHelper.GetString(ds.Tables[0].Rows[0]["NeighborhoodDevelopers"], string.Empty);
                    if (String.IsNullOrEmpty(developers))
                    {
                        return defaultString;
                    }

                    string[] developerArray = developers.Split('|');
                    foreach (string dev in developerArray)
                    {
                        string img = string.Empty;
                        string url = string.Empty;

                        DataSet developerDs = tree.SelectNodes()
                            .OnCurrentSite()
                            .Type("custom.Developers", q => q.Columns("DeveloperPhoto", "DeveloperProfileLink", "DeveloperName"))
                            .Where("DevelopersID = " + dev);

                        if (!DataHelper.DataSourceIsEmpty(developerDs))
                        {
                            DataRow row = developerDs.Tables[0].Rows[0];
                            if (String.IsNullOrEmpty(ValidationHelper.GetString(row["DeveloperPhoto"], string.Empty)))
                            {
                                img = defaultString;
                            }
                            else
                            {
                                var imgDoc = tree.SelectSingleNode(ValidationHelper.GetGuid(row["DeveloperPhoto"], new Guid()), "en-us", SiteContext.CurrentSiteName);

                                if (imgDoc != null)
                                {
                                    img = "<img src=\"" + imgDoc.NodeAliasPath + "\">";
                                }
                            }

                            returnString += GetDevelopers(dev, true, img + ValidationHelper.GetString(row["DeveloperName"], string.Empty));
                        }
                    }
                }

                if (!String.IsNullOrEmpty(returnString))
                {
                    return returnString;
                }
                else
                {
                    return defaultString;
                }
            }

            public static string GetDeveloperPriceRange(string documentId)
            {
                string result = string.Empty;
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                CMS.DocumentEngine.TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(documentId, -1), tree);
                if (node != null)
                {
                    string lowPrice = string.Empty;
                    string highPrice = string.Empty;
                    lowPrice = node.GetValue("DeveloperPriceRangeLow", string.Empty);
                    highPrice = node.GetValue("DeveloperPriceRangeHigh", string.Empty);

                    result = (String.IsNullOrEmpty(lowPrice) ? "" : "$" + lowPrice) + (String.IsNullOrEmpty(lowPrice) || String.IsNullOrEmpty(highPrice) ? "" : " - ") + (String.IsNullOrEmpty(highPrice) ? "" : "$" + highPrice);
                }

                return result;
            }

            public static string ReturnUrlSrc(string nodeGuidParam = "")
            {
                try
                {
                    if (nodeGuidParam != null && nodeGuidParam.Trim() != "")
                    {
                        string nodeGuidStr = ValidationHelper.GetString(nodeGuidParam, "");
                        if (!String.IsNullOrEmpty(nodeGuidStr))
                        {
                            Guid nodeGUID = new Guid(nodeGuidStr);
                            if (nodeGUID != null)
                            {
                                int nodeId = TreePathUtils.GetNodeIdByNodeGUID(nodeGUID, SiteContext.CurrentSiteName);

                                if (nodeId != null)
                                {
                                    TreeProvider tp = new TreeProvider(MembershipContext.AuthenticatedUser);
                                    CMS.DocumentEngine.TreeNode node = tp.SelectSingleNode(nodeId);

                                    return TreePathUtils.GetDocumentUrl(node.DocumentID).Replace("~/", "/");
                                }
                            }
                        }
                    }
                    return "";
                }
                catch
                {
                    return "";
                }
            }

            public static string GetDevelopers(object txtValue, bool addUrl, string linkText = "")
            {
                return GetDevelopers(txtValue, addUrl, false, linkText);
            }

            public static string GetDevelopers(object txtValue, bool addUrl, bool targetBlank, string linkText = "", bool urlOnly = false)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                string returnText = "";
                string txt = txtValue.ToString();
                int i = 0;

                string[] developers = txt.Split('|');

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                TreeProvider tree = new TreeProvider(ui);

                foreach (string developer in developers)
                {
                    DataSet ds = null;

                    ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = " + developer);

                    if (ds != null)
                    {
                        DataTable dt = ds.Tables[0];

                        foreach (DataRow dr in dt.Rows)
                        {
                            if (Convert.ToInt32(dr["DeveloperActive"]) == 1)
                            {
                                if (urlOnly)
                                {
                                    return ValidationHelper.GetString(dr["NodeAliasPath"], string.Empty);
                                }
                                if (addUrl)
                                {
                                    string target = String.Empty;
                                    if (targetBlank)
                                    {
                                        target = " target='blank' ";
                                    }

                                    if (linkText != "")
                                    {
                                        returnText += "<a href='" + dr["NodeAliasPath"] + ".aspx'" + target + ">" + linkText + "</a>";
                                    }
                                    else
                                    {
                                        returnText += "<a href='" + dr["NodeAliasPath"] + ".aspx'" + target + ">" + dr["DeveloperName"] + "</a>";
                                    }
                                }
                                else
                                {
                                    returnText += dr["DeveloperName"];
                                }
                            }

                            if (++i != developers.Length && Convert.ToInt32(dr["DeveloperActive"]) == 1)
                            {
                                returnText += ", ";
                            }
                        }
                    }
                }

                return returnText;
            }

            //public static string GetDevelopersFloorplan(object txtValue)
            //{
            //    if (txtValue == null || txtValue.ToString().Trim() == "")
            //        return "";

            //    string returnText = "";
            //    string txt = txtValue.ToString();
            //    int i = 0;

            //    string[] developers = ValidationHelper.GetString(txtValue, string.Empty).Split('|');

            //    UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            //    TreeProvider tree = new TreeProvider(ui);

            //    foreach (string developer in developers)
            //    {
            //        DataSet ds = null;

            //        ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = " + developer);

            //        if (ds != null)
            //        {
            //            DataTable dt = ds.Tables[0];

            //            foreach (DataRow dr in dt.Rows)
            //            {
            //          string floorPlanLink = ValidationHelper.GetString(dr["DeveloperFloorPlan"], String.Empty);

            //                if (Convert.ToInt32(dr["DeveloperActive"]) == 1 && floorPlanLink != String.Empty)
            //                {
            //                    returnText += "<a href='" + floorPlanLink + "' target='_blank'>Available Floorplans</a>";

            //                }

            //                if (++i != developers.Length && Convert.ToInt32(dr["DeveloperActive"]) == 1 && floorPlanLink != String.Empty)
            //                {
            //                    returnText += ", ";
            //                }
            //            }

            //        }

            //    }

            //    return returnText;
            //}
            public static string GetDeveloperFloorPlan(object txtValue)
            {
                string href = string.Empty;
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                // int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                //CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);
                //if (node != null)
                //{
                string developers = ValidationHelper.GetString(txtValue, string.Empty);
                string[] developerArray = developers.Split('|');

                foreach (string d in developerArray)
                {
                    DataSet ds = null;

                    ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = " + d);

                    if (ds != null)
                    {
                        DataTable dt = ds.Tables[0];

                        foreach (DataRow dr in dt.Rows)
                        {
                            //href = ValidationHelper.GetString(dr["NodeAliasPath"], string.Empty);
                            href = ValidationHelper.GetString(dr["DeveloperFloorPlanUrl"], string.Empty);
                        }
                    }
                }
                //}

                return href;
            }

            public static string GetCityName(object value)
            {
                string ret = string.Empty;
                try
                {
                    string[] cities = value.ToString().Split('|');
                    DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "itemid=" + cities[0], "", 0);
                    ret = ds.Tables[0].Rows[0]["CityName"].ToString();
                }
                catch { }
                return ret;
            }

            public static string GetNeighborhoodURL(object txtValue)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                TreeProvider tree = new TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                return node.GetValue("NodeAliasPath").ToString();
            }

            public static string GetNeighborhood(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                TreeProvider tree = new TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);
                if (urlOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("NodeAliasPath"), string.Empty);
                }

                if (textOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("neighborhoodName"), string.Empty);
                }

                if (linkText != "")
                {
                    return "<a href='" + node.GetValue("NodeAliasPath") + ".aspx'>" + linkText + "</a>";
                }
                else
                {
                    return "<a href='" + node.GetValue("NodeAliasPath") + ".aspx'>" + (string)node.GetValue("NeighborhoodName") + "</a>";
                }
                //return neighborhoodID.ToString();
            }

            public static string GetParentNeighborhood(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
            {
                try
                {
                    if (txtValue == null || txtValue.ToString().Trim() == "")
                        return "";

                    int homeID = Convert.ToInt32(txtValue.ToString());

                    UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                    TreeProvider tree = new TreeProvider(ui);

                    CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(homeID);
                    if (node != null)
                    {
                        return GetNeighborhood(node.Parent.NodeID, linkText, urlOnly, textOnly);
                    }
                    else
                    {
                        return "";
                    }
                }
                catch { }
                return "";
            }

            public static string GetNeighborhoodID(object txtValue)
            {
                try
                {
                    if (txtValue == null || txtValue.ToString().Trim() == "")
                        return "0";

                    int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                    UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                    TreeProvider tree = new TreeProvider(ui);

                    CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                    return ValidationHelper.GetString(node.GetValue("NeighborhoodID"), "0");
                    //return neighborhoodID.ToString();
                }
                catch { }
                return "";
            }

            public static string GetNeighborhoodPhone(object txtValue)
            {
                try
                {
                    if (txtValue == null || txtValue.ToString().Trim() == "")
                        return "";

                    int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                    UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                    TreeProvider tree = new TreeProvider(ui);

                    CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                    return ValidationHelper.GetString(node.GetValue("NeighborhoodPhoneNumber").ToString().Replace("/", "").Replace("-", "").Replace(".", ""), string.Empty);
                }
                catch { }
                return "";
            }

            public static string GetNeighborhoodObject(object txtValue)
            {
                try
                {
                    if (txtValue == null || txtValue.ToString().Trim() == "")
                        return "";

                    int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                    UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                    CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                    CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                    string objectTxt = string.Empty;

                    if (node != null)
                    {
                        objectTxt = "{";
                        objectTxt += "\"neighborhoodName\": \"" + ValidationHelper.GetString(node.GetValue("NeighborhoodName"), string.Empty) + "\",";
                        objectTxt += "\"neighborhoodAddress\": \"" + ValidationHelper.GetString(node.GetValue("NeighborhoodAddress"), string.Empty) + "\",";
                        objectTxt += "\"neighborhoodLatitude\": \"" + ValidationHelper.GetString(node.GetValue("NeighborhoodLattitude"), string.Empty) + "\",";
                        objectTxt += "\"neighborhoodLongitude\": \"" + ValidationHelper.GetString(node.GetValue("NeighborhoodLongitude"), string.Empty) + "\",";
                        objectTxt += "\"neighborhoodUrl\": \"" + ValidationHelper.GetString(node.NodeAliasPath, string.Empty) + "\",";
                        objectTxt += "\"neighborhoodIcon\": \"../BlueKey/Templates/images/mapMarker" + GetNeighborhoodIcon(ValidationHelper.GetString(node.GetValue("NeighborhoodPriceRangeLow"), string.Empty)) + "\"";
                        objectTxt += "}";
                    }

                    return objectTxt;
                }
                catch { }
                return "";
            }

            public static string GetNeighborhoodName(object txtValue)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                TreeProvider tree = new TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                return (string)node.GetValue("NeighborhoodName");
                //return neighborhoodID.ToString();
            }

            public static string GetSafeMonth(object txtValue)
            {
                string ret = string.Empty;
                try
                {
                    ret = ((DateTime)txtValue).ToString("MMM");
                }
                catch { }
                return ret;
            }

            public static string GetNeighborhoodIcon(object txtValue)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "01";

                int neighborhoodCost = 0;

                //return txtValue.ToString();
                try
                {
                    neighborhoodCost = Convert.ToInt32(txtValue.ToString().Replace(",", ""));
                }
                catch
                {
                    return "01";
                }

                if (neighborhoodCost <= 100000) return "01";
                if (neighborhoodCost > 100000 && neighborhoodCost <= 150000) return "02";
                if (neighborhoodCost > 150000 && neighborhoodCost <= 200000) return "03";
                if (neighborhoodCost > 200000 && neighborhoodCost <= 250000) return "04";
                if (neighborhoodCost > 250000 && neighborhoodCost <= 300000) return "05";
                if (neighborhoodCost > 300000 && neighborhoodCost <= 400000) return "06";
                if (neighborhoodCost > 400000 && neighborhoodCost <= 500000) return "07";
                if (neighborhoodCost > 500000 && neighborhoodCost <= 750000) return "08";
                if (neighborhoodCost > 750000) return "09";

                return "01";
            }

            public static string GetNeighborhoodIconLots(object txtValue)
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "01";

                int neighborhoodCost = 0;

                //return txtValue.ToString();
                try
                {
                    neighborhoodCost = Convert.ToInt32(txtValue.ToString().Replace(",", ""));
                }
                catch
                {
                    return "01";
                }

                if (neighborhoodCost <= 50000) return "01";
                if (neighborhoodCost > 50000 && neighborhoodCost <= 100000) return "02";
                if (neighborhoodCost > 100000 && neighborhoodCost <= 150000) return "03";
                if (neighborhoodCost > 150000 && neighborhoodCost <= 200000) return "04";
                if (neighborhoodCost > 200000) return "05";

                return "01";
            }
        }

        #endregion app_code classes

        private string _where = "1 = 1";
        public bool FilterByQuery = true;

        protected int defaultCity = 0;
        protected int defaultArea = 0;
        protected int defaultCounty = 0;
        protected int defaultPrice = 0;
        protected int defaultBuilder = 0;
        protected int defaultType = 0;
        protected int defaultLifestyle = 0;
        protected int defaultNeighborhood = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            DataSet ds = DocumentHelper.GetDocuments("custom.Neighborhood")
                                                .Path("/Neighborhoods/%")
                                                .Where("NeighborhoodActive = 1")
                                                .OnSite(SiteContext.CurrentSiteName)
                                                .Published(true)
                                                .OrderBy("NeighborhoodName ASC");

            rptNeighborhoods.DataSource = ds;
            rptNeighborhoods.DataBind();
        }

        protected string GetDocumentUrl(object documentId)
        {
            int id = ValidationHelper.GetInteger(documentId, -1);
            string val = string.Empty;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            var node = DocumentHelper.GetDocument(id, tree);

            if (node != null)
            {
                val = node.NodeAliasPath;
            }

            return val;
        }

        protected string GetPriceRange(object range, object low)
        {
            if (range != null)
            {
                return ValidationHelper.GetString(range, string.Empty);
            }
            else
            {
                return ValidationHelper.GetString(low, string.Empty) + "+";
            }
        }

        protected string GetDevelopers(object developers)
        {
            string val = string.Empty;
            if (developers != null)
            {
                val = NewHomesGuideFunctions.GetDevelopers(Eval("NeighborhoodDevelopers"), true);
            }

            return val;
        }

        #region Filter Population Methods

        protected void PopulateCounties()
        {
            ddCounty.Items.Clear();

            ddCounty.Items.Insert(0, new ListItem("By County", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyVisible = 1", "CountyName ASC");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCounty.Items.Insert(i, new ListItem(dr["CountyName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }
        }

        protected void PopulateCities()
        {
            ddCity.Items.Clear();

            ddCity.Items.Insert(0, new ListItem("By City", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }
        }

        public void ddCounty_filterSetAreas(object obj, EventArgs sender)
        {
            int county = Convert.ToInt32(ddCounty.SelectedValue);

            if (county != 0)
            {
                resetCities(county);
            }
            else
            {
                PopulateCities();
            }
        }

        protected void resetCounties(int area)
        {
            if (area != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.area", "ItemID = '" + Convert.ToString(area) + "'", "AreaName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyArea = '" + Convert.ToString(area) + "' AND CountyVisible = 1", "CountyName ASC");

                ddCounty.Items.Clear();

                ddCounty.Items.Insert(0, new ListItem("Any " + row["AreaName"] + " County", "0"));

                int i = 1;

                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    ddCounty.Items.Insert(i, new ListItem(dr["CountyName"].ToString(), dr["ItemID"].ToString()));
                    i++;
                }
            }
        }

        protected void resetCities(int county)
        {
            if (county != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", "CountyName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty in (SELECT ItemID FROM customlocations_county WHERE CountyName LIKE '" + row["CountyName"] + "') AND CityVisible = 1", "CityName ASC");

                ddCity.Items.Clear();

                ddCity.Items.Insert(0, new ListItem("Any " + row["CountyName"] + " City", "0"));

                int i = 1;

                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                    i++;
                }
            }
        }

        protected void PopulateBuilders()
        {
            ddBuilder.Items.Insert(0, new ListItem("By Builder", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Developers", "DeveloperActive = 1", "DeveloperName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddBuilder.Items.Insert(i, new ListItem(dr["DeveloperName"].ToString(), dr["DevelopersID"].ToString()));
                i++;
            }
        }

        protected void PopulateTypes()
        {
            ddType.Items.Insert(0, new ListItem("By Type", "0"));
            ddType.Items.Insert(1, new ListItem("Condos/Lofts/Townhomes", "1"));
            ddType.Items.Insert(2, new ListItem("Single-Family Homes", "2"));
            ddType.Items.Insert(3, new ListItem("Homesites", "3"));
        }

        protected void PopulateLifestyles()
        {
            ddLifestyle.Items.Insert(0, new ListItem("By Lifestyle", "0"));
            ddLifestyle.Items.Insert(1, new ListItem("55+ Community", "1"));
            ddLifestyle.Items.Insert(2, new ListItem("Golf Community", "2"));
            ddLifestyle.Items.Insert(3, new ListItem("Planned Community", "3"));
            ddLifestyle.Items.Insert(4, new ListItem("Waterfront Community", "4"));
        }

        #endregion Filter Population Methods

        #region Filter Filter Methods

        private string getLocationFilterQueryByCounty(int county)
        {
            string str_while = "";

            DataSet dsCounty = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", null);
            DataTable dtCounties = dsCounty.Tables[0];

            if (dtCounties.Rows.Count > 0)
            {
                str_while += " AND (";

                foreach (DataRow countyRow in dtCounties.Rows)
                {
                    str_while += "NeighborhoodCounty LIKE '" + countyRow["CountyName"].ToString() + "'";
                    str_while += " OR ";
                }

                str_while = str_while.Substring(0, str_while.Length - 4) + ")";
            }

            /**
            if (dtCities.Rows.Count > 0)
            {
                str_while += " AND (";

                foreach (DataRow city in dtCities.Rows)
                {
                    str_while += "NeighborhoodCity LIKE '" + city["CityName"].ToString() + "'";
                    str_while += " OR ";
                }

                str_while = str_while.Substring(0, str_while.Length - 4) + ")";
            }
            **/

            return str_while;
        }

        private string getLocationFilterQueryByCity(int cityID)
        {
            string str_while = "";

            DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "ItemID = '" + Convert.ToString(cityID) + "'", null);
            DataTable dtCities = dsCities.Tables[0];

            str_while += " AND (";

            foreach (DataRow city in dtCities.Rows)
            {
                str_while += "NeighborhoodCity LIKE '" + city["CityName"].ToString() + "'";
                str_while += " OR ";
            }

            str_while = str_while.Substring(0, str_while.Length - 4) + ")";

            return str_while;
        }

        private void ClearFilters()
        {
            string url = RequestContext.RawURL;
            string path = url.Substring(0, url.IndexOf("?"));

            Response.Redirect(path);
        }

        private void SetFilter()
        {
            string where = _where;
            string url = RequestContext.RawURL;

            // Build where condition according to dropdowns setings
            if (ddCity.SelectedValue != this.defaultCity.ToString())
            {
                where += this.getLocationFilterQueryByCity(Convert.ToInt32(ddCity.SelectedValue));
            }

            if (ddCounty.SelectedValue != this.defaultCounty.ToString())
            {
                where += this.getLocationFilterQueryByCounty(Convert.ToInt32(ddCounty.SelectedValue));
            }

            if ((QueryHelper.GetInteger("low", -1) > 0) || QueryHelper.GetInteger("high", -1) > 0)
            {
                string[] range = { ((ValidationHelper.GetInteger(hfLowValue.Value, 0) * 1000)).ToString(), (ValidationHelper.GetInteger(hfHighValue.Value, 0) * 1000).ToString() };
                where += " AND (CASE WHEN NeighborhoodPriceRangeHigh IS NULL THEN 999999999999 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeHigh, ',', ''), 's', ''), '+', '')) END >= " + Convert.ToInt32(range[0]) + " AND CASE WHEN NeighborhoodPriceRangeLow IS NULL THEN 0 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeLow, ',', ''), 's', ''), '+', '')) END <= " + Convert.ToInt32(range[1]) + ") ";
            }

            if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
            {
                where += " AND NeighborhoodDevelopers = '" + ddBuilder.SelectedValue + "' ";
            }

            if (ddType.SelectedValue != this.defaultType.ToString())
            {
                where += " AND NeighborhoodTypes = '" + ddType.SelectedValue + "' ";
            }

            if (ddLifestyle.SelectedValue != this.defaultLifestyle.ToString())
            {
                where += " AND NeighborhoodLifestyle = '" + ddLifestyle.SelectedValue + "' ";
            }

            string neighborhood = QueryHelper.GetString("neighborhood", "");

            if (neighborhood != "")
            {
                where += " AND NeighborhoodID = '" + neighborhood + "' ";
            }

            CMS.EventLog.EventLogProvider.LogInformation("BlueKey", "Debug", "Neighborhood Where: " + where);

            if (where != "")
            {
                // Set where condition
                //    this.WhereCondition = where;
                //ddDebugLabel.Text = where;
            }

            if (!RequestHelper.IsPostBack() || QueryHelper.GetString("filter", string.Empty) == "1")
            {
                var documents = DocumentHelper.GetDocuments("custom.Neighborhood")
                                           .Path("/Neighborhoods/%")
                                           .Where(where + " AND NeighborhoodActive = 1")
                                           .OnSite(SiteContext.CurrentSiteName)
                                           .Published(true)
                                           .OrderBy("NeighborhoodName ASC");
                ltlShowingCount.Text = documents.Count.ToString();

                var totalDocs = DocumentHelper.GetDocuments("custom.Neighborhood")
                                                .Path("/Neighborhoods/%")
                                                .Where("NeighborhoodActive = 1")
                                                .OnSite(SiteContext.CurrentSiteName)
                                                .Published(true)
                                                .OrderBy("NeighborhoodName ASC");
                ltlTotalCount.Text = totalDocs.Count.ToString();

                rptNeighborhoods.DataSource = documents;
            }

            // Filter changed event
            // this.RaiseOnFilterChanged();
        }

        private void GetFilter()
        {
            string low = QueryHelper.GetString("low", "");
            string high = QueryHelper.GetString("high", "");
            string county = QueryHelper.GetString("county", "");
            string city = QueryHelper.GetString("city", "");
            string builder = QueryHelper.GetString("builder", "");
            string type = QueryHelper.GetString("type", "");
            string lifestyle = QueryHelper.GetString("lifestyle", "");

            // Set order if in query
            if (county != "")
            {
                resetCities(Convert.ToInt32(county));
                ddCounty.SelectedValue = county.ToString();
            }

            if ("" == city)
            {
                try
                {
                    city = DocumentContext.CurrentDocument.GetValue("City").ToString();
                    city = ddCity.Items.FindByText(city).Value.ToString();
                }
                catch (Exception ex) { }
            }

            if (city != "")
            {
                try
                {
                    ddCity.SelectedValue = city.ToString();
                }
                catch { }
            }

            if (low != "")
            {
                try
                {
                    hfLowValue.Value = low;
                }
                catch { }
            }

            if (high != "")
            {
                try
                {
                    hfHighValue.Value = high;
                }
                catch { }
            }

            if (builder != "")
            {
                try
                {
                    ddBuilder.SelectedValue = builder.ToString();
                }
                catch { }
            }

            if (type != "")
            {
                try
                {
                    ddType.SelectedValue = type.ToString();
                }
                catch { }
            }

            if (lifestyle != "")
            {
                try
                {
                    ddLifestyle.SelectedValue = lifestyle.ToString();
                }
                catch { }
            }
        }

        public void btnFilter_Click(object sender, EventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;

                url = URLHelper.RemoveParameterFromUrl(url, "low");
                url = URLHelper.RemoveParameterFromUrl(url, "high");
                url = URLHelper.RemoveParameterFromUrl(url, "county");
                url = URLHelper.RemoveParameterFromUrl(url, "city");
                url = URLHelper.RemoveParameterFromUrl(url, "builder");
                url = URLHelper.RemoveParameterFromUrl(url, "type");
                url = URLHelper.RemoveParameterFromUrl(url, "lifestyle");
                url = URLHelper.RemoveParameterFromUrl(url, "filter");
                url = URLHelper.RemoveParameterFromUrl(url, "page");

                if (ddCounty.SelectedValue != this.defaultCounty.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "county", ddCounty.SelectedValue);
                }

                if (ddCity.SelectedValue != this.defaultCity.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "city", ddCity.SelectedValue);
                }

                if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "builder", ddBuilder.SelectedValue);
                }

                if (ddType.SelectedValue != this.defaultType.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "type", ddType.SelectedValue);
                }

                if (ddLifestyle.SelectedValue != this.defaultLifestyle.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "lifestyle", ddLifestyle.SelectedValue);
                }

                if (ValidationHelper.GetInteger(hfLowValue.Value, 0) > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "low", ValidationHelper.GetString(hfLowValue.Value, string.Empty));
                }

                if (ValidationHelper.GetInteger(hfHighValue.Value, 0) > 0 && ValidationHelper.GetInteger(hfHighValue.Value, 0) != 100000)
                {
                    url = URLHelper.AddParameterToUrl(url, "high", ValidationHelper.GetString(hfHighValue.Value, string.Empty));
                }

                url = URLHelper.AddParameterToUrl(url, "filter", "1");
                url = url + "#filter";
                // Redirect with new query parameters
                URLHelper.Redirect(url);
            }
            else
            {
                // Set filter settings
                SetFilter();
            }
        }

        #endregion Filter Filter Methods
    }
}