using System;

using CMS.PortalEngine;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds page layout API examples.
    /// </summary>
    /// <pageTitle>Page layouts</pageTitle>
    internal class PageLayouts
    {
        /// <heading>Setting a custom layout for a page template</heading>
        private void SetCustomLayout()
        {
            // Gets the page template
            PageTemplateInfo pageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");
            if (pageTemplate != null)
            {
                // Sets the template's layout type to ASCX
                pageTemplate.PageTemplateLayoutType = LayoutTypeEnum.Ascx;

                // Sets the custom layout code of the template
                pageTemplate.PageTemplateLayout = "<cms:CMSWebPartZone ZoneID=\"zoneExample\" runat=\"server\" />";

                // Saves the template changes to the database
                PageTemplateInfoProvider.SetPageTemplateInfo(pageTemplate);
            }
        }


        /// <heading>Creating a shared page layout</heading>
        private void CreateLayout()
        {
            // Creates a new layout object
            LayoutInfo newLayout = new LayoutInfo();

            // Sets the layout properties
            newLayout.LayoutDisplayName = "New layout";
            newLayout.LayoutCodeName = "NewLayout";
            newLayout.LayoutDescription = "This is a layout created by an API example";
            newLayout.LayoutType = LayoutTypeEnum.Ascx;
            newLayout.LayoutCode = "<cms:CMSWebPartZone ZoneID=\"zoneA\" runat=\"server\" />";

            // Saves the layout to the database
            LayoutInfoProvider.SetLayoutInfo(newLayout);
        }


        /// <heading>Updating a shared page layout</heading>
        private void GetAndUpdateLayout()
        {
            // Gets the layout
            LayoutInfo updateLayout = LayoutInfoProvider.GetLayoutInfo("NewLayout");
            if (updateLayout != null)
            {
                // Adds another web part zone into the layout code
                updateLayout.LayoutCode += "<cms:CMSWebPartZone ZoneID=\"zoneB\" runat=\"server\" />";

                // Saves the changes to the database
                LayoutInfoProvider.SetLayoutInfo(updateLayout);
            }
        }

        
        /// <heading>Updating multiple shared page layouts</heading>
        private void GetAndBulkUpdateLayouts()
        {                        
            // Gets all shared page layouts whose code name starts with 'NewLayout'
            var layouts = LayoutInfoProvider.GetLayouts().WhereStartsWith("LayoutCodeName", "NewLayout");
            
            // Loops through individual layouts
            foreach (LayoutInfo layout in layouts)
            {                
                // Updates the layout properties
                layout.LayoutDisplayName = layout.LayoutDisplayName.ToUpper();

                // Saves the changes to the database
                LayoutInfoProvider.SetLayoutInfo(layout);
            }
        }


        /// <heading>Assigning a shared page layout to a page template</heading>
        private void AssignSharedLayoutToTemplate()
        {
            // Gets the page template
            PageTemplateInfo pageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo("NewTemplate");

            // Gets the layout
            LayoutInfo sharedLayout = LayoutInfoProvider.GetLayoutInfo("NewLayout");

            if ((pageTemplate != null) && (sharedLayout != null))
            {
                // Assigns the page layout to the template
                pageTemplate.LayoutID = sharedLayout.LayoutId;                

                // Saves the template changes to the database
                PageTemplateInfoProvider.SetPageTemplateInfo(pageTemplate);
            }
        }


        /// <heading>Deleting a shared page layout</heading>
        private void DeleteLayout()
        {
            // Gets the layout
            LayoutInfo deleteLayout = LayoutInfoProvider.GetLayoutInfo("NewLayout");

            if (deleteLayout != null)
            {
                // Deletes the layout
                LayoutInfoProvider.DeleteLayoutInfo(deleteLayout);
            }
        }
    }
}
