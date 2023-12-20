using System;
using System.Collections.Generic;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Represents data for contact import. 
    /// </summary>
    internal class ContactImportData
    {
        /// <summary>
        /// Contact email
        /// </summary>
        public string Email
        {
            get
            {
                return Data["ContactEmail"];
            }
            set
            {
                Data["ContactEmail"] = value;
            }
        }


        public Dictionary<string, string> mData;


        /// <summary>
        /// Represents FieldName, FieldValue pair
        /// </summary>
        public Dictionary<string, string> Data
        {
            get { return mData; }
            private set { mData = value; }
        }
        

        public ContactImportData(IList<string> fields, IList<string> fieldsValues)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }
            if (fieldsValues == null)
            {
                throw new ArgumentNullException("fieldsValues");
            }

            if (fields.Count != fieldsValues.Count)
            {
                throw new ArgumentException("");
            }

            Data = new Dictionary<string, string>();
            for (int i = 0; i < fields.Count; i++)
            {
                Data.Add(fields[i], fieldsValues[i]);
            }
        }
    }
}