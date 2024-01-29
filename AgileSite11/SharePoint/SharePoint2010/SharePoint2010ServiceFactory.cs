using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides instances of SharePoint 2010 services' implementations.
    /// </summary>
    internal class SharePoint2010ServiceFactory : ISharePointServiceFactory
    {
        /// <summary>
        /// Contains mapping of SharePoint service interfaces to SharePoint 2010 implementations.
        /// </summary>
        private static readonly Dictionary<Type, Type> serviceImplementations = new Dictionary<Type, Type>()
        {
            {typeof(ISharePointSiteService), typeof(SharePoint2010SiteService)},
            {typeof(ISharePointListService), typeof(SharePoint2010ListService)},
            {typeof(ISharePointFileService), typeof(SharePoint2010FileService)}
        };


        /// <summary>
        /// Gets implementation of SharePoint service. The service is provided given connectionData.
        /// </summary>
        /// <typeparam name="IService">Type of service to get implementation for</typeparam>
        /// <param name="connectionData">Data (credentials etc.) needed to establish connection to SharePoint server</param>
        /// <returns>Service implementation</returns>
        /// <exception cref="CMS.SharePoint.SharePointServiceNotSupportedException">Thrown when IService implementation is not available.</exception>
        /// <exception cref="CMS.SharePoint.SharePointConnectionNotSupportedException">Thrown when available service implementation does not support given connectionData.</exception>
        public IService GetImplementationFor<IService>(SharePointConnectionData connectionData)
            where IService : ISharePointService
        {
            if (!serviceImplementations.ContainsKey(typeof(IService)))
                throw new SharePointServiceNotSupportedException(String.Format("[SharePoint2010ServiceFactory.GetImplementationFor]: The service of type '{0}' does not have an implementation.", typeof(IService)));

            // Make sure service works with its own copy of connectionData
            SharePointConnectionData connectionDataClone = connectionData.Clone();

            Type serviceImplementation = serviceImplementations[typeof(IService)];
            IService serviceInstance = (IService)Activator.CreateInstance(serviceImplementation, connectionDataClone);

            if (!serviceInstance.IsConnectionSupported())
            {
                throw new SharePointConnectionNotSupportedException(String.Format("[SharePoint2010ServiceFactory.GetImplementationFor]: Available service implementation '{0}' for '{1}' does not support given connectionData. The authentication mode might not be supported.",
                    serviceImplementation, typeof(IService)));
            }

            return serviceInstance;
        }
    }
}
