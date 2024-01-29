using System;
using System.Collections;
using System.Web;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Flood protection class.
    /// </summary>
    public static class FloodProtectionHelper
    {
        #region "Variables"

        // Try to get maximum capacity from web.config, else use default value = 1000
        private static int mMaxPoolCapacity = -1;

        // Initialize IP pool hashtable with maximum capacity
        private static readonly Hashtable ipPool = new Hashtable();

        // Initialize user pool hashtable with maximum capacity
        private static readonly Hashtable userPool = new Hashtable();

        // Indicates whether flood protection should be based on user account
        private static bool? mUserBasedProtection;

        #endregion


        #region "Properties"

        /// <summary>
        /// Maximum pool capacity
        /// </summary>
        private static int MaxPoolCapacity
        {
            get
            {
                if (mMaxPoolCapacity < 0)
                {
                    mMaxPoolCapacity = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSMaxFloodItems"], 1000);
                }

                return mMaxPoolCapacity;
            }
        }


        /// <summary>
        /// Gets or sets the value that indictaes whether for authenticated user is action's
        /// flood interval checked with dependence on current user or IP address
        /// </summary>
        public static bool UserBasedProtection
        {
            get
            {
                if (mUserBasedProtection == null)
                {
                    mUserBasedProtection = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUserBasedFloodProtection"], true);
                }

                return mUserBasedProtection.Value;
            }
            set
            {
                mUserBasedProtection = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the current IP address or user floods the system.
        /// </summary>
        /// <param name="sitename">Site name</param> 
        /// <param name="ui">User info object</param>
        public static bool CheckFlooding(string sitename, IUserInfo ui)
        {
            // Check if flood protection is enabled for the site
            if (!String.IsNullOrEmpty(sitename) && SettingsKeyInfoProvider.GetBoolValue(sitename + ".CMSFloodProtectionEnabled"))
            {
                if (HttpContext.Current != null)
                {
                    int floodInterval = SettingsKeyInfoProvider.GetIntValue(sitename + ".CMSFloodInterval");

                    if (UserBasedProtection && (ui != null) && (!ui.IsPublic()))
                    {
                        DateTime lastCheck = ValidationHelper.GetDateTime(userPool[ui.UserID], DateTime.MinValue);
                        if (lastCheck > DateTime.MinValue)
                        {
                            // Interval is smaller than flooding interval, consider as flooding
                            if (lastCheck.AddSeconds(floodInterval) > DateTime.Now)
                            {
                                return true;
                            }
                        }

                        // Update ip's last check time
                        userPool[ui.UserID] = DateTime.Now;

                        // If IPs pool count reaches maximum capacity, clear it
                        if (userPool.Count >= MaxPoolCapacity)
                        {
                            userPool.Clear();
                        }
                    }
                    else
                    {
                        DateTime lastCheck = ValidationHelper.GetDateTime(ipPool[RequestContext.UserHostAddress], DateTime.MinValue);
                        if (lastCheck > DateTime.MinValue)
                        {
                            // Interval is smaller than flooding interval, consider as flooding
                            if (lastCheck.AddSeconds(floodInterval) > DateTime.Now)
                            {
                                return true;
                            }
                        }

                        // Update ip's last check time
                        ipPool[RequestContext.UserHostAddress] = DateTime.Now;

                        // If IPs pool count reaches maximum capacity, clear it
                        if (ipPool.Count >= MaxPoolCapacity)
                        {
                            ipPool.Clear();
                        }
                    }
                }
            }

            // Is not flooding
            return false;
        }

        #endregion
    }
}