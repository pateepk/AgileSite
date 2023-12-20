using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.EventLog;
using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents a two-dimensional jagged lookup table to retrieve server IDs in a round robin fashion.
    /// </summary>
    /// <remarks>
    /// To ensure that this lookup table is thread-safe, its public interface methods were wrapped
    /// internally in critical sections.
    /// </remarks>
    internal sealed class SMTPServerLookupTable
    {
        #region "Constants"

        /// <summary>
        /// Constant used as a placeholder name that represents all sites.
        /// </summary>
        /// <remarks>
        /// Global objects must have site ID 0 and/or site name equal to empty string.
        /// </remarks>
        private const string GLOBAL = "global";

        #endregion


        #region "Variables"

        // Singleton instance of the SMTP server lookup table.
        private static SMTPServerLookupTable lookupTable;

        // Lock over initialization of the singleton that guarantees that only one instance is created.
        private static readonly object instanceLock = new object();

        // Collection of all available SMTP server, which uses server ID as the key.
        private Dictionary<int, SMTPServerTokenData> servers;

        // Collection of server chains (list of server IDs) for individual sites. Uses site name as the key.
        private Dictionary<string, int[]> serverChains;

        // Lock over the lookup operations that ensures that SMTP servers are manipulated sequentially.
        private static readonly object lookupLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the singleton instance of the SMTP server lookup table.
        /// </summary>
        internal static SMTPServerLookupTable Instance
        {
            get
            {
                if (lookupTable == null)
                {
                    lock (instanceLock)
                    {
                        if (lookupTable == null)
                        {
                            lookupTable = new SMTPServerLookupTable();
                        }
                    }
                }

                return lookupTable;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new instance of the SMTP server lookup table.
        /// </summary>
        private SMTPServerLookupTable()
        {
            Init();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Acquires the next available server for the given site if not busy.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        /// <returns>Information about the server and its availability</returns>
        internal SMTPServerLookupResult AcquireNext(string siteName)
        {
            SMTPServerTokenData smtpServer;
            SMTPServerAvailabilityEnum availability;

            try
            {
                lock (lookupLock)
                {
                    availability = Next(siteName, out smtpServer);

                    if (availability == SMTPServerAvailabilityEnum.Available)
                    {
                        Acquire(smtpServer.ServerID);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("EmailEngine", "SMTPServerLookupTable", ex);

                smtpServer = null;
                availability = SMTPServerAvailabilityEnum.TemporarilyUnavailable;
            }

            return new SMTPServerLookupResult(availability, smtpServer);
        }


        /// <summary>
        /// Returns a next available SMTP server for a given site using a simple round robin.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="smtpServer">If an SMTP server is available, this will contain the reference to the server</param>
        /// <returns>Next available SMTP server for a site.</returns>
        private SMTPServerAvailabilityEnum Next(string siteName, out SMTPServerTokenData smtpServer)
        {
            // If no site name is specified, work with default and global only
            if (string.IsNullOrEmpty(siteName))
            {
                siteName = GLOBAL;
            }

            // If global server chain has not been added yet, add it now to make sure the global default already exists
            if (!serverChains.ContainsKey(GLOBAL))
            {
                AddChain(GLOBAL);
            }

            // When chain is not found, get all available SMTP servers for the site from DB
            if (!serverChains.ContainsKey(siteName))
            {
                AddChain(siteName);
            }

            var chain = serverChains[siteName];

            // Get SMTP server availability and if available return next available server
            if ((chain == null) || (chain.Length == 0))
            {
                smtpServer = null;
                return SMTPServerAvailabilityEnum.PermanentlyUnavailable;
            }

            // SMTP servers are returned on first available basis (no load balancing/round robin)
            foreach (var id in chain)
            {
                var server = servers[id];
                if ((server != null) && (server.ServerStatus == SMTPServerStatusEnum.Idle))
                {
                    smtpServer = server;
                    return SMTPServerAvailabilityEnum.Available;
                }
            }

            smtpServer = null;
            return SMTPServerAvailabilityEnum.TemporarilyUnavailable;
        }


        /// <summary>
        /// This method will mark the server as being in use. It provides exclusive access and
        /// should be used before sending any messages via the specified server. When the sending
        /// is finished, the server should be released immediately afterwards.
        /// </summary>
        /// <param name="serverId">SMTP server ID</param>
        private void Acquire(int serverId)
        {
            SMTPServerTokenData server;
            if (servers.TryGetValue(serverId, out server))
            {
                server.ServerStatus = SMTPServerStatusEnum.Busy;
            }
        }


        /// <summary>
        /// This method will mark the server as available to any requests to send message.
        /// It should be called immediately after the sending is complete.
        /// </summary>
        /// <param name="smtpServer">SMTP server</param>
        internal void Release(SMTPServerTokenData smtpServer)
        {
            lock (lookupLock)
            {
                SMTPServerTokenData server;
                if (servers.TryGetValue(smtpServer.ServerID, out server))
                {
                    server.ServerStatus = SMTPServerStatusEnum.Idle;
                }
            }
        }


        /// <summary>
        /// This method will mark all servers as available to any requests to send message.
        /// </summary>
        internal void ReleaseAll()
        {
            lock (lookupLock)
            {
                // Ensure all servers are in Idle state
                foreach (var server in servers.Values)
                {
                    server.ServerStatus = SMTPServerStatusEnum.Idle;
                }
            }
        }


        /// <summary>
        /// Flushes the lookup table and creates a new one.
        /// </summary>
        internal void Flush()
        {
            lock (lookupLock)
            {
                // Get current server states
                var states = servers.Values.ToDictionary(server => server.ServerID, server => server.ServerStatus);

                Init();

                // Re-set server states (only servers based on SMTPServerInfo objects are present at this moment)
                foreach (var server in servers.Values)
                {
                    if (states.ContainsKey(server.ServerID))
                    {
                        server.ServerStatus = states[server.ServerID];
                    }
                }
            }
        }


        /// <summary>
        /// Initializes this instance of the lookup table.
        /// </summary>
        private void Init()
        {
            // Clear list of all SMTP servers and reload it from the DB
            LoadServers();

            // Tear down old server status lookup table and instantiate a new one
            serverChains = new Dictionary<string, int[]>(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Loads all available SMTP servers from the database.
        /// </summary>
        private void LoadServers()
        {
            var serverSet = SMTPServerInfoProvider.GetSMTPServers();

            if (!DataHelper.DataSourceIsEmpty(serverSet))
            {
                servers = (from row in serverSet.Tables[0].AsEnumerable()
                           select new SMTPServerTokenData(row))
                    .ToDictionary(server => server.ServerID);
            }
            else
            {
                servers = new Dictionary<int, SMTPServerTokenData>();
            }
        }


        /// <summary>
        /// Adds a chain of available servers for a given site to the lookup table.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private void AddChain(string siteName)
        {
            bool isGlobal = String.Equals(siteName, GLOBAL, StringComparison.OrdinalIgnoreCase);

            // Get site ID by site name or 0 for GLOBAL
            int siteId = (isGlobal) ? 0 : EmailHelper.GetSiteId(siteName);

            List<int> serverIDs = new List<int>();

            // Add default server for site if there's one (use negative site ID as the key)
            AddDefaultServer(siteName, siteId, serverIDs);

            // Add site servers (for sites only)
            if (!isGlobal)
            {
                serverIDs.AddRange(SMTPServerInfoProvider.GetSiteSMTPServerIDs(siteId));
            }

            // Add global servers
            serverIDs.AddRange(SMTPServerInfoProvider.GetGlobalSMTPServerIDs());

            // Create and add a new server chain (using server priorities)
            serverChains.Add(siteName, SortByPriority(serverIDs));
        }


        /// <summary>
        /// Adds a default SMTP server for the given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="serverIDs">List of server IDs that will be used to create a server chain</param>
        private void AddDefaultServer(string siteName, int siteId, List<int> serverIDs)
        {
            string site = !String.Equals(siteName, GLOBAL, StringComparison.OrdinalIgnoreCase) ? siteName : String.Empty;

            // Check if site defines SMTP server in the settings
            if (!String.IsNullOrEmpty(EmailHelper.Settings.ServerName(site)))
            {
                int smtpServerId = siteId * -1;

                servers.Add(smtpServerId, GetDefaultSMTPServer(smtpServerId, site));
                serverIDs.Add(smtpServerId);
            }
        }


        /// <summary>
        /// Sorts the list of server IDs by priority.
        /// </summary>
        /// <param name="serverIDs">The IDs of SMTP servers available for the given site</param>
        /// <returns>Array of server IDs sorted by priority</returns>
        private int[] SortByPriority(List<int> serverIDs)
        {
            List<int> serverIDsByPriority = new List<int>();

            int[] priorities = (int[])Enum.GetValues(typeof(SMTPServerPriorityEnum));
            Array.Sort(priorities);
            Array.Reverse(priorities);

            // Iterate over individual priority groups and for every server in that group add it
            foreach (int priority in priorities)
            {
                foreach (int ID in serverIDs)
                {
                    var server = servers[ID];
                    if ((server != null) && (server.ServerPriority == (SMTPServerPriorityEnum)priority))
                    {
                        serverIDsByPriority.Add(ID);
                    }
                }
            }

            return serverIDsByPriority.ToArray();
        }


        /// <summary>
        /// Wraps settings for a default SMTP server in a dynamic SMTP server container.
        /// </summary>
        /// <param name="smtpServerId">ID to assign to the created SMTP server</param>
        /// <param name="siteName">Site name</param>
        /// <returns>SMTP server container for the default server</returns>
        private static SMTPServerTokenData GetDefaultSMTPServer(int smtpServerId, string siteName)
        {
            return new SMTPServerTokenData
            {
                ServerID = smtpServerId,
                ServerName = EmailHelper.Settings.ServerName(siteName),
                ServerUserName = EmailHelper.Settings.ServerUserName(siteName),
                ServerPassword = EmailHelper.Settings.ServerPassword(siteName, true),
                ServerUseSSL = EmailHelper.Settings.ServerUseSSL(siteName),
                ServerPriority = SMTPServerPriorityEnum.High,
                ServerEnabled = true
            };
        }


        /// <summary>
        /// Returns number of SMTP servers for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        internal int GetSMTPServerCount(string siteName)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                siteName = GLOBAL;
            }

            var chain = serverChains[siteName];

            return (chain != null) ? chain.Length : 0;
        }

        #endregion
    }
}