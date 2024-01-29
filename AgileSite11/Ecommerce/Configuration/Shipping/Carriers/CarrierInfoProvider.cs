using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing CarrierInfo management.
    /// </summary>
    public class CarrierInfoProvider : AbstractInfoProvider<CarrierInfo, CarrierInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CarrierInfoProvider()
            : base(CarrierInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
            
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CarrierInfo objects.
        /// </summary>
        public static ObjectQuery<CarrierInfo> GetCarriers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns CarrierInfo with specified ID.
        /// </summary>
        /// <param name="id">CarrierInfo ID</param>
        public static CarrierInfo GetCarrierInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns CarrierInfo with specified name.
        /// </summary>
        /// <param name="name">CarrierInfo name</param>
        public static CarrierInfo GetCarrierInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Returns CarrierInfo with specified name.
        /// </summary>
        /// <param name="name">CarrierInfo name</param>
        /// <param name="siteName">Site name</param>
        public static CarrierInfo GetCarrierInfo(string name, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns CarrierInfo with specified GUID.
        /// </summary>
        /// <param name="guid">CarrierInfo GUID</param>                
        public static CarrierInfo GetCarrierInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CarrierInfo.
        /// </summary>
        /// <param name="infoObj">CarrierInfo to be set</param>
        public static void SetCarrierInfo(CarrierInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CarrierInfo.
        /// </summary>
        /// <param name="infoObj">CarrierInfo to be deleted</param>
        public static void DeleteCarrierInfo(CarrierInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CarrierInfo with specified ID.
        /// </summary>
        /// <param name="id">CarrierInfo ID</param>
        public static void DeleteCarrierInfo(int id)
        {
            CarrierInfo infoObj = GetCarrierInfo(id);
            DeleteCarrierInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"


        /// <summary>
        /// Returns a query for all the CarrierInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<CarrierInfo> GetCarriers(int siteId)
        {
            return ProviderObject.GetCarriersInternal(siteId);
        }


        /// <summary>
        /// Returns Carrier provider object for given id of carrier info. Returns instance of DefaultCarrierProvider when carrier not found.
        /// Return null when carrier provider class not found.
        /// </summary>
        /// <param name="carrierId">ID of carrier to instantiate carrier provider for.</param>
        public static ICarrierProvider GetCarrierProvider(int carrierId)
        {
            return ProviderObject.GetCarrierProviderInternal(carrierId);
        }

        #endregion


        #region "Internal methods - Advanced"


        /// <summary>
        /// Returns a query for all the CarrierInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<CarrierInfo> GetCarriersInternal(int siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }


        /// <summary>
        /// Returns Carrier provider object for given id of carrier info. Returns instance of DefaultCarrierProvider when carrier not found.
        /// Return null when carrier provider class not found.
        /// </summary>
        /// <param name="carrierId">ID of carrier to instantiate carrier provider for.</param>
        protected virtual ICarrierProvider GetCarrierProviderInternal(int carrierId)
        {
            var carrier = GetCarrierInfo(carrierId);
            if (carrier == null)
            {
                return new DefaultCarrierProvider();
            }

            return ClassHelper.GetClass<ICarrierProvider>(carrier.CarrierAssemblyName, carrier.CarrierClassName);
        }

        #endregion
    }
}