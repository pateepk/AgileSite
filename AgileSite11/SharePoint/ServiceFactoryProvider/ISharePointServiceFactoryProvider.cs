using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.SharePoint;

[assembly: RegisterImplementation(typeof(ISharePointServiceFactoryProvider), typeof(SharePointServiceFactoryProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.SharePoint
{
    /// <summary>
    /// Describes SharePoint service factory.
    /// </summary>
    public interface ISharePointServiceFactoryProvider
    {
        /// <summary>
        /// Registers new implementation of ISharePointServiceFactory for given SharePoint version.
        /// </summary>
        /// <typeparam name="TServiceFactory">Class implementing ISharePointServiceFactory</typeparam>
        /// <param name="sharePointVersion">SharePoint version</param>
        /// <seealso cref="SharePointVersion"/>
        void RegisterFactory<TServiceFactory>(string sharePointVersion)
            where TServiceFactory : ISharePointServiceFactory, new();


        /// <summary>
        /// Gets instance of ISharePointServiceFactory.
        /// </summary>
        /// <param name="sharePointVersion">SharePoint version for which to get service factory</param>
        /// <returns>ISharePointServiceFactory instance</returns>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when no service factory for sharePointVersion found.</exception>
        /// <seealso cref="SharePointVersion"/>
        ISharePointServiceFactory GetSharePointServiceFactory(string sharePointVersion);
    }
}
