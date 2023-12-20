using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to proper instance of ISharePointServiceFactory.
    /// </summary>
    public class SharePointServiceFactoryProvider : ISharePointServiceFactoryProvider
    {
        #region "Variables"
        
        /// <summary>
        /// Dictionary mapping SharePoint versions to ISharePointServiceFactory implementations.
        /// </summary>
        private static readonly SafeDictionary<string, Type> serviceFactoryImplementations = new SafeDictionary<string, Type>
        {
            {SharePointVersion.SHAREPOINT_2010, typeof(SharePoint2010ServiceFactory)},
            {SharePointVersion.SHAREPOINT_2013, typeof(SharePoint2013ServiceFactory)},
            {SharePointVersion.SHAREPOINT_2016, typeof(SharePoint2013ServiceFactory)},
            {SharePointVersion.SHAREPOINT_ONLINE, typeof(SharePointOnlineServiceFactory)}
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets instance of current implementation of ISharePointServiceFactory.
        /// </summary>
        /// <returns>ISharePointServiceFactory implementation</returns>
        internal static ISharePointServiceFactoryProvider Current
        {
            get
            {
                return Service.Resolve<ISharePointServiceFactoryProvider>();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers new implementation of ISharePointServiceFactory for given SharePoint version.
        /// </summary>
        /// <typeparam name="TServiceFactory">Class implementing ISharePointServiceFactory</typeparam>
        /// <param name="sharePointVersion">SharePoint version</param>
        /// <seealso cref="SharePointVersion"/>
        public void RegisterFactory<TServiceFactory>(string sharePointVersion)
            where TServiceFactory : ISharePointServiceFactory, new()
        {
            serviceFactoryImplementations.Add(sharePointVersion, typeof(TServiceFactory));
        }


        /// <summary>
        /// Gets instance of ISharePointServiceFactory.
        /// </summary>
        /// <param name="sharePointVersion">SharePoint version for which to get service factory</param>
        /// <returns>ISharePointServiceFactory instance</returns>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when no service factory for sharePointVersion found.</exception>
        /// <seealso cref="SharePointVersion"/>
        public ISharePointServiceFactory GetSharePointServiceFactory(string sharePointVersion)
        {
            return GetSharePointServiceFactoryInternal(sharePointVersion);
        }


        /// <summary>
        /// Gets instance of ISharePointServiceFactory.
        /// </summary>
        /// <param name="sharePointVersion">SharePoint version for which to get service factory</param>
        /// <returns>ISharePointServiceFactory instance</returns>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when no service factory for sharePointVersion found.</exception>
        protected virtual ISharePointServiceFactory GetSharePointServiceFactoryInternal(string  sharePointVersion)
        {
            Type serviceFactoryImplementation;
            if ((sharePointVersion == null) || ((serviceFactoryImplementation = serviceFactoryImplementations[sharePointVersion]) == null))
            {
                throw new SharePointServiceFactoryNotSupportedException(String.Format("[SharePointServiceFactoryProvider.GetSharePointServiceFactoryInternal]: No service factory for version '{0}' available.", sharePointVersion));
            }

            ISharePointServiceFactory factory = (ISharePointServiceFactory) Activator.CreateInstance(serviceFactoryImplementation);

            return factory;
        }

        #endregion
    }
}
