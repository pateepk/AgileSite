using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Data;

namespace NHG_T
{
    /// <summary>
    /// Summary description for NewHomesGuideFunctions
    /// </summary>
    public static class NewHomesGuideFunctions
    {
        public static string GetDeveloperProfileLink(string documentId)
        {
            string result = string.Empty;
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(documentId, -1), tree);
            if (node != null)
            {
                string profileLink = string.Empty;
                profileLink = node.GetValue("DeveloperProfileLink", string.Empty);
                if (!String.IsNullOrEmpty(profileLink))
                {
                    TreeNode profileNode = tree.SelectSingleNode(new Guid(profileLink), "en-us", SiteContext.CurrentSiteName);
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

            TreeNode node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(documentId, -1), tree);
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
                                TreeNode node = tp.SelectSingleNode(nodeId);

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

            TreeNode node = tree.SelectSingleNode(neighborhoodID);

            return node.GetValue("NodeAliasPath").ToString();
        }

        public static string GetNeighborhood(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
        {
            if (txtValue == null || txtValue.ToString().Trim() == "")
                return "";

            int neighborhoodID = Convert.ToInt32(txtValue.ToString());

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            TreeProvider tree = new TreeProvider(ui);

            TreeNode node = tree.SelectSingleNode(neighborhoodID);
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

                TreeNode node = tree.SelectSingleNode(homeID);
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

                TreeNode node = tree.SelectSingleNode(neighborhoodID);

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

                TreeNode node = tree.SelectSingleNode(neighborhoodID);

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

            TreeNode node = tree.SelectSingleNode(neighborhoodID);

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
}