using System;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds page attachment API examples.
    /// </summary>
    /// <pageTitle>Attachments</pageTitle>
    internal class Attachments
    {
        /// <heading>Getting the attachments of a page</heading>
        private void GetAttachments()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                // Iterates over all attachments of the page
                foreach (DocumentAttachment attachment in page.AllAttachments)
                {
                    // Perform any action with the attachment object (DocumentAttachment)
                }
            }

            // To get only unsorted attachments, use the TreeNode.Attachments collection
            // To get only page field attachments, use the TreeNode.GroupedAttachments collection
        }


        /// <heading>Inserting attachments into page fields</heading>
        private void InsertFieldAttachment()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes("CMS.Article")
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                DocumentAttachment attachment = null;

                // Prepares the path of the file
                string file = System.Web.HttpContext.Current.Server.MapPath("/FileFolder/file.png");

                // Inserts the attachment into the "MenuItemTeaserImage" field and updates the page
                attachment = DocumentHelper.AddAttachment(page, "MenuItemTeaserImage", file);
                page.Update();
            }
        }


        /// <heading>Inserting grouped attachments into page fields</heading>
        private void InsertGroupedAttachment()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                DocumentAttachment attachment = null;

                // Prepares the path of the file
                string file = System.Web.HttpContext.Current.Server.MapPath("/FileFolder/file.png");

                // Generates a new GUID for the attachment
                Guid attachmentGuid = Guid.NewGuid();

                // Prepares the GUID of the grouped attachment page field (of the 'Attachments' data type)
                // To find the GUID of a field:
                // Navigate to the 'Page types' application -> Select the Page type -> 'Fields' tab -> Select the field
                Guid fieldGuid = Guid.Parse("2458d5fd-54f0-4a07-b2cf-a45c38e793db");

                // Inserts the attachment into the field specified by the GUID and updates the page
                attachment = DocumentHelper.AddGroupedAttachment(page, attachmentGuid, fieldGuid, file);
            }
        }


        /// <heading>Adding unsorted attachments</heading>
        private void InsertUnsortedAttachment()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;


            // Prepares the path of the file
            string file = System.Web.HttpContext.Current.Server.MapPath("/FileFolder/file.png");

            if (page != null)
            {
                // Adds the file as an attachment of the page
                DocumentHelper.AddUnsortedAttachment(page, Guid.NewGuid(), file, ImageHelper.AUTOSIZE, ImageHelper.AUTOSIZE, ImageHelper.AUTOSIZE);
            }
        }


        /// <heading>Changing the order of unsorted attachments</heading>
        private void MoveAttachmentUpDown()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                // Gets an attachment by file name
                DocumentAttachment attachment = DocumentHelper.GetAttachment(page, "file.png");

                // Moves the attachment down in the list
                DocumentHelper.MoveAttachmentDown(attachment.AttachmentGUID, page);

                // Moves the attachment up in the list
                DocumentHelper.MoveAttachmentUp(attachment.AttachmentGUID, page);
            }
        }


        /// <heading>Modifying attachment metadata</heading>
        private void EditMetadata()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                // Gets an attachment by file name
                DocumentAttachment attachment = DocumentHelper.GetAttachment(page, "file.png");

                // Edits the attachment's metadata (name, title and description)
                attachment.AttachmentName += " - modified";
                attachment.AttachmentTitle = "Attachment title";
                attachment.AttachmentDescription = "Attachment description.";

                // Ensures that the attachment can be updated without supplying its binary data
                attachment.AllowPartialUpdate = true;

                // Saves the modified attachment into the database
                attachment.Update();
            }
        }


        /// <heading>Deleting attachments</heading>
        private void DeleteAttachments()
        {
            // Creates a new instance of the Tree provider
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            // Gets a page
            TreeNode page = tree.SelectNodes()
                .Path("/Articles")
                .OnCurrentSite()
                .Culture("en-us")
                .FirstObject;

            if (page != null)
            {
                // Gets an attachment by file name
                DocumentAttachment attachment = DocumentHelper.GetAttachment(page, "file.png");

                // Deletes the attachment
                DocumentHelper.DeleteAttachment(page, attachment.AttachmentGUID);
            }
        }
    }
}