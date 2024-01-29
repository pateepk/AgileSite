using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace NHG_C
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

        public static string GetDeveloperInformationLink(object neighborhoodId)
        {
            string defaultString = "<a href=\"\"><img itemprop='image' src='/BlueKey/Templates/images/sampleBuilderLogo.png' alt=''></a>";
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
                    //DataSet ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "NeighborhoodID = " + ValidationHelper.GetInteger(dev, -1) + " AND NeighborhoodActive = 1", "NeighborhoodName ASC");
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
                                img = "<img itemprop='image' src=\"" + imgDoc.NodeAliasPath + "\">";
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


        //INPUT: 
        //Master-Bedroom_St.-Martin_Hunter-Quinn-Homes
        //Darrell-Creek_Ashton-Woods_Exterior
        //Neighborhood_Builder_Photo-Name

        //OUTPUT: 
        //Master Bedroom of the St. Martin at The Preserve at Poplar Grove
        //Exterior of the Ashton Woods at Darrell Creek
        //Photo Name of the Builder at Ashton Woods

        public static string GetImageAltTagBuilder(object imgNameText)
        {
            string defaultString = "";
            string returnString = string.Empty;

            if (imgNameText != null)
            {
                string fullName = imgNameText.ToString().Replace("-", " ");
                string[] parts = fullName.Split('_');

                if (parts.Length == 3)
                {
                    string partOne = parts[2];
                    string partTwo = parts[1];
                    string partThree = parts[0];

                    returnString = string.Format("{0} of the {1} at {2}", partOne, partTwo, partThree);
                }
                else
                {
                    returnString = imgNameText.ToString().Replace("-", " ").Replace("_", " ");
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


        public static string GetImageAltTag(object imgNameText)
        {
            string defaultString = "";
            string returnString = string.Empty;

            if (imgNameText != null)
            {
                string fullName = imgNameText.ToString().Replace("-", " ");
                string[] parts = fullName.Split('_');

                if (parts.Length == 3)
                {
                    string partOne = parts[2];
                    string partTwo = parts[0];
                    string partThree = parts[1];

                    returnString = string.Format("{0} at {1} by {2}", partOne, partTwo, partThree);
                }
                else
                {
                    returnString = imgNameText.ToString().Replace("-", " ").Replace("_", " ");
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



        public static string GetDeveloperURL(object neighborhoodId, string linkText)
        {
            string defaultString = "<a href=\"\"></a>";
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
                    DataSet developerDs = tree.SelectNodes()
                        .OnCurrentSite()
                        .Type("custom.Developers", q => q.Columns("DeveloperPhoto", "DeveloperProfileLink", "DeveloperName"))
                        .Where("DevelopersID = " + dev);

                    if (!DataHelper.DataSourceIsEmpty(developerDs))
                    {
                        DataRow row = developerDs.Tables[0].Rows[0];
                        returnString = GetDevelopers(dev, true, linkText);
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


        public static string GetDeveloperImageForIntro(object neighborhoodId)
        {
            string defaultString = "<a href=\"\"><img itemprop='image' src='/BlueKey/Templates/images/sampleBuilderLogo.png' alt=''></a>";
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
                    //DataSet ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "NeighborhoodID = " + ValidationHelper.GetInteger(dev, -1) + " AND NeighborhoodActive = 1", "NeighborhoodName ASC");
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
                                img = "<img itemprop='image' src=\"" + imgDoc.NodeAliasPath + "\">";
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

        public static string GetDeveloperImageForIntro2(object neighborhoodId)
        {
            string defaultString = "<a href=\"\"><img itemprop='image' src='/BlueKey/Templates/images/sampleBuilderLogo.png' alt=''></a>";
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
                    //DataSet ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "NeighborhoodID = " + ValidationHelper.GetInteger(dev, -1) + " AND NeighborhoodActive = 1", "NeighborhoodName ASC");
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
                                img = "<img itemprop='image' src=\"" + imgDoc.NodeAliasPath + "\">";
                            }
                        }

                        returnString += GetDevelopers(dev, true, img + ValidationHelper.GetString("Builder Information", string.Empty));
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
        public static string GetDeveloperImageForIntro3(object neighborhoodId)
        {
            string defaultString = "<a href=\"\"><img itemprop='image' src='/BlueKey/Templates/images/sampleBuilderLogo.png' alt=''></a>";
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
                    //DataSet ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "NeighborhoodID = " + ValidationHelper.GetInteger(dev, -1) + " AND NeighborhoodActive = 1", "NeighborhoodName ASC");
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
                                img = "<img itemprop='image' src=\"" + imgDoc.NodeAliasPath + "\">";
                            }
                        }

                        returnString += GetDevelopers(dev, false, ValidationHelper.GetString(row["DeveloperName"], string.Empty));
                        //returnString +=  row["DeveloperName"];
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


        public static string GetDeveloperLogo(object neighborhoodId)
        {
            string defaultString = "/BlueKey/Templates/images/sampleBuilderLogo.png";
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
                                img = imgDoc.NodeAliasPath;
                            }
                        }

                        returnString = img;
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
                            int nodeId = CMS.DocumentEngine.TreePathUtils.GetNodeIdByNodeGUID(nodeGUID, SiteContext.CurrentSiteName);

                            //if (nodeId != null)
                            {
                                CMS.DocumentEngine.TreeProvider tp = new CMS.DocumentEngine.TreeProvider(MembershipContext.AuthenticatedUser);
                                CMS.DocumentEngine.TreeNode node = tp.SelectSingleNode(nodeId);
                                return TreePathUtils.GetDocumentUrl(node.DocumentID).Replace("~/", "/");
                                //return CMS.CMSHelper.CMSContext.GetUrl(node.NodeAliasPath, node.DocumentUrlPath, CMSContext.CurrentSiteName);
                            }
                        }
                    }
                }
                return "null";
            }
            catch
            {
                return "catch";
            }
        }


        public static string GetDevelopers(object txtValue, bool addUrl, string linkText = "")
        {
            return GetDevelopers(txtValue, addUrl, false, linkText);
        }

        public static string GetDevelopers(object txtValue, bool addUrl, bool targetBlank, string linkText = "", bool urlOnly = false)
        {
            try
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                string returnText = "";
                string txt = txtValue.ToString();
                int i = 0;

                string[] developers = txt.Split('|');

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                foreach (string developer in developers)
                {

                    DataSet ds = null;

                    ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Developers", "DevelopersID = " + developer);

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
                                        returnText += "<a href='" + dr["NodeAliasPath"] + "'" + target + ">" + linkText + "</a>";
                                    }
                                    else
                                    {
                                        returnText += "<a href='" + dr["NodeAliasPath"] + "'" + target + ">" + dr["DeveloperName"] + "</a>";
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
            catch { }
            return "";
        }

        public static string GetNeighborhood(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
        {
            try
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                if (urlOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("NodeAliasPath"), string.Empty);
                }

                if (textOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("NeighborhoodName"), string.Empty);
                }

                if (linkText != "")
                {
                    return "<a href='" + node.GetValue("NodeAliasPath") + "'>" + linkText + "</a>";
                }
                else
                {
                    return "<a href='" + node.GetValue("NodeAliasPath") + "'>" + (string)node.GetValue("NeighborhoodName") + "</a>";
                }
                //return neighborhoodID.ToString();
            }
            catch { }
            return "";
        }

        public static string GetParentNeighborhood(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
        {
            try
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int homeID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

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
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                return ValidationHelper.GetString(node.GetValue("NeighborhoodID"), "");
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
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

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

                ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Developers", "DevelopersID = " + d);

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
    }

    public partial class BlueKey_CMSWebParts_BKTrending : CMSAbstractWebPart
    {
        private int DEFAULT_NUM_OF_DOCS = 2;
        private int _numOfDocs;

        protected void Page_Load(object sender, EventArgs e)
        {
            string docType = ValidationHelper.GetString(this.GetValue("DocumentType"), string.Empty);
            _numOfDocs = ValidationHelper.GetInteger(this.GetValue("NumberOfDocuments"), DEFAULT_NUM_OF_DOCS);

            switch (docType)
            {
                case "Neighborhoods":
                    LoadNeighborhoods();
                    break;
                case "Builders":
                    LoadBuilders();
                    break;
                case "Homes":
                    LoadHomes();
                    break;
            }
        }

        private void LoadNeighborhoods()
        {
            DataSet ds = DocumentHelper.GetDocuments("custom.Neighborhood").Where("NeighborhoodIsFeatured = 1 AND NeighborhoodActive = 1").OrderBy("NewID()").TopN(_numOfDocs);


            rptNeighborhoods.DataSource = ds;
            rptNeighborhoods.DataBind();

            pnlNeighborhoods.Visible = rptNeighborhoods.Items.Count > 0 ? true : false;
            pnlBuilders.Visible = false;
            pnlHomes.Visible = false;
        }

        private void LoadBuilders()
        {
            DataSet ds = DocumentHelper.GetDocuments("custom.Developers").Where("DeveloperIsFeatured = 1 AND DeveloperActive = 1").OrderBy("NewID()").TopN(_numOfDocs);
            rptDevelopers.DataSource = ds;
            rptDevelopers.DataBind();

            pnlBuilders.Visible = rptDevelopers.Items.Count > 0 ? true : false;
            pnlNeighborhoods.Visible = false;
            pnlHomes.Visible = false;
        }

        private void LoadHomes()
        {
            DataSet ds = DocumentHelper.GetDocuments("custom.Listing").Where("ListingIsFeatured = 1").OrderBy("NewID()").TopN(_numOfDocs);
            rptHomes.DataSource = ds;
            rptHomes.DataBind();

            pnlHomes.Visible = rptHomes.Items.Count > 0 ? true : false;
            pnlNeighborhoods.Visible = false;
            pnlBuilders.Visible = false;
        }

        protected string GetDeveloper(object id, bool addUrl = false, bool targetBlank = false, string text = "", bool urlOnly = false)
        {
            string developer = string.Empty;
            if (id != null)
            {
                if (urlOnly)
                {
                    return NewHomesGuideFunctions.GetDevelopers(ValidationHelper.GetString(id, string.Empty), addUrl, targetBlank, text, true);
                }
                if (addUrl)
                {
                    if (text == "")
                    {
                        return NewHomesGuideFunctions.GetDevelopers(ValidationHelper.GetString(id, string.Empty), addUrl, targetBlank);
                    }
                    else
                    {
                        return NewHomesGuideFunctions.GetDevelopers(ValidationHelper.GetString(id, string.Empty), addUrl, targetBlank, text);
                    }
                }
                else
                {
                    return NewHomesGuideFunctions.GetDevelopers(ValidationHelper.GetString(id, string.Empty), addUrl, targetBlank);
                }
            }

            return developer;
        }

        protected string GetNeighborhood(object docNodeId, string linkText = "", bool linkOnly = false, bool textOnly = false)
        {
            string url = RequestContext.RawURL;
            if (docNodeId != null)
            {
                url = GetNeighborhoodUpdate(ValidationHelper.GetString(docNodeId, string.Empty), linkText, linkOnly, textOnly);
            }

            return url;
        }

        public static string GetNeighborhoodUpdate(object txtValue, string linkText = "", bool urlOnly = false, bool textOnly = false)
        {
            try
            {
                if (txtValue == null || txtValue.ToString().Trim() == "")
                    return "";

                int neighborhoodID = Convert.ToInt32(txtValue.ToString());

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(neighborhoodID);

                if (urlOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("DocumentUrlPath"), string.Empty);
                }

                if (textOnly)
                {
                    return ValidationHelper.GetString(node.GetValue("NeighborhoodName"), string.Empty);
                }

                if (linkText != "")
                {
                    return "<a href='" + node.GetValue("DocumentUrlPath") + "'>" + linkText + "</a>";
                }
                else
                {
                    return "<a href='" + node.GetValue("DocumentUrlPath") + "'>" + (string)node.GetValue("NeighborhoodName") + "</a>";
                }
                //return neighborhoodID.ToString();
            }
            catch { }
            return "";
        }



        protected string GetParentNeighborhood(object docNodeId, string linkText, bool urlOnly = false, bool textOnly = false)
        {
            string url = RequestContext.RawURL;
            if (docNodeId != null)
            {
                url = NewHomesGuideFunctions.GetParentNeighborhood(docNodeId, linkText, urlOnly, textOnly);
            }

            return url;
        }

        protected string GetThumb(object id)
        {
            string url = string.Empty;
            var guidString = ValidationHelper.GetString(id, String.Empty);
            string defaultImg = "/BlueKey/Templates/images/imageNotAvailable.jpg";
            if (String.IsNullOrEmpty(guidString)) return defaultImg;

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);
            var node = tree.SelectSingleNode(new Guid(guidString), "en-us", SiteContext.CurrentSiteName);

            if (node != null)
            {
                url = node.NodeAliasPath;
            }


            return url;
        }

        protected string GetListing(object documentNodeId)
        {
            string url = RequestContext.RawURL;
            if (documentNodeId != null)
            {
                int docNodeId = ValidationHelper.GetInteger(documentNodeId, -1);

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                CMS.DocumentEngine.TreeNode node = tree.SelectSingleNode(docNodeId, "en-us");

                if (node != null)
                {
                    url = node.Parent.NodeAliasPath;
                }
            }

            return url;
        }
    }
}