using System;

using CMS.WebFarmSync;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds web farm API examples.
    /// </summary>
    /// <pageTitle>Web farms</pageTitle>
    internal class WebFarm
    {
        /// <heading>Creating a web farm server</heading>
        private void CreateWebFarmServer()
        {
            // Creates a new web farm server object
            WebFarmServerInfo newServer = new WebFarmServerInfo();

            // Sets the properties for the server
            newServer.ServerDisplayName = "New server";
            newServer.ServerName = "NewServer";
            newServer.ServerEnabled = true;

            // Saves the web farm server to the database
            WebFarmServerInfoProvider.SetWebFarmServerInfo(newServer);
        }


        /// <heading>Updating a web farm server</heading>
        private void GetAndUpdateWebFarmServer()
        {
            // Gets the web farm server
            WebFarmServerInfo updateServer = WebFarmServerInfoProvider.GetWebFarmServerInfo("NewServer");

            if (updateServer != null)
            {
                // Updates the properties of the server
                updateServer.ServerDisplayName = updateServer.ServerDisplayName.ToLowerCSafe();

                // Saves the changed server to the database
                WebFarmServerInfoProvider.SetWebFarmServerInfo(updateServer);
            }
        }


        /// <heading>Updating multiple web farm servers</heading>
        private void GetAndBulkUpdateWebFarmServers()
        {
            // Gets all enabled web farm servers
            var servers = WebFarmServerInfoProvider.GetWebFarmServers().WhereTrue("ServerEnabled");

            // Loops through individual servers
            foreach (WebFarmServerInfo server in servers)
            {
                // Disables the server
                server.ServerEnabled = false;

                // Saves the changed server to the database
                WebFarmServerInfoProvider.SetWebFarmServerInfo(server);
            }
        }


        /// <heading>Deleting a web farm server</heading>
        private void DeleteWebFarmServer()
        {
            // Gets the web farm server
            WebFarmServerInfo deleteServer = WebFarmServerInfoProvider.GetWebFarmServerInfo("NewServer");

            if (deleteServer != null)
            {
                // Deletes the web farm server
                WebFarmServerInfoProvider.DeleteWebFarmServerInfo(deleteServer);
            }
        }


        /// <heading>Creating a web farm synchronization task</heading>
        private void CreateTask()
        {
            // Sets the properties for the task
            string taskTarget = "";
            string taskTextData = "WebFarmTask";

            // Creates the web farm task
            WebFarmHelper.CreateTask(DataTaskType.ClearHashtables, taskTarget, taskTextData);
        }
    }
}
