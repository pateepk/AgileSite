using System;

using CMS.CMSImportExport;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <pageTitle>Import and export</pageTitle>
    internal class ImportExport
    {
        /// <summary>
        /// Holds import API examples.
        /// </summary>
        /// <groupHeading>Import</groupHeading>
        private class Import
        {
            /// <heading>Importing an object</heading>
            private void ImportObject()
            {
                // Creates an object containing the import settings
                SiteImportSettings settings = new SiteImportSettings(MembershipContext.AuthenticatedUser);

                // Initializes the import settings
                settings.SourceFilePath = "Packages\\APIExample_User.zip";
                settings.ImportType = ImportTypeEnum.AllNonConflicting;
                settings.LoadDefaultSelection();

                // Imports the package
                ImportProvider.ImportObjectsData(settings);

                // Deletes temporary data
                ImportProvider.DeleteTemporaryFiles(settings, false);
            }


            /// <heading>Importing a site</heading>
            private void ImportSite()
            {
                // Prepares the site import parameters
                string websitePath = System.Web.HttpContext.Current.Server.MapPath("~/");
                string sourceFilePath = "Packages\\APIExample_Site.zip";
                string siteDisplayName = "Imported site";
                string siteName = "ImportedSite";
                string siteDomain = "127.0.0.1";

                // Verifies that the system does not already contain a site with the given name
                if (SiteInfoProvider.GetSiteInfo(siteName) == null)
                {
                    // Imports the site
                    ImportProvider.ImportSite(siteName, siteDisplayName, siteDomain, sourceFilePath, websitePath, MembershipContext.AuthenticatedUser);
                }
            }
        }

        /// <summary>
        /// Holds export API examples.
        /// </summary>
        /// <groupHeading>Export</groupHeading>
        private class Export
        {
            /// <heading>Exporting an object (user)</heading>
            private void ExportObject()
            {
                // Deletes temporary export files
                try
                {
                    ExportProvider.DeleteTemporaryFiles();
                }
                catch
                {
                }

                // Gets the user
                UserInfo exportedUser = UserInfoProvider.GetUserInfo("Username");

                // Ensures that the user exists
                if (exportedUser != null)
                {
                    // Prepares the export parameters
                    string websitePath = System.Web.HttpContext.Current.Server.MapPath("~/");
                    string exportFileName = string.Format("APIExample_User_{0:yyyy-MM-dd_hh-mm}.zip", DateTime.Now);
                    string exportFilePath = FileHelper.GetFullFilePhysicalPath(ImportExportHelper.GetSiteUtilsFolder(), websitePath) + "Export\\" + exportFileName;

                    // Verifies that an export package with the given name doesn't already exist
                    if (!File.Exists(exportFilePath))
                    {
                        // Exports the user object
                        ExportProvider.ExportObject(exportedUser, exportFilePath, websitePath, MembershipContext.AuthenticatedUser);
                    }
                }
            }


            /// <heading>Exporting a site</heading>
            private void ExportSite()
            {
                // Deletes temporary export files
                try
                {
                    ExportProvider.DeleteTemporaryFiles();
                }
                catch
                {
                }

                // Prepares the export parameters
                string websitePath = System.Web.HttpContext.Current.Server.MapPath("~/");
                string exportFileName = string.Format("APIExample_Site_{0:yyyy-MM-dd_hh-mm}.zip", DateTime.Now);
                string exportFilePath = FileHelper.GetFullFolderPhysicalPath(ImportExportHelper.GetSiteUtilsFolder(), websitePath) + "Export\\" + exportFileName;
                string siteName = "ImportedSite";

                // Verifies that the site exists
                if (SiteInfoProvider.GetSiteInfo(siteName) != null)
                {
                    // Verifies that an export package with the given name doesn't already exist
                    if (!File.Exists(exportFilePath))
                    {
                        // Exports the site
                        ExportProvider.ExportSite(siteName, exportFilePath, websitePath, false, MembershipContext.AuthenticatedUser);
                    }
                }
            }

            /// <heading>Deleting export packages from the file system</heading>
            private void DeletePackages()
            {
                // Prepares package parameters
                string websitePath = System.Web.HttpContext.Current.Server.MapPath("~/");
                string exportPath = FileHelper.GetFullFolderPhysicalPath(ImportExportHelper.GetSiteUtilsFolder(), websitePath) + "Export\\";
                string filesToDelete = @"APIExample*.zip";

                // Gets a list of export packages
                string[] fileList = Directory.GetFiles(exportPath, filesToDelete);

                // Deletes the files
                foreach (string file in fileList)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Deletion was unsuccessful
                    }
                }
            }
        }
    }
}
