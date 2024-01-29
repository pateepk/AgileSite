using CMS.DataEngine;
using CMS.WorkflowEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Synchronization;
using CMS.UIControls;
using System;
using System.Linq;
using System.Globalization;

namespace NHG_C.CMSApp.utils
{
    public partial class EditListingTitle : CMSPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Server.ScriptTimeout = 600;
            var user = MembershipContext.AuthenticatedUser;
            UpdateListingMetaTitles(user);            
        }
        
        private void UpdateListingMetaTitles(UserInfo user)
        {
            // This method is being used to update the custom document type items. 
            // Creates an instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // The pages are retrieved from the Dancing Goat site and in the "en-us" culture
            DateTime dateAfter = DateTime.Now.AddDays(-2);
            var pages = DocumentHelper.GetDocuments("custom.Listing")
                //.Path("/Neighborhoods/Ashton-Woods-Homes/Darrell-Creek%", PathTypeEnum.Children)
                //.WhereLike("NodeGuid", "7cdebaf7-a7be-42fc-8834-cf88dad6c891")
            .OnSite("TheGreaterCharlestonNewHomesGuide")
            .Culture("en-us")
            .WhereGreaterThan("DocumentCreatedWhen", dateAfter);
            

            // Updates the "DocumentName" and "ArticleTitle" fields of each retrieved page
            foreach (var page in pages)
            {
                // Checks if the page isn't already checked-out
                if (!page.IsCheckedOut)
                {
                    // Checks out the page
                    page.CheckOut();
                }
				
                var rt = Convert.ToString(page.GetValue("NodeAliasPath"));
				var pg = Convert.ToString(page.GetValue("NodeAlias"));
				var res = rt.Replace("/" + pg,"");
								
				TreeNode parentPage = tree.SelectNodes()
				.Path(res)
				.OnCurrentSite()
				.Culture("en-us")
				.FirstObject;
								
                var readyText = Convert.ToString(page.GetValue("ListingReadyText"));
                var dt = Convert.ToString(page.GetValue("ListingTitle") + ", " + page.GetValue("ListingCity") + ", SC - " + parentPage.GetValue("DocumentName"));
				var par = Convert.ToString(page.GetValue("RelativeURL"));
								
				par = par.Replace(dt,"");
				dt = dt.Replace(" - Featured","");
				
				page.SetValue("DocumentPageTitle", dt);

                var DocumentPageTitle = (string)page["DocumentPageTitle"];
                Response.Write(HTMLHelper.HTMLEncode(page.DocumentPageTitle) + "::NEW UPDATED<br />!!");
                                
                page.Update(true);
                page.CheckIn();
                
                // Gets the page's workflow
                WorkflowManager workflowManager = WorkflowManager.GetInstance(tree);
                WorkflowInfo workflow = workflowManager.GetNodeWorkflow(page);

                // Checks if the page uses workflow
                if (workflow != null)
                {
                    // Publishes the page with a comment. There needs to be only one workflow path to the Published step.
                    page.Publish("Review finished, publishing page.");
                }
            }
        }
        
    }
}