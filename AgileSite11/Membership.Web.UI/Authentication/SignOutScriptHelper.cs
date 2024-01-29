using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Helpers;
using CMS.Base.Web.UI;

namespace CMS.Membership.Web.UI
{
    /// <summary>
    /// Contains helper methods for sign out scripts
    /// </summary>
    public class SignOutScriptHelper
    {
        private static List<ICustomSignOutScriptProvider> mSignOutScriptProviders = new List<ICustomSignOutScriptProvider>();
        private static readonly object lockObject = new object();


        /// <summary>
        /// Registers a custom SignOut script provider.
        /// </summary>
        /// <param name="provider">The provider to be registered</param>
        public static void RegisterCustomSignOutScriptProvider(ICustomSignOutScriptProvider provider)
        {
            lock (lockObject)
            {
                // Copy the collection
                var newCollection = new List<ICustomSignOutScriptProvider>(mSignOutScriptProviders);

                // Add the provider
                newCollection.Add(provider);

                // Make the new collection accessible atomically (ConcurrentBag is avoided because every GetEnumerator call copies the whole collection)
                mSignOutScriptProviders = newCollection;
            }
        }


        /// <summary>
        /// Gets the script that performs a SignOut of all the registered third party providers.
        /// </summary>
        /// <param name="page">Page to which helper scripts can be registered.</param>
        /// <returns>Script that performs a SignOut of all the registered third party providers or null if none is necessary.</returns>
        public static string GetSignOutOnClickScript(Page page)
        {
            if (mSignOutScriptProviders.Count > 0)
            {
                string currentUrlSignOut = URLHelper.GetAbsoluteUrl("~/CMSModules/Membership/CMSPages/SignOut.ashx");
                currentUrlSignOut = URLHelper.AppendQuery(currentUrlSignOut, QueryHelper.BuildQueryWithHash("signout", "1"));
                string callbackScript = "window.location.href=" + ScriptHelper.GetString(currentUrlSignOut) + "; return false;";

                // Indicates whether any of the registered providers actually registered a script
                bool aScriptHasBeenRegistered = false;

                foreach (var provider in mSignOutScriptProviders)
                {
                    string newScript = provider.GetSignOutScript(String.Format("new Function({0})", ScriptHelper.GetString(callbackScript)), page);
                    if (!String.IsNullOrEmpty(newScript))
                    {
                        // A custom script has been registered
                        aScriptHasBeenRegistered = true;
                        callbackScript = newScript;
                    }
                }

                return aScriptHasBeenRegistered ? callbackScript + "return false;" : null;
            }

            return null;
        }
    }
}
