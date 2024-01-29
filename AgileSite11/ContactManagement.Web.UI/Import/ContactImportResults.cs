using System;
using System.Collections.Generic;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Data class carrying results of the import operation. 
    /// </summary>
    internal class ContactImportResults
    {
        public ContactImportResults()
        {
            UpdatedContacts = new List<ContactInfo>();
            CreatedContactGuids = new List<Guid>();
            Exceptions = new List<Exception>();
            FailedData = new List<ContactImportData>();
        }


        /// <summary>
        /// Number of contacts from input batch which already existed in DB and were updated or were in the input batch more than once.
        /// </summary>
        public int Duplicities
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts from input batch which were not inserted or were not updated if they already existed in DB.
        /// </summary>
        public int Failures
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts which were created.
        /// </summary>
        public int Imported
        {
            get;
            set;
        }


        /// <summary>
        /// Guids of created contacts.
        /// </summary>
        public List<Guid> CreatedContactGuids
        {
            get;
            set;
        }


        /// <summary>
        /// Contacts which were successfully updated. Happens when imported contact already exists in DB.
        /// </summary>
        public List<ContactInfo> UpdatedContacts
        {
            get;
            set;
        }


        /// <summary>
        /// All exceptions which happened during import.
        /// </summary>
        public List<Exception> Exceptions
        {
            get;
            set;
        }


        /// <summary>
        /// Data which were not imported, data correspond to each exception in Exceptions
        /// </summary>
        public List<ContactImportData> FailedData
        {
            get;
            set;
        }
    }
}