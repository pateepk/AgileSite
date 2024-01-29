using System;

using CMS.Localization;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds localization API examples.
    /// </summary>
    /// <pageTitle>Localization</pageTitle>
    internal class Localization
    {
        /// <summary>
        /// Holds culture API examples.
        /// </summary>
        /// <groupHeading>Cultures</groupHeading>
        private class Cultures
        {
            /// <heading>Creating a new culture</heading>
            private void CreateCulture()
            {
                // Creates a new culture object
                CultureInfo newCulture = new CultureInfo();

                // Sets the culture properties
                newCulture.CultureName = "New culture";
                newCulture.CultureCode = "nw-cu";
                newCulture.CultureShortName = "Culture";

                // Saves the new culture to the database
                CultureInfoProvider.SetCultureInfo(newCulture);
            }


            /// <heading>Updating a culture</heading>
            private void GetAndUpdateCulture()
            {
                // Gets the culture using the culture code
                CultureInfo updateCulture = CultureInfoProvider.GetCultureInfo("nw-cu");

                if (updateCulture != null)
                {
                    // Updates the culture properties
                    updateCulture.CultureName = updateCulture.CultureName.ToLower();

                    // Saves the modified culture to the database
                    CultureInfoProvider.SetCultureInfo(updateCulture);
                }
            }


            /// <heading>Updating multiple cultures</heading>
            private void GetAndBulkUpdateCultures()
            {
                // Prepares a where condition for loading all cultures whose code starts with 'nw'
                string where = "CultureCode LIKE N'nw%'";

                // Get all cultures that fulfill the condition
                InfoDataSet<CultureInfo> cultures = CultureInfoProvider.GetCultures(where, null);
                
                // Loops through individual cultures
                foreach (CultureInfo culture in cultures)
                {
                    // Updates the culture properties
                    culture.CultureName = culture.CultureName.ToUpper();

                    // Saves the updated culture to the database
                    CultureInfoProvider.SetCultureInfo(culture);
                }
            }

            /// <heading>Assigning a culture to a site</heading>
            private void AddCultureToSite()
            {
                // Gets the culture using the culture code
                CultureInfo culture = CultureInfoProvider.GetCultureInfo("nw-cu");

                if (culture != null)
                {
                    // Assigns the culture to the current site
                    CultureSiteInfoProvider.AddCultureToSite(culture.CultureID, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a culture from a site</heading>
            private void RemoveCultureFromSite()
            {
                // Gets the culture using the culture code
                CultureInfo removeCulture = CultureInfoProvider.GetCultureInfo("nw-cu");

                if (removeCulture != null)
                {
                    // Gets the object representing the relationship between the culture and the current site
                    CultureSiteInfo cultureSite = CultureSiteInfoProvider.GetCultureSiteInfo(removeCulture.CultureID, SiteContext.CurrentSiteID);

                    if (cultureSite != null)
                    {
                        // Removes the culture from the current site
                        CultureSiteInfoProvider.DeleteCultureSiteInfo(cultureSite);
                    }
                }
            }


            /// <heading>Deleting a culture</heading>
            private void DeleteCulture()
            {
                // Gets the culture using the culture code
                CultureInfo deleteCulture = CultureInfoProvider.GetCultureInfo("nw-cu");

                if (deleteCulture != null)
                {
                    // Deletes the culture
                    CultureInfoProvider.DeleteCultureInfo(deleteCulture);
                }
            }
        }
    }
}
