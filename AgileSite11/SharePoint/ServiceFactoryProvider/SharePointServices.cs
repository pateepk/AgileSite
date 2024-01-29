using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to SharePoint services using current implementation of <see cref="ISharePointServiceFactoryProvider"/>.
    /// </summary>
    public static class SharePointServices
    {
        /// <summary>
        /// Gets implementation of SharePoint service suitable for version specified in connectionData. The service uses given connectionData.
        /// </summary>
        /// <typeparam name="IService">Type of service to get implementation for</typeparam>
        /// <param name="connectionData">Data (credentials etc.) needed to establish connection to SharePoint server</param>
        /// <returns>Service implementation</returns>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when no service factory for specified SharePoint version found.</exception>
        /// <exception cref="SharePointServiceNotSupportedException">Thrown when IService implementation is not available.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when available service implementation does not support given connectionData.</exception>
        public static IService GetService<IService>(SharePointConnectionData connectionData)
            where IService : ISharePointService
        {
            return SharePointServiceFactoryProvider.Current.GetSharePointServiceFactory(connectionData.SharePointConnectionSharePointVersion).GetImplementationFor<IService>(connectionData);
        }
    }
}
