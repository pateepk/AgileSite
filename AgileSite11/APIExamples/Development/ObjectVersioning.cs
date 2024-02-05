using System;

using CMS.Synchronization;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Base;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to object versions.
    /// </summary>
    /// <pageTitle>Object versioning and recycle bin</pageTitle>
    internal class ObjectVersioning
    {
        /// <summary>
        /// Holds versioning API examples.
        /// </summary>
        /// <groupHeading>Versioning</groupHeading>
        private class Versioning
        {
            /// <heading>Creating a new version of an object</heading>
            private void CreateVersion()
            {
                // Gets an object (CSS stylesheet in this case)
                CssStylesheetInfo newStylesheetVersion = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");
                if (newStylesheetVersion != null)
                {
                    // Checks if object versioning is allowed for stylesheet objects on the current site
                    if (ObjectVersionManager.AllowObjectVersioning(newStylesheetVersion))
                    {
                        // Sets the properties for the stylesheet version
                        newStylesheetVersion.StylesheetDisplayName = newStylesheetVersion.StylesheetDisplayName.ToLowerCSafe();

                        // Adds the version to the history of the stylesheet (does not affect the current version)
                        ObjectVersionManager.CreateVersion(newStylesheetVersion, CMSActionContext.CurrentUser.UserID, true);
                    }
                }
            }


            /// <heading>Rolling an object back to a previous version</heading>
            private void RollbackVersion()
            {
                // Gets an object (CSS stylesheet in this case)
                CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");
                if (stylesheet != null)
                {
                    // Gets version "1.1" of the given stylesheet
                    ObjectVersionHistoryInfo version = ObjectVersionHistoryInfoProvider.GetVersionHistories()
                                                                                            .WhereEquals("VersionObjectID", stylesheet.StylesheetID)                                                                                            
                                                                                            .WhereEquals("VersionObjectType", stylesheet.TypeInfo.ObjectType)
                                                                                            .WhereEquals("VersionNumber", "1.1")
                                                                                            .FirstObject;
                    
                    if (version != null)
                    {
                        // Rolls the stylesheet back to version 1.1
                        ObjectVersionManager.RollbackVersion(version.VersionID);
                    }
                }
            }


            /// <heading>Permanently deleting an object version</heading>
            private void DestroyVersion()
            {
                // Gets an object (CSS stylesheet in this case)
                CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");
                if (stylesheet != null)
                {
                    // Gets the latest version of the object
                    ObjectVersionHistoryInfo version = ObjectVersionManager.GetLatestVersion(stylesheet.TypeInfo.ObjectType, stylesheet.StylesheetID);

                    if (version != null)
                    {
                        // Permanently deletes the latest version of the object
                        ObjectVersionManager.DestroyObjectVersion(version.VersionID);
                    }
                }
            }


            /// <heading>Clearing the version history of an object</heading>
            private void DestroyHistory()
            {
                // Gets an object (CSS stylesheet in this case)
                CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");
                if (stylesheet != null)
                {
                    // Clears the version history for the specified object
                    ObjectVersionManager.DestroyObjectHistory(stylesheet.TypeInfo.ObjectType, stylesheet.StylesheetID);
                }
            }
        }


        /// <summary>
        /// Holds recycle bin API examples.
        /// </summary>
        /// <groupHeading>Recycle bin</groupHeading>
        private class RecycleBin
        {
            /// <heading>Deleting an object to the recycle bin</heading>
            private void DeleteObject()
            {
                // Gets the object (CSS stylesheet in this case)
                CssStylesheetInfo deleteStylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");

                if (deleteStylesheet != null)
                {
                    // Checks if the recycle bin is enabled for objects of the given type on the current site
                    if (ObjectVersionManager.AllowObjectRestore(deleteStylesheet))
                    {
                        // Deletes the object (to the recycle bin)
                        CssStylesheetInfoProvider.DeleteCssStylesheetInfo(deleteStylesheet);
                    }
                }
            }


            /// <heading>Permanently deleting (destroying) an object</heading>
            private void DestroyObject()
            {
                // Gets the object (CSS stylesheet in this case)
                CssStylesheetInfo stylesheet = CssStylesheetInfoProvider.GetCssStylesheetInfo("Stylesheet");
                if (stylesheet != null)
                {
                    // Prepares an action context for permanently deleting objects
                    using (CMSActionContext context = new CMSActionContext())
                    {
                        // Disables creation of object versions for all code wrapped within the context (including recycle bin versions)
                        context.CreateVersion = false;

                        // Permanently deletes (destroys) the object, without creating any records in the recycle bin
                        CssStylesheetInfoProvider.DeleteCssStylesheetInfo(stylesheet);
                    }
                }
            }


            /// <heading>Restoring objects from the recycle bin</heading>
            private void RestoreObject()
            {
                // Gets all CSS stylesheets from the recycle bin that were deleted by the current user
                var deletedStylesheetVersions = ObjectVersionHistoryInfoProvider.GetVersionHistories()
                                                        .WhereEquals("VersionObjectType", CssStylesheetInfo.OBJECT_TYPE)
                                                        .WhereEquals("VersionDeletedByUserID", MembershipContext.AuthenticatedUser.UserID)
                                                        .OrderBy("VersionDeletedWhen DESC");

                // Loops through individual deleted stylesheet objects
                foreach (ObjectVersionHistoryInfo stylesheetVersion in deletedStylesheetVersions)
                {
                    // Restores the object from the recycle bin, including any child objects
                    ObjectVersionManager.RestoreObject(stylesheetVersion.VersionID, true);
                }
            }
        }
    }
}
