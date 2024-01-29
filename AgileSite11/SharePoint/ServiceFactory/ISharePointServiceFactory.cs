using System;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Describes SharePoint service factory.
    /// </summary>
    public interface ISharePointServiceFactory
    {
        /// <summary>
        /// Gets implementation of SharePoint service. The service uses given connectionData.
        /// </summary>
        /// <typeparam name="IService">Type of service to get implementation for</typeparam>
        /// <param name="connectionData">Data (credentials etc.) needed to establish connection to SharePoint server</param>
        /// <returns>Service implementation</returns>
        /// <exception cref="CMS.SharePoint.SharePointServiceNotSupportedException">Thrown when IService implementation is not available.</exception>
        /// <exception cref="CMS.SharePoint.SharePointConnectionNotSupportedException">Thrown when available service implementation does not support given connectionData.</exception>
        IService GetImplementationFor<IService>(SharePointConnectionData connectionData)
            where IService : ISharePointService;
    }
}
