using CMS.Synchronization;
using CMS.SiteProvider;
using CMS.Base;
using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds staging API examples.
    /// </summary>
    /// <pageTitle>Staging</pageTitle>
    internal class Staging
    {
        /// <summary>
        /// Holds staging server API examples.
        /// </summary>
        /// <groupHeading>Staging servers</groupHeading>
        private class StagingServers
        {
            /// <heading>Creating a staging server</heading>
            private void CreateStagingServer()
            {
                // Creates a new staging server object
                ServerInfo newServer = new ServerInfo();

                // Sets the server properties
                newServer.ServerDisplayName = "New target server";
                newServer.ServerName = "NewTargetServer";
                newServer.ServerEnabled = true;
                newServer.ServerSiteID = SiteContext.CurrentSiteID;
                newServer.ServerURL = "http://localhost/Kentico/";
                newServer.ServerAuthentication = ServerAuthenticationEnum.UserName;
                newServer.ServerUsername = "admin";
                newServer.ServerPassword = "pass";

                // Saves the staging server to the database
                ServerInfoProvider.SetServerInfo(newServer);
            }


            /// <heading>Updating a staging server</heading>
            private void GetAndUpdateStagingServer()
            {
                // Gets the staging server
                ServerInfo updateServer = ServerInfoProvider.GetServerInfo("NewTargetServer", SiteContext.CurrentSiteID);
                if (updateServer != null)
                {
                    // Updates the server properties
                    updateServer.ServerDisplayName = updateServer.ServerDisplayName.ToLowerCSafe();

                    // Saves the updated server to the database
                    ServerInfoProvider.SetServerInfo(updateServer);
                }
            }


            /// <heading>Updating multiple staging servers</heading>
            private void GetAndBulkUpdateStagingServers()
            {
                // Gets all staging servers defined on the current site whose code name starts with 'New'
                var servers = ServerInfoProvider.GetServers()
                                                    .WhereStartsWith("ServerName", "New")
                                                    .WhereEquals("ServerSiteID", SiteContext.CurrentSiteID);

                // Loops through individual servers
                foreach (ServerInfo server in servers)
                {
                    // Updates the server properties
                    server.ServerDisplayName = server.ServerDisplayName.ToUpper();

                    // Saves the updated server to the database
                    ServerInfoProvider.SetServerInfo(server);
                }
            }


            /// <heading>Deleting a staging server</heading>
            private void DeleteStagingServer()
            {
                // Gets the staging server
                ServerInfo deleteServer = ServerInfoProvider.GetServerInfo("NewTargetServer", SiteContext.CurrentSiteID);

                if (deleteServer != null)
                {
                    // Deletes the staging server
                    ServerInfoProvider.DeleteServerInfo(deleteServer);
                }
            }
        }


        /// <summary>
        /// Holds staging task API examples.
        /// </summary>
        /// <groupHeading>Staging tasks</groupHeading>
        private class StagingTasks
        {
            /// <heading>Synchronizing staging tasks</heading>
            private void GetAndSynchronizeTasks()
            {
                // Gets a staging server
                ServerInfo server = ServerInfoProvider.GetServerInfo("NewTargetServer", SiteContext.CurrentSiteID);

                if (server != null)
                {
                    // Gets all staging tasks that target the given server
                    var tasks = StagingTaskInfoProvider.SelectTaskList(SiteContext.CurrentSiteID, server.ServerID, null, null);

                    // Loops through individual staging tasks
                    foreach (StagingTaskInfo task in tasks)
                    {
                        // Synchronizes the staging task
                        string result = new StagingTaskRunner(server.ServerID).RunSynchronization(task.TaskID);

                        if (string.IsNullOrEmpty(result))
                        {
                            // The task synchronization was successful
                        }
                        else 
                        {
                            // The task synchronization failed
                            // The 'result' string returned by the RunSynchronization method contains the error message for the given task
                        }
                    }          
                }
            }


            /// <heading>Deleting all staging tasks that target a server</heading>
            private void DeleteTasks()
            {
                // Gets the staging server
                ServerInfo server = ServerInfoProvider.GetServerInfo("NewTargetServer", SiteContext.CurrentSiteID);

                if (server != null)
                {
                    // Gets all staging tasks that target the given server
                    var tasks = StagingTaskInfoProvider.SelectTaskList(SiteContext.CurrentSiteID, server.ServerID, null, null);
                    
                    // Loops through individual staging tasks
                    foreach (StagingTaskInfo task in tasks)
                    {                        
                        // Deletes the staging task
                        StagingTaskInfoProvider.DeleteTaskInfo(task);
                    }
                }
            }


            /// <heading>Running code without logging staging tasks</heading>
            private void DisableTaskLogging()
            {
                // Prepares an action context for running code without logging of staging tasks
                using (new CMSActionContext() { LogSynchronization = false })
                {
                    // Creates a new role without logging any staging tasks
                    RoleInfo newRole = new RoleInfo();
                    newRole.RoleDisplayName = "New role";
                    newRole.RoleName = "NewRole";
                    newRole.SiteID = SiteContext.CurrentSiteID;
                    
                    RoleInfoProvider.SetRoleInfo(newRole);
                }
            }


            /// <heading>Logging staging tasks under specific task groups</heading>
            private void LogTasksUnderGroups()
            {
                // Gets a "collection" of task groups (in this case one group whose code name is equal to "Group_Name")
                var taskGroups = TaskGroupInfoProvider.GetTaskGroups().WhereEquals("TaskGroupCodeName", "Group_Name");

                // Prepares a synchronization action context
                // The context ensures that any staging tasks logged by the wrapped code are included in the specified task groups
                using (new SynchronizationActionContext() { TaskGroups = taskGroups })
                {
                    // Creates a new role object
                    RoleInfo newRole = new RoleInfo();

                    // Sets the role properties
                    newRole.RoleDisplayName = "New role";
                    newRole.RoleName = "NewRole";
                    newRole.SiteID = SiteContext.CurrentSiteID;

                    // Saves the role to the database
                    RoleInfoProvider.SetRoleInfo(newRole);
                }
            }
        }
    }
}
