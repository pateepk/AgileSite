using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;


using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;

using CMS.Activities.Loggers;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.Protection;
using CMS.SiteProvider;
using CMS.WebAnalytics;

using System.Globalization;


using CMS.WorkflowEngine;
using CMS.Synchronization;
using CMS.UIControls;

namespace NHG_T.CMSApp.utils
{
    public partial class EditVersionHistory : CMSPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var user = MembershipContext.AuthenticatedUser;

            //if (!user.IsGlobalAdministrator)
            //{
            //    // This page should only be accessible to global admins
            //    RequestHelper.Respond404();
            //    return;
            //}

            var user = MembershipContext.AuthenticatedUser;

            if (!user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin, SiteContext.CurrentSiteName))
            {
                // This page should only be accessible to global admins
                RequestHelper.Respond404();
                return;
            }
        }

        private void DeleteDocumentHistory(UserInfo user)
        {
            // This method is being used to update the custom document type items. 
            // Creates an instance of the Tree provider
			TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

			// Gets the published version of pages stored under the "/New-Homes-in-Myrtle-Beach/Bill-Clark-Homes/Arrowhead-Grand/" path
			// The pages are retrieved from the Dancing Goat site and in the "en-us" culture
			var pages = DocumentHelper.GetDocuments("custom.Listing")
		//	.Path("/New-Homes-in-Myrtle-Beach/Bill-Clark-Homes/Arrowhead-Grand/%", PathTypeEnum.Children)
			.WhereLike("DocumentName", "%")
			.OnSite("TheGreaterCharlestonNewHomesGuide")
			.Culture("en-us");

				Response.Write("hello");

				
				
			// Updates the "DocumentName" and "ArticleTitle" fields of each retrieved page
				foreach (var page in pages)
					{
						//page.DocumentName = "Updated article name";
						var amt = Convert.ToString(page.GetValue("ListingPrice"));
						decimal d = decimal.Parse(amt, NumberStyles.Currency);
						Response.Write(page.GetValue("ListingPrice") + "~~~~");
						page.SetValue("ListingPriceValue", d);
						//var documentName = (string)page["DocumentName"];
						Response.Write(HTMLHelper.HTMLEncode(page.DocumentName) + "<br />!!");
						// Updates the page in the database
					    page.Update();
					}
        }

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


        private void UpdateListingDates(UserInfo user)
        {
            //update date field from text to date
            // This method is being used to update the custom document type items. 
            // Creates an instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets the published version of pages stored under the "/New-Homes-in-Myrtle-Beach/Bill-Clark-Homes/Arrowhead-Grand/" path
            // The pages are retrieved from the Dancing Goat site and in the "en-us" culture
            var pages = DocumentHelper.GetDocuments("custom.Listing")
                //	.Path("/New-Homes-in-Myrtle-Beach/Bill-Clark-Homes/Arrowhead-Grand/%", PathTypeEnum.Children)
            .WhereLike("DocumentName", "%")
            .OnSite("TheGreaterCharlestonNewHomesGuide")
            .Culture("en-us");

            //Response.Write("hello");



            // Updates the "DocumentName" and "ArticleTitle" fields of each retrieved page
            foreach (var page in pages)
            {
                //page.DocumentName = "Updated article name";
                var readyText = Convert.ToString(page.GetValue("ListingReadyText"));
                DateTime dt = convertToDate(readyText);

                Response.Write(page.GetValue("ListingReadyText") + "~~~~");
                page.SetValue("ListingReadyText", dt);
                //var documentName = (string)page["DocumentName"];
                Response.Write(HTMLHelper.HTMLEncode(page.DocumentName) + "<br />!!");
                // Updates the page in the database
                page.Update();
            }
        }

        private void DeleteObjectHistory(UserInfo user)
        {
            // Get ids and types of all objects that have version information
            
        }



        protected void DeleteObjectHistory_Click(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            DeleteObjectHistory(user);
        }

        protected void DeleteDocumentHistory_Click(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            DeleteDocumentHistory(user);
        }

        protected void UpdateListingDates_Click(object sender, EventArgs e)
        {
            var user = MembershipContext.AuthenticatedUser;
            UpdateListingDates(user);
        }
    }
}