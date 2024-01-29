using System.Collections.Generic;
using System.Data.SqlClient;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.EventLog;
using CMS.LicenseProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing ContactChangeRecalculationQueueInfo management.
    /// </summary>
    internal class ContactChangeRecalculationQueueInfoProvider : AbstractInfoProvider<ContactChangeRecalculationQueueInfo, ContactChangeRecalculationQueueInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactChangeRecalculationQueueInfoProvider()
            : base(ContactChangeRecalculationQueueInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContactChangeRecalculationQueueInfo objects.
        /// </summary>
        public static ObjectQuery<ContactChangeRecalculationQueueInfo> GetContactChangeRecalculationQueues()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ContactChangeRecalculationQueueInfo with specified ID.
        /// </summary>
        /// <param name="id">ContactChangeRecalculationQueueInfo ID</param>
        public static ContactChangeRecalculationQueueInfo GetContactChangeRecalculationQueueInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ContactChangeRecalculationQueueInfo.
        /// </summary>
        /// <param name="infoObj">ContactChangeRecalculationQueueInfo to be set</param>
        public static void SetContactChangeRecalculationQueueInfo(ContactChangeRecalculationQueueInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified ContactChangeRecalculationQueueInfo.
        /// </summary>
        /// <param name="infoObj">ContactChangeRecalculationQueueInfo to be deleted</param>
        public static void DeleteContactChangeRecalculationQueueInfo(ContactChangeRecalculationQueueInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes ContactChangeRecalculationQueueInfo with specified ID.
        /// </summary>
        /// <param name="id">ContactChangeRecalculationQueueInfo ID</param>
        public static void DeleteContactChangeRecalculationQueueInfo(int id)
        {
            ContactChangeRecalculationQueueInfo infoObj = GetContactChangeRecalculationQueueInfo(id);
            DeleteContactChangeRecalculationQueueInfo(infoObj);
        }

        #endregion


        #region "Bulk insert methods"

        /// <summary>
        /// Bulk inserts the given <paramref name="contactChange"/>.
        /// </summary>
        /// <param name="contactChange">Data table containing values to be inserted</param>
        public static void BulkInsert(IEnumerable<IDataTransferObject> contactChange)
        {
            if (InsufficientLicense())
            {
                string message = "ContactChangeRecalculationQueueInfoProvider.BulkInsert";
                EventLogProvider.LogEvent(EventType.WARNING, message, LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message);
                return;
            }

            using (new CMSConnectionScope(DatabaseSeparationHelper.OM_CONNECTION_STRING, true))
            {
                var dataTableContainer = Service.Resolve<IDataTableProvider>().ConvertToDataTable(contactChange, PredefinedObjectType.CONTACTCHANGERECALCULATIONQUEUEINFO);
                ConnectionHelper.BulkInsert(dataTableContainer.DataTable, "OM_ContactChangeRecalculationQueue", new BulkInsertSettings
                {
                    Options = SqlBulkCopyOptions.CheckConstraints
                });
            }
        }


        private static bool InsufficientLicense()
        {
            return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement, ModuleName.CONTACTMANAGEMENT);
        }

        #endregion
    }
}