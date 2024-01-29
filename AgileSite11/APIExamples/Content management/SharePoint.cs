using System;
using System.Data;
using System.IO;

using CMS.SharePoint;
using CMS.SiteProvider;
using CMS.Base;
using CMS.EventLog;

namespace APIExamples
{
    /// <summary>
    /// Holds SharePoint API examples.
    /// </summary>
    /// <pageTitle>SharePoint</pageTitle>
    internal class SharePoint
    {
        /// <heading>Creating a SharePoint connection</heading>
        private void CreateSharePointConnection()
        {
            // Specifies the parameters for the SharePoint connection
            // The siteUrl variable represents the URL of your configured SharePoint site
            string siteUrl = "https://mycompany.sharepoint.com";
            string userName = "yourSharePointUsername";
            string password = "yourSharePointPassword";

            // Creates a new SharePoint connection object
            SharePointConnectionInfo newConnection = new SharePointConnectionInfo();

            // Sets the properties of the connection
            newConnection.SharePointConnectionSiteUrl = siteUrl;
            newConnection.SharePointConnectionSiteID = SiteContext.CurrentSiteID;
            newConnection.SharePointConnectionDisplayName = "New connection";
            newConnection.SharePointConnectionName = "NewConnection";
            newConnection.SharePointConnectionSharePointVersion = SharePointVersion.SHAREPOINT_ONLINE;
            newConnection.SharePointConnectionAuthMode = SharePointAuthMode.DEFAULT;
            newConnection.SharePointConnectionUserName = userName;
            newConnection.SharePointConnectionPassword = password;

            // Saves the SharePoint connection to the database
            SharePointConnectionInfoProvider.SetSharePointConnectionInfo(newConnection);
        }


        /// <heading>Updating a SharePoint connection</heading>
        private void GetAndUpdateSharePointConnection()
        {
            // Gets the SharePoint connection
            SharePointConnectionInfo connection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo("NewConnection", SiteContext.CurrentSiteID);

            // Updates the properties of the connection
            connection.SharePointConnectionDisplayName = connection.SharePointConnectionDisplayName.ToLowerCSafe();

            // Saves the updated connection to the database
            SharePointConnectionInfoProvider.SetSharePointConnectionInfo(connection);
        }


        /// <heading>Deleting a SharePoint connection</heading>
        private void DeleteSharePointConnection()
        {
            // Gets the SharePoint connection
            SharePointConnectionInfo connection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo("NewConnection", SiteContext.CurrentSiteID);

            if (connection != null)
            {
                // Deletes the SharePoint connection
                SharePointConnectionInfoProvider.DeleteSharePointConnectionInfo(connection);
            }
        }


        /// <heading>Retrieving SharePoint lists</heading>
        private void GetAllLists()
        {
            // Gets the SharePoint connection
            SharePointConnectionInfo connection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo("NewConnection", SiteContext.CurrentSiteID);

            // Converts the SharePointConnectionInfo into a connection data object
            SharePointConnectionData connectionData = connection.ToSharePointConnectionData();

            // Gets the list service implementation
            ISharePointListService listService = SharePointServices.GetService<ISharePointListService>(connectionData);

            // Chooses the SharePoint list type that will be retrieved
            // You can use an enumeration or template identifier (listed in http://msdn.microsoft.com/en-us/library/microsoft.sharepoint.splisttemplatetype.aspx)
            int listType = SharePointListType.ALL;

            try
            {
                // Gets all lists of the specified type (all list types are retrieved in this case)
                DataSet results = listService.GetLists(listType);

                if ((results.Tables.Count == 0) || (results.Tables[0].Rows.Count == 0))
                {
                    // No lists were retrieved from the SharePoint server
                }
            }
            catch (Exception ex)
            {
                // The retrieval of the lists ended with an exception
                // Logs the exception to the Kentico event log
                EventLogProvider.LogException("SharePoint API Example", "EXCEPTION", ex);
            }
        }


        /// <heading>Retrieving SharePoint list items</heading>
        private void GetListItems()
        {
            // Specifies the list name
            string listName = "SharePointList";

            // Gets the SharePoint connection
            SharePointConnectionInfo connection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo("NewConnection", SiteContext.CurrentSiteID);

            // Converts the SharePointConnectionInfo into a connection data object
            SharePointConnectionData connectionData = connection.ToSharePointConnectionData();

            // Gets the list service implementation
            ISharePointListService listService = SharePointServices.GetService<ISharePointListService>(connectionData);

            try
            {
                // Gets the specified list's items
                DataSet results = listService.GetListItems(listName);

                if ((results.Tables.Count == 0) || (results.Tables[0].Rows.Count == 0))
                {
                    // No list items were retrieved from the SharePoint server 
                }
            }
            catch (Exception ex)
            {
                // The retrieval of the list items ended with an exception
                // Logs the exception to the Kentico event log
                EventLogProvider.LogException("SharePoint API Example", "EXCEPTION", ex);
            }
        }


        /// <heading>Retrieving a file from a SharePoint server</heading>
        private void GetFile()
        {
            // Specifies the relative path of the file
            string filePath = "/Picture library/picture.jpg";

            // Gets the SharePoint connection
            SharePointConnectionInfo connection = SharePointConnectionInfoProvider.GetSharePointConnectionInfo("NewConnection", SiteContext.CurrentSiteID);

            // Converts the SharePointConnectionInfo into a connection data object
            SharePointConnectionData connectionData = connection.ToSharePointConnectionData();

            // Gets the file service implementation
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionData);

            try
            {
                // Gets the file object
                ISharePointFile file = fileService.GetFile(filePath);

                // Gets the file's metadata
                string extension = file.Extension;

                // Gets a stream of the file's binary content
                Stream fileContentStream = file.GetContentStream();

                // Gets a byte array of the file's binary content
                byte[] fileContentBytes = file.GetContentBytes();
            }
            catch (Exception ex)
            {
                // The retrieval of the file ended with an exception
                // Logs the exception to the Kentico event log
                EventLogProvider.LogException("SharePoint API Example", "EXCEPTION", ex);
            }
        }
    }
}
