using System;
using CMS;
using CMS.Scheduler;
using CMS.EventLog;
using CMS.DataEngine;
using CMS.WorkflowEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Synchronization;
using CMS.UIControls;
using System.Linq;
using System.Globalization;


[assembly: RegisterCustomClass("Custom.EditListingTitleTask", typeof(Custom.EditListingTitleTask))]

namespace Custom
{
    public class EditListingTitleTask : ITask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="ti">Info object representing the scheduled task</param>
        public string Execute(TaskInfo ti)
        {
            string detail = "Executed from '~/App_Code/EditListingTitleTask.cs'. Task data:" + ti.TaskData;

            // Logs the execution of the task in the event log.
            EventLogProvider.LogInformation("EditListingTitleTask", "Execute", detail);

            UpdateListingMetaTitles();

            return null;
        }

        private void UpdateListingMetaTitles()
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
                var res = rt.Replace("/" + pg, "");

                TreeNode parentPage = tree.SelectNodes()
                .Path(res)
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

                var readyText = Convert.ToString(page.GetValue("ListingReadyText"));
                var docTitle = Convert.ToString(page.GetValue("ListingTitle") + ", " + page.GetValue("ListingCity") + ", SC - " + parentPage.GetValue("DocumentName"));
                var par = Convert.ToString(page.GetValue("RelativeURL"));

                par = par.Replace(docTitle, "");
                docTitle = docTitle.Replace(" - Featured", "");

                page.SetValue("DocumentPageTitle", docTitle);

                //get developer/builder name
                // The pages are retrieved from the Dancing Goat site and in the "en-us" culture

                var developers = DocumentHelper.GetDocuments("custom.Developers")
                    //.Path("/Neighborhoods/Ashton-Woods-Homes/Darrell-Creek%", PathTypeEnum.Children)
                .WhereLike("DevelopersID", Convert.ToString(page.GetValue("ListingDeveloper")))
                .OnSite("TheGreaterCharlestonNewHomesGuide")
                .Culture("en-us");

                string developerName = "";
                foreach (var developer in developers)
                {
                    developerName = Convert.ToString(developer.GetValue("DeveloperName"));
                }

                
                
                

               // var DocumentPageTitle = (string)page["DocumentPageTitle"];
                //Response.Write(HTMLHelper.HTMLEncode(page.DocumentPageTitle) + "::NEW UPDATED<br />!!");
                //[Listing Title Field] - [City] - [Neighborhood] - [Builder] - [Price] -[# of BR] BR - [# of BA] BA - [# of SF] SF - [Plan Name] 
                //Update Listing Description

                //page.SetValue("DocumentPageDescription", docTitle);
                var docDesc = docTitle;
                if (!String.IsNullOrEmpty(developerName))
                {
                    docDesc += " - " + developerName;
                }
                if (!String.IsNullOrEmpty(Convert.ToString(page.GetValue("ListingPrice"))))
                {
                    docDesc += " - " + Convert.ToString(page.GetValue("ListingPrice"));
                }
                if (!String.IsNullOrEmpty(Convert.ToString(page.GetValue("ListingBedrooms"))))
                {
                    docDesc += " - " + Convert.ToString(page.GetValue("ListingBedrooms")) + " BR";
                    docDesc += " - " + Convert.ToString(page.GetValue("ListingBathrooms")) + " BA";

                }
                if (!String.IsNullOrEmpty(Convert.ToString(page.GetValue("ListingSquareFootage"))))
                {
                    docDesc += " - " + Convert.ToString(page.GetValue("ListingSquareFootage")) + " SF";
                }
                if (!String.IsNullOrEmpty(Convert.ToString(page.GetValue("ListingPlan"))))
                {
                    docDesc += " - " + Convert.ToString(page.GetValue("ListingPlan"));
                }

                page.SetValue("DocumentPageDescription", docDesc);


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