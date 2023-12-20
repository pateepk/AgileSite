using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Imports contacts into database.
    /// </summary>
    internal class ContactImporter
    {
        private readonly IContactChangeRepository mContactChangeRepository;
        private Dictionary<string, FormFieldInfo> mContactFields;


        /// <summary>
        /// Gets the email comparer which determines email's uniqueness.
        /// </summary>
        public static StringComparer EmailComparer => StringComparer.OrdinalIgnoreCase;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactImporter"/>.
        /// </summary>
        /// <param name="contactChangeRepository">Represents contact change repository</param>
        /// <exception cref="ArgumentNullException"><paramref name="contactChangeRepository"/> is <c>null</c></exception>
        public ContactImporter(IContactChangeRepository contactChangeRepository)
        {
            if (contactChangeRepository == null)
            {
                throw new ArgumentNullException("contactChangeRepository");
            }

            mContactChangeRepository = contactChangeRepository;
        }


        /// <summary>
        /// Imports contacts into database. Large number of contacts can be imported, because bulk insert is used. Finds existing contacts by ContactEmail and updates them.
        /// This method does not check permissions. Contact groups and scoring are recalculated only for updated contacts.
        /// Contacts cannot be imported as global.
        /// </summary>
        /// <example>
        /// Input:
        /// 1@email.cz
        /// 2@email.cz
        /// 2@email.cz
        /// 2@email.cz
        /// 3@email.cz
        /// 
        /// Already in DB:
        /// 3@email.cz
        /// 
        /// That's the result, when everything succeeds:
        /// 2 imported (1@email.cz and 2@email.cz)
        /// 3 duplicities (twice 2@email.cz and 3@email.cz) 
        /// </example>
        /// <param name="records">Contacts to be imported</param>
        /// <returns>Results of the import</returns>
        public ContactImportResults ImportContacts(IList<ContactImportData> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            // Already existing contacts with email in records
            // Key is contact email
            ILookup<string, ContactInfo> existingContactsByEmail = ContactInfoProvider.GetContacts()
                                                                                      .WhereIn("ContactEmail", records.Select(d => d.Email).ToList())
                                                                                      .AsEnumerable()
                                                                                      .ToLookup(c => c.ContactEmail, EmailComparer);

            var contactDataTable = ClassStructureInfo.GetClassInfo("om.contact").GetNewDataSet().Tables[0];
            var contactsInContactDataTable = new List<ContactImportData>();

            var result = new ContactImportResults();

            foreach (var record in records)
            {
                if (existingContactsByEmail.Contains(record.Email))
                {
                    UpdateExistingContacts(existingContactsByEmail[record.Email], record, result);
                }
                else
                {
                    Guid newContactGuid = Guid.NewGuid();

                    try
                    {
                        AddDataRow(contactDataTable, record, newContactGuid);
                        result.CreatedContactGuids.Add(newContactGuid);
                        contactsInContactDataTable.Add(record);
                    }
                    catch (Exception e)
                    {
                        result.Failures++;
                        result.Exceptions.Add(e);
                        result.FailedData.Add(record);
                    }
                }
            }
            if (contactDataTable.Rows.Count > 0)
            {
                try
                {
                    InsertContactsToDatabase(contactDataTable);

                    result.Imported += contactDataTable.Rows.Count;
                }
                catch(Exception e)
                {
                    result.Failures += contactDataTable.Rows.Count;
                    result.Exceptions.AddRange(Enumerable.Repeat(e, contactDataTable.Rows.Count));
                    result.FailedData = contactsInContactDataTable;
                }
            }

            return result;
        }


        private Dictionary<string, FormFieldInfo> GetContactFields()
        {
            if (mContactFields == null)
            {
                var filterFieldsForm = FormHelper.GetFormInfo(ContactInfo.OBJECT_TYPE, false);
                mContactFields = filterFieldsForm.GetFields(true, false).ToDictionary(f => f.Name);
            }

            return mContactFields;
        }


        /// <summary>
        /// Inserts given dataTable containing contact data to database.
        /// </summary>
        private void InsertContactsToDatabase(DataTable contactDataTable)
        {
            using (new CMSConnectionScope(DatabaseSeparationHelper.OM_CONNECTION_STRING, true))
            {
                ConnectionHelper.BulkInsert(contactDataTable, "OM_Contact", new BulkInsertSettings
                {
                    Options = SqlBulkCopyOptions.CheckConstraints
                });
            }

            Guid firstContactGuid = (Guid)contactDataTable.Rows[0]["ContactGUID"];
            var firstContact = ContactInfoProvider.GetContactInfo(firstContactGuid);

            for (int i = firstContact.ContactID; i < firstContact.ContactID + contactDataTable.Rows.Count; i++)
            {
                var changeData = new ContactChangeData
                {
                    ContactID = i,
                    ChangedColumns = null,
                    ContactIsNew = true,
                    ContactWasMerged = false,
                };

                mContactChangeRepository.Save(changeData);
            }
        }


        /// <summary>
        /// Updates <paramref name="contactsToUpdate"/> with values specified in <paramref name="contactImportData"/>.
        /// </summary>
        /// <param name="contactsToUpdate">Those contacts will be updated</param>
        /// <param name="contactImportData">New field values</param>
        /// <param name="result">Results of this operation will be stored in this instance</param>
        private void UpdateExistingContacts(IEnumerable<ContactInfo> contactsToUpdate, ContactImportData contactImportData, ContactImportResults result)
        {
            var exceptions = new List<Exception>();

            foreach (var contact in contactsToUpdate)
            {
                try
                {
                    SetContactInfoFields(contact, contactImportData);
                    ContactInfoProvider.SetContactInfo(contact);
                    result.UpdatedContacts.Add(contact);
                }
                catch (ContactImportException e)
                {
                    exceptions.Add(e);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Conversion failed"))
                    {
                        string errorMessage = ResHelper.GetString("om.contact.importcsv.checkmapping");
                        exceptions.Add(new ContactImportException(errorMessage, errorMessage, e));
                    }
                    else if (e.InnerException != null)
                    {
                        // DbExceptions are handled by AbstractDataConnection.HandleError and the db exception is in InnerException
                        exceptions.Add(e.InnerException);    
                    }
                    else
                    {
                        exceptions.Add(e);
                    }
                }
            }

            if (exceptions.Any())
            {
                result.Failures++;
                result.FailedData.Add(contactImportData);
                result.Exceptions.Add(new AggregateException(String.Join("\n", exceptions.Select(e => e.Message)), exceptions));
            }
            else
            {
                result.Duplicities++; // One contact from input set had duplicities
            }
        }


        /// <summary>
        /// Sets <paramref name="contact"/> fields to values specified in <paramref name="contactImportData"/>. 
        /// </summary>
        /// <param name="contact">Contact whose fields will be updated</param>
        /// <param name="contactImportData">New values of the fields</param>
        private void SetContactInfoFields(ContactInfo contact, ContactImportData contactImportData)
        {
            foreach (var contactData in contactImportData.Data)
            {
                ValidateFieldLength(contactData.Key, contactData.Value);
                contact.SetValue(contactData.Key, contactData.Value);
            }
        }


        /// <summary>
        /// Adds <paramref name="contactImportData"/> as row to <paramref name="contactDataTable"/>. 
        /// </summary>
        private void AddDataRow(DataTable contactDataTable, ContactImportData contactImportData, Guid guid)
        {
            var row = contactDataTable.NewRow();

            row["ContactGUID"] = guid.ToString();
            row["ContactLastModified"] = row["ContactCreated"] = DateTime.Now;
            row["ContactMonitored"] = true;

            var contactFields = GetContactFields();
            foreach (var contactData in contactImportData.Data)
            {
                ValidateFieldLength(contactData.Key, contactData.Value);

                try
                {
                    row[contactData.Key] = contactData.Value;
                }
                catch (Exception e)
                {
                    if (e is InvalidCastException || e is ArgumentException)
                    {
                        string errorMessage = GetErrorMessageForInvalidCast(
                            contactFields[contactData.Key].GetDisplayName(MacroResolver.GetInstance()),
                            contactFields[contactData.Key].DataType
                            );
                        
                        throw new ContactImportException(errorMessage, errorMessage, e);
                    }

                    throw;
                }
            }

            if (string.IsNullOrEmpty(row["ContactLastName"].ToString()))
            {
                row["ContactLastName"] = ContactHelper.ANONYMOUS + DateTime.Now.ToString(ContactHelper.ANONYMOUS_CONTACT_LASTNAME_DATE_PATTERN);
            }

            contactDataTable.Rows.Add(row);
        }


        /// <summary>
        /// Prepares error message when invalid cast occures while processing contact data.
        /// </summary>
        /// <param name="fieldDisplayName">Field name</param>
        /// <param name="fieldDataType">Type of field</param>
        /// <returns>Localized error message.</returns>
        private static string GetErrorMessageForInvalidCast(string fieldDisplayName, string fieldDataType)
        {
            switch (fieldDataType)
            {
                case FieldDataType.Integer:
                case FieldDataType.LongInteger:
                    return String.Format(
                        ResHelper.GetString("om.contact.importcsv.integerexpected"),
                        ResHelper.LocalizeString(fieldDisplayName)
                        );

                default:
                    return String.Format(
                        ResHelper.GetString("om.contact.importcsv.invalidtype"),
                        ResHelper.LocalizeString(fieldDisplayName),
                        fieldDataType
                        );
            }
        }


        /// <summary>
        /// Validates value according to fields parameters.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="value">Field value</param>
        private void ValidateFieldLength(string fieldName, string value)
        {
            if (value == null)
            {
                return;
            }

            int allowedSize = GetContactFields()[fieldName].Size;

            if (allowedSize > 0 && value.Length >= allowedSize)
            {
                string errorMessage = String.Format(
                    ResHelper.GetString("om.contact.importcsv.toolongvalue"),
                    ResHelper.LocalizeString(GetContactFields()[fieldName].GetDisplayName(MacroResolver.GetInstance())),
                    allowedSize
                    );

                throw new ContactImportException(errorMessage, errorMessage);
            }
        }
    }
}