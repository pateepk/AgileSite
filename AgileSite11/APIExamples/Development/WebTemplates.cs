using System;

using CMS.CMSImportExport;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds web template API examples.
    /// </summary>
    /// <pageTitle>Web templates</pageTitle>
    internal class WebTemplates
    {
        /// <heading>Creating a web template</heading>
        private void CreateWebTemplate()
        {
            // Creates a new web template object
            WebTemplateInfo newTemplate = new WebTemplateInfo();

            // Sets the web template properties
            newTemplate.WebTemplateDisplayName = "New site template";
            newTemplate.WebTemplateName = "NewSiteTemplate";
            newTemplate.WebTemplateDescription = "This is web template created through the Kentico API.";
            newTemplate.WebTemplateFileName = "~\\App_Data\\Templates\\NewSiteTemplate";
            newTemplate.WebTemplateLicenses = "X";

            // Sets the web template order (places the new web template at the end of the list of all templates)
            InfoDataSet<WebTemplateInfo> webTemplates = WebTemplateInfoProvider.GetWebTemplates(null, null, 0, "WebTemplateID", false);
            newTemplate.WebTemplateOrder = webTemplates.Items.Count + 1;            

            // Saves the web template to the database
            WebTemplateInfoProvider.SetWebTemplateInfo(newTemplate);
        }


        /// <heading>Updating a web template</heading>
        private void GetAndUpdateWebTemplate()
        {
            // Gets the web template
            WebTemplateInfo updateTemplate = WebTemplateInfoProvider.GetWebTemplateInfo("NewSiteTemplate");
            if (updateTemplate != null)
            {
                // Updates the web template properties
                updateTemplate.WebTemplateDisplayName = updateTemplate.WebTemplateDisplayName.ToLower();

                // Moves the template down in the list of all templates
                WebTemplateInfoProvider.MoveTemplateDown(updateTemplate.WebTemplateId);

                // Moves the template up in the list of all templates
                WebTemplateInfoProvider.MoveTemplateUp(updateTemplate.WebTemplateId);

                // Saves the template changes to the database
                WebTemplateInfoProvider.SetWebTemplateInfo(updateTemplate);
            }
        }


        /// <heading>Updating multiple web templates</heading>
        private void GetAndBulkUpdateWebTemplates()
        {
            // Prepares a where condition for loading all web templates whose name starts with 'New'
            string where = "WebTemplateName LIKE N'New%'";

            // Gets all web templates that fulfill the condition
            InfoDataSet<WebTemplateInfo> templates = WebTemplateInfoProvider.GetWebTemplates(where, null);
            
            // Loops through individual web templates
            foreach (WebTemplateInfo template in templates)
            {
                // Updates the template properties
                template.WebTemplateDisplayName = template.WebTemplateDisplayName.ToUpper();

                // Saves the changes to the database
                WebTemplateInfoProvider.SetWebTemplateInfo(template);
            }
        }


        /// <heading>Deleting a web template</heading>
        private void DeleteWebTemplate()
        {
            // Gets the web template
            WebTemplateInfo deleteTemplate = WebTemplateInfoProvider.GetWebTemplateInfo("NewSiteTemplate");

            if (deleteTemplate != null)
            {
                // Deletes the web template
                WebTemplateInfoProvider.DeleteWebTemplateInfo(deleteTemplate);
            }
        }
    }
}
