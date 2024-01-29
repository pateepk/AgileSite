using CMS.Helpers;
using CMS.Protection;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds Banned IP API examples.
    /// </summary>
    /// <pageTitle>Banned IPs</pageTitle>
    internal class BannedIPs
    {
        /// <heading>Creating a new banned IP</heading>
        private void CreateBannedIp()
        {
            // Creates a new banned IP object
            BannedIPInfo bannedIP = new BannedIPInfo();

            // Sets the banned IP's properties
            bannedIP.IPAddress = "0.0.0.1";
            bannedIP.IPAddressBanReason = "The IP has been banned.";
            bannedIP.IPAddressAllowed = false;
            bannedIP.IPAddressAllowOverride = false;
            bannedIP.IPAddressBanType = BanControlEnum.AllNonComplete.ToStringRepresentation();
            bannedIP.IPAddressBanEnabled = true;

            // Saves the banned IP to the database
            BannedIPInfoProvider.SetBannedIPInfo(bannedIP);
        }


        /// <heading>Updating a banned IP</heading>
        private void GetAndUpdateBannedIp()
        {
            // Gets the banned IP from the database
            BannedIPInfo bannedIP = BannedIPInfoProvider.GetBannedIPs()
                                                        .WhereEquals("IPAddess", "0.0.0.1")
                                                        .TopN(1) // Optimizes performance by limiting the amount of data transferred from the database server
                                                        .FirstObject; // Ensures conversion to the BannedIPInfo type

            // Updates the banned IP properties - bans only registrations from this IP address
            bannedIP.IPAddressBanType = BanControlEnum.Registration.ToStringRepresentation();

            // Saves the changes to the database
            BannedIPInfoProvider.SetBannedIPInfo(bannedIP);
        }


        /// <heading>Updating multiple banned IPs</heading>
        private void GetAndBulkUpdateBannedIps()
        {
            // Gets all banned IPs whose address starts with '0.0.0'
            var bannedIPs = BannedIPInfoProvider.GetBannedIPs().WhereStartsWith("BannedIP", "0.0.0.");

            // Loops through individual banned IPs
            foreach (BannedIPInfo bannedIP in bannedIPs)
            {
                // Updates the banned IP properties - disables the ban
                bannedIP.IPAddressBanEnabled = false;

                // Saves the changes to the database
                BannedIPInfoProvider.SetBannedIPInfo(bannedIP);
            }
        }


        /// <heading>Checking if a user's IP is banned</heading>
        private void CheckBannedIp()
        {
            // Gets the IP of the current user
            string checkedIP = RequestContext.UserHostAddress;

            // Checks if the IP is allowed. Please note that this method also checks the current user's IP by default if you omit the first argument.
            if (!BannedIPInfoProvider.IsAllowed(checkedIP, SiteContext.CurrentSiteName, BanControlEnum.AllNonComplete))
            {
                // Do something (IP is banned)
            }
        }


        /// <heading>Deleting a banned IP</heading>
        private void DeleteBannedIp()
        {
            // Gets the banned IP
            BannedIPInfo bannedIP = BannedIPInfoProvider.GetBannedIPs()
                                                        .WhereEquals("IPAddess", "0.0.0.1")
                                                        .TopN(1) // Optimizes performance by limiting the amount of data transferred from the database server
                                                        .FirstObject; // Ensures conversion to the BannedIPInfo type

            if (bannedIP != null)
            {
                // Deletes the banned IP
                BannedIPInfoProvider.DeleteBannedIPInfo(bannedIP);
            }
        }        
    }
}
