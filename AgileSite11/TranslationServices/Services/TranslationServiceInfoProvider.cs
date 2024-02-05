using System;

using CMS.DataEngine;

namespace CMS.TranslationServices
{

    /// <summary>
    /// Class providing TranslationServiceInfo management.
    /// </summary>
    public class TranslationServiceInfoProvider : AbstractInfoProvider<TranslationServiceInfo, TranslationServiceInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationServiceInfoProvider()
            : base(TranslationServiceInfo.TYPEINFO, new HashtableSettings
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
        /// Returns translation service with specified ID.
        /// </summary>
        /// <param name="serviceId">Translation service ID.</param>        
        public static TranslationServiceInfo GetTranslationServiceInfo(int serviceId)
        {
            return ProviderObject.GetInfoById(serviceId);
        }


        /// <summary>
        /// Returns translation service with specified name.
        /// </summary>
        /// <param name="serviceName">Translation service name.</param>                
        public static TranslationServiceInfo GetTranslationServiceInfo(string serviceName)
        {
            return ProviderObject.GetInfoByCodeName(serviceName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified translation service.
        /// </summary>
        /// <param name="serviceObj">Translation service to be set.</param>
        public static void SetTranslationServiceInfo(TranslationServiceInfo serviceObj)
        {
            ProviderObject.SetInfo(serviceObj);
        }


        /// <summary>
        /// Deletes specified translation service.
        /// </summary>
        /// <param name="serviceObj">Translation service to be deleted.</param>
        public static void DeleteTranslationServiceInfo(TranslationServiceInfo serviceObj)
        {
            ProviderObject.DeleteInfo(serviceObj);
        }


        /// <summary>
        /// Deletes translation service with specified ID.
        /// </summary>
        /// <param name="serviceId">Translation service ID.</param>
        public static void DeleteTranslationServiceInfo(int serviceId)
        {
            TranslationServiceInfo serviceObj = GetTranslationServiceInfo(serviceId);
            DeleteTranslationServiceInfo(serviceObj);
        }


        /// <summary>
        /// Returns the query of all translation services.
        /// </summary>
        public static ObjectQuery<TranslationServiceInfo> GetTranslationServices()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all translation services matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<TranslationServiceInfo> GetTranslationServices(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetTranslationServices().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TranslationServiceInfo info)
        {
            base.SetInfo(info);

            AbstractHumanTranslationService.ClearHashtables();
            AbstractMachineTranslationService.ClearHashtables();
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(TranslationServiceInfo info)
        {
            base.DeleteInfo(info);

            AbstractHumanTranslationService.ClearHashtables();
            AbstractMachineTranslationService.ClearHashtables();
        }

        #endregion	
    }
}
