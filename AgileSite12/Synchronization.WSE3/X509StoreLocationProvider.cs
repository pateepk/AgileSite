using System;
using System.Security.Cryptography.X509Certificates;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Synchronization.WSE3;

[assembly: RegisterImplementation(typeof(IX509StoreLocationProvider), typeof(X509StoreLocationProvider), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.Synchronization.WSE3
{
    /// <summary>
    /// Base implementation of <see cref="IX509StoreLocationProvider"/>.
    /// </summary>
    internal sealed class X509StoreLocationProvider : IX509StoreLocationProvider
    {
        private readonly bool mIsWebSiteNameEnvironmentVariableEmpty;


        /// <summary>
        /// Initializes a new instance of <see cref="X509StoreLocationProvider"/>.
        /// </summary>
        public X509StoreLocationProvider()
        {
            // more details at https://stackoverflow.com/questions/25678419/how-to-check-if-code-is-running-on-azure-websites/25695126#25695126
            mIsWebSiteNameEnvironmentVariableEmpty = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        }


        /// <summary>
        /// Returns store location used for X.509 certificate store.
        /// </summary>
        public StoreLocation GetStoreLocation()
        {
            return mIsWebSiteNameEnvironmentVariableEmpty && !SystemContext.IsRunningOnAzure ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
        }
    }
}
