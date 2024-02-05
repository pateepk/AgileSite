using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using System;
using System.Web.UI;

namespace CMSApp.TrainingNetwork
{
    public partial class EditVersionHistory : Page
    {
        #region "Page Events"

        protected void Page_Load(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;

            if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin, SiteContext.CurrentSiteName))
            {
                // This page should only be accessible to global admins
                RequestHelper.Respond404();
                return;
            }
        }

        #endregion "Page Events"

        #region "general events"

        protected void btnUpdateTNNVideoData_Clicked(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            UpdateTNNVideoData(user);
        }

        protected void btnClearTNNVideoVersionHistory_Clicked(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            ClearTNNVideoVersionHistory(user);
        }

        protected void btnUpdateTNNVideoPages_Clicked(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            UpdateTNNVideoPages(user);
        }

        #endregion "general events"

        #region "methods"

        #region "update page data"

        /// <summary>
        /// Update Video data
        /// </summary>
        /// <param name="user"></param>
        private void UpdateTNNVideoPages(UserInfo user)
        {
            // reference: https://docs.kentico.com/api11/content-management/pages#Pages-Pagemanagement

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            string pagesList = String.Empty;

            // Get all Training network video "pages"
            var pages = DocumentHelper.GetDocuments()
                .WhereLike("DocumentName", "%")
                .OnSite("TrainingNetworkPortal")
                .Published(true)
                .Type("custom.TNVideoProd")
                .Culture("en-us");

            foreach (var page in pages)
            {
                string sku = Convert.ToString(page.GetValue("TNVideoProdSKU"));
                page.SetValue("TNVideoProdSKU", sku);
                object a = page.GetValue("TNVideoProdProducerGUID");
                page.SetValue("TNVideoProdProducerGUID", a);
                string pageString = "Update (only) " + HTMLHelper.HTMLEncode(page.DocumentName) + "&nbsp;" + sku + "!!<br />";
                page.Update(false);
                pagesList += pageString;
            }

            Response.Write(pagesList);
        }

        /// <summary>
        /// Clear Video Pages version history
        /// </summary>
        /// <param name="user"></param>
        private void ClearTNNVideoVersionHistory(UserInfo user)
        {
            // reference: https://docs.kentico.com/api11/content-management/pages#Pages-Pagemanagement

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Get all Training network video "pages"
            var pages = DocumentHelper.GetDocuments()
                .WhereLike("DocumentName", "%")
                .OnSite("TrainingNetworkPortal")
                .Published(true)
                .Type("custom.TNVideoProd")
                .Culture("en-us");

            string pagesList = String.Empty;
            foreach (var page in pages)
            {
                page.VersionManager.ClearDocumentHistory(page.DocumentID);
                page.VersionManager.DestroyDocumentHistory(page.DocumentID);
                page.VersionManager.EnsureVersion(page, true);
                string sku = Convert.ToString(page.GetValue("TNVideoProdSKU"));
                page.SetValue("TNVideoProdSKU", sku);
                object a = page.GetValue("TNVideoProdProducerGUID");
                page.SetValue("TNVideoProdProducerGUID", a);
                string pageString = "Deleting History " + HTMLHelper.HTMLEncode(page.DocumentName) + "&nbsp;" + sku + "!!<br />";
                pagesList += pageString;
            }
            Response.Write(pagesList);
        }

        /// <summary>
        /// Update video data
        /// </summary>
        /// <param name="user"></param>
        private void UpdateTNNVideoData(UserInfo user)
        {
            // reference: https://docs.kentico.com/api11/content-management/pages#Pages-Pagemanagement

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Get all Training network video "pages"
            var pages = DocumentHelper.GetDocuments()
                .WhereLike("DocumentName", "%")
                .OnSite("TrainingNetworkPortal")
                .Published(true)
                .Type("custom.TNVideoProd")
                .Culture("en-us");

            string pagesList = String.Empty;
            foreach (var page in pages)
            {
                page.VersionManager.ClearDocumentHistory(page.DocumentID);
                page.VersionManager.DestroyDocumentHistory(page.DocumentID);
                page.VersionManager.EnsureVersion(page, true);

                string sku = Convert.ToString(page.GetValue("TNVideoProdSKU"));
                page.SetValue("TNVideoProdSKU", sku);
                object a = page.GetValue("TNVideoProdProducerGUID");
                page.SetValue("TNVideoProdProducerGUID", a);
                string pageString = "Updating (with clear history) " + HTMLHelper.HTMLEncode(page.DocumentName) + "&nbsp;" + sku + "!!<br />";
                page.Update(false);
                pagesList += pageString;
            }

            Response.Write(pagesList);
        }

        /// <summary>
        /// Update video data
        /// </summary>
        /// <param name="user"></param>
        private void UpdateTNNVideoData_old(UserInfo user)
        {
            // reference: https://docs.kentico.com/api11/content-management/pages#Pages-Pagemanagement

            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Get all Training network video "pages"
            var pages = DocumentHelper.GetDocuments()
                .WhereLike("DocumentName", "%")
                .OnSite("TrainingNetworkPortal")
                .Published(true)
                .Type("custom.TNVideoProd")
                .Culture("en-us");

            string pagesList = String.Empty;
            foreach (var page in pages)
            {
                page.VersionManager.ClearDocumentHistory(page.DocumentID);
                page.VersionManager.DestroyDocumentHistory(page.DocumentID);
                page.VersionManager.EnsureVersion(page, true);

                //if (page.IsCheckedOut)
                //{
                //    page.CheckIn();
                //}
                //page.CheckOut();
                string sku = Convert.ToString(page.GetValue("TNVideoProdSKU"));
                page.SetValue("TNVideoProdSKU", sku);
                //page.Archive();
                //page.Publish();
                object a = page.GetValue("TNVideoProdProducerGUID");
                page.SetValue("TNVideoProdProducerGUID", a);
                string pageString = "Deleting History " + HTMLHelper.HTMLEncode(page.DocumentName) + "&nbsp;" + sku + "!!<br />";
                //Response.Write(pageString);
                //page.Update(false);
                //page.CheckIn();
                //page.MoveToPublishedStep();
                //page.VersionManager.SaveVersion(page);
                pagesList += pageString;
            }

            // Get all Training network video "pages"
            pages = DocumentHelper.GetDocuments()
                .WhereLike("DocumentName", "%")
                .OnSite("TrainingNetworkPortal")
                .Published(true)
                .Type("custom.TNVideoProd")
                .Culture("en-us");

            //string pagesList = String.Empty;
            foreach (var page in pages)
            {
                //page.VersionManager.ClearDocumentHistory(page.DocumentID);
                //page.VersionManager.DestroyDocumentHistory(page.DocumentID);
                //page.VersionManager.EnsureVersion(page, true);

                //if (page.IsCheckedOut)
                //{
                //    page.CheckIn();
                //}
                //page.CheckOut();
                string sku = Convert.ToString(page.GetValue("TNVideoProdSKU"));
                page.SetValue("TNVideoProdSKU", sku);
                //page.Archive();
                //page.Publish();
                object a = page.GetValue("TNVideoProdProducerGUID");
                page.SetValue("TNVideoProdProducerGUID", a);
                string pageString = "Updating " + HTMLHelper.HTMLEncode(page.DocumentName) + "&nbsp;" + sku + "!!<br />";
                //Response.Write(pageString);
                page.Update(false);
                //page.CheckIn();
                //page.MoveToPublishedStep();
                //page.VersionManager.SaveVersion(page);
                pagesList += pageString;
            }

            Response.Write(pagesList);
        }

        #endregion "update page data"

        #region "junk"

        /// <summary>
        /// Convert string to date
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private DateTime convertToDate(string txt)
        {
            switch (txt)
            {
                case "Jan":
                    {
                        return Convert.ToDateTime("1/1/2018");
                    }
                case "Feb":
                    {
                        return Convert.ToDateTime("2/1/2018");
                    }
                case "Mar":
                    {
                        return Convert.ToDateTime("3/1/2018");
                    }
                case "Apr":
                    {
                        return Convert.ToDateTime("4/1/2018");
                    }
                case "May":
                    {
                        return Convert.ToDateTime("5/1/2018");
                    }
                case "Jun":
                    {
                        return Convert.ToDateTime("6/1/2018");
                    }
                case "Jul":
                    {
                        return Convert.ToDateTime("7/1/2018");
                    }
                case "Aug":
                    {
                        return Convert.ToDateTime("8/1/2018");
                    }
                case "Sep":
                    {
                        return Convert.ToDateTime("9/1/2018");
                    }
                case "Oct":
                    {
                        return Convert.ToDateTime("10/1/2017");
                    }
                case "Nov":
                    {
                        return Convert.ToDateTime("11/1/2017");
                    }
                case "Dec":
                    {
                        return Convert.ToDateTime("12/1/2017");
                    }
                default:
                    {
                        return Convert.ToDateTime("1/1/2000");
                    }
            }
        }

        #endregion "junk"

        #endregion "methods"
    }
}