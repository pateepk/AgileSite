using System;

using CMS.Relationships;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to page relationships.
    /// </summary>
    /// <pageTitle>Page relationships</pageTitle>
    internal class PageRelationshipsMain
    {
        /// <summary>
        /// Holds relationship name API examples.
        /// </summary>
        /// <groupHeading>Relationship names (types)</groupHeading>
        private class RelationshipNames
        {
            /// <heading>Creating a relationship name</heading>
            private void CreateRelationshipName()
            {
                // Creates a new relationship name object
                RelationshipNameInfo newName = new RelationshipNameInfo();

                // Sets the relationship name properties
                newName.RelationshipDisplayName = "New relationship";
                newName.RelationshipName = "NewRelationship";

                // Saves the relationship name to the database
                RelationshipNameInfoProvider.SetRelationshipNameInfo(newName);
            }


            /// <heading>Updating a relationship name</heading>
            private void GetAndUpdateRelationshipName()
            {
                // Gets the relationship name
                RelationshipNameInfo updateName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (updateName != null)
                {
                    // Updates the relationship name properties
                    updateName.RelationshipDisplayName = updateName.RelationshipDisplayName.ToLower();

                    // Saves the updated relationship name to the database
                    RelationshipNameInfoProvider.SetRelationshipNameInfo(updateName);
                }
            }


            /// <heading>Updating multiple relationship names</heading>
            private void GetAndBulkUpdateRelationshipNames()
            {
                // Gets all relationship names whose code name starts with 'New'
                var relationshipNames = RelationshipNameInfoProvider.GetRelationshipNames().WhereStartsWith("RelationshipName", "New");
                
                // Loops through individual relationship names
                foreach (RelationshipNameInfo name in relationshipNames)
                {
                    // Updates the relationship name properties
                    name.RelationshipDisplayName = name.RelationshipDisplayName.ToUpper();

                    // Saves the updated relationship name to the database
                    RelationshipNameInfoProvider.SetRelationshipNameInfo(name);
                }               
            }


            /// <heading>Assigning a relationship name to a site</heading>
            private void AddRelationshipNameToSite()
            {
                // Gets the relationship name
                RelationshipNameInfo relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (relationshipName != null)
                {
                    // Allows the relationship name to be used on the current site
                    RelationshipNameSiteInfoProvider.AddRelationshipNameToSite(relationshipName.RelationshipNameId, SiteContext.CurrentSiteID);
                }
            }


            /// <heading>Removing a relationship name from a site</heading>
            private void RemoveRelationshipNameFromSite()
            {
                // Gets the relationship name
                RelationshipNameInfo relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (relationshipName != null)
                {
                    // Gets the relationship between the relationship name and the current site
                    RelationshipNameSiteInfo nameSite =
                        RelationshipNameSiteInfoProvider.GetRelationshipNameSiteInfo(relationshipName.RelationshipNameId, SiteContext.CurrentSiteID);

                    // Removes the relationship name from the site
                    RelationshipNameSiteInfoProvider.DeleteRelationshipNameSiteInfo(nameSite);
                }
            }


            /// <heading>Deleting a relationship name</heading>
            private void DeleteRelationshipName()
            {
                // Gets the relationship name
                RelationshipNameInfo deleteName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (deleteName != null)
                {
                    // Deletes the relationship name
                    RelationshipNameInfoProvider.DeleteRelationshipName(deleteName);
                }
            }
        }


        /// <summary>
        /// Holds page relationship API examples.
        /// </summary>
        /// <groupHeading>Page relationships</groupHeading>
        private class PageRelationships
        {
            /// <heading>Creating a relationship between pages</heading>
            private void CreateRelationship()
            {
                // Gets the relationship name
                RelationshipNameInfo relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (relationshipName != null)
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the pages that will be connected through the relationship
                    TreeNode firstPage = tree.SelectNodes()
                        .Path("/FirstPage")
                        .OnCurrentSite()
                        .Culture("en-us")
                        .FirstObject;

                    TreeNode secondPage = tree.SelectNodes()
                        .Path("/SecondPage")
                        .OnCurrentSite()
                        .Culture("en-us")
                        .FirstObject;

                    // Creates the relationship between the pages
                    RelationshipInfoProvider.AddRelationship(firstPage.NodeID, secondPage.NodeID, relationshipName.RelationshipNameId);
                }
            }


            /// <heading>Removing a relationship between pages</heading>
            private void DeleteRelationship()
            {
                // Gets the relationship name
                RelationshipNameInfo relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo("NewRelationship");

                if (relationshipName != null)
                {
                    // Prepares a TreeProvider instance
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                    // Gets the pages that are connected through the relationship
                    TreeNode firstPage = tree.SelectNodes()
                        .Path("/FirstPage")
                        .OnCurrentSite()
                        .Culture("en-us")
                        .FirstObject;

                    TreeNode secondPage = tree.SelectNodes()
                        .Path("/SecondPage")
                        .OnCurrentSite()
                        .Culture("en-us")
                        .FirstObject;

                    // Removes the relationship between the pages
                    RelationshipInfoProvider.RemoveRelationship(firstPage.NodeID, secondPage.NodeID, relationshipName.RelationshipNameId);
                }
            }
        }
    }
}
