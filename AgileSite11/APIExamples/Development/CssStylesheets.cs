using System;

using CMS.PortalEngine;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds CSS stylesheet API examples.
    /// </summary>
    /// <pageTitle>CSS stylesheets</pageTitle>
    internal class CssStylesheets
    {
        /// <heading>Creating a new CSS stylesheet</heading>
        private void CreateCssStylesheet()
        {
            // Creates a new CSS stylesheet object
            CssStylesheetInfo newStylesheet = new CssStylesheetInfo();

            // Sets the stylesheet properties
            newStylesheet.StylesheetDisplayName = "New stylesheet";
            newStylesheet.StylesheetName = "NewStylesheet";
            newStylesheet.StylesheetText = "CSS code";

            // Saves the CSS stylesheet to the database
            CssStylesheetInfoProvider.SetCssStylesheetInfo(newStylesheet);
        }


        /// <heading>Updating a CSS stylesheet</heading>
        private void GetAndUpdateCssStylesheet()
        {
            // Gets the CSS stylesheet
            CssStylesheetInfo updateStylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("NewStylesheet");
            if (updateStylesheet != null)
            {
                // Updates the stylesheet properties
                updateStylesheet.StylesheetDisplayName = updateStylesheet.StylesheetDisplayName.ToLower();

                // Saves the changes to the database
                CssStylesheetInfoProvider.SetCssStylesheetInfo(updateStylesheet);
            }
        }


        /// <heading>Updating multiple CSS stylesheets</heading>
        private void GetAndBulkUpdateCssStylesheets()
        {            
            // Gets all CSS stylesheets whose name starts with 'NewStylesheet'            
            var stylesheets = CssStylesheetInfoProvider.GetCssStylesheets().WhereStartsWith("StylesheetName", "NewStylesheet");
            
            // Loops through individual stylesheets
            foreach (CssStylesheetInfo stylesheet in stylesheets)
            {
                // Updates the stylesheet properties
                stylesheet.StylesheetDisplayName = stylesheet.StylesheetDisplayName.ToUpper();

                // Saves the changes to the database
                CssStylesheetInfoProvider.SetCssStylesheetInfo(stylesheet);
            }            
        }


        /// <heading>Assigning a CSS stylesheet to a site</heading>
        private void AddCssStylesheetToSite()
        {
            // Gets the CSS stylesheet
            CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("NewStylesheet");
            if (stylesheet != null)
            {                
                // Assigns the stylesheet to the current site
                CssStylesheetSiteInfoProvider.AddCssStylesheetToSite(stylesheet.StylesheetID, SiteContext.CurrentSiteID);
            }
        }


        /// <heading>Removing a CSS stylesheet from a site</heading>
        private void RemoveCssStylesheetFromSite()
        {
            // Gets the CSS stylesheet
            CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("NewStylesheet");
            if (stylesheet != null)
            {
                // Gets the binding object representing the relationship between the stylesheet and the current site
                CssStylesheetSiteInfo stylesheetSite = CssStylesheetSiteInfoProvider.GetCssStylesheetSiteInfo(stylesheet.StylesheetID, SiteContext.CurrentSiteID);

                // Removes the stylesheet from the current site
                CssStylesheetSiteInfoProvider.DeleteCssStylesheetSiteInfo(stylesheetSite);
            }
        }


        /// <heading>Deleting a CSS stylesheet</heading>
        private void DeleteCssStylesheet()
        {
            // Gets the CSS stylesheet
            CssStylesheetInfo deleteStylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("NewStylesheet");

            // Deletes the CSS stylesheet
            CssStylesheetInfoProvider.DeleteCssStylesheetInfo(deleteStylesheet);
        }
    }
}
