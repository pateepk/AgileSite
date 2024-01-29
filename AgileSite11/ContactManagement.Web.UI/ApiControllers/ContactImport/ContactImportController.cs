using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;

using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactImportController))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Handles posting of contacts to the server.
    /// </summary>
    /// <exclude />
    [ContactImportExceptionHandling]
    public sealed class ContactImportController : CMSApiController
    {
        /// <summary>
        /// Imports contacts. 
        /// Contacts are assigned to contact group when <paramref name="contactGroupGuid"/> is specified. 
        /// </summary>
        /// <param name="request">Fields must contain "ContactEmail".</param>
        /// <param name="contactGroupGuid">Optional contact group where contacts will be assigned</param>
        public ResultModel Post([FromBody] RequestModel request, Guid? contactGroupGuid = null)
        {
            if (!IsAuthorized())
            {
                throw new UnauthorizedAccessException();
            }

            CheckRequestDataIntegrity(request);

            var resultModel = new ResultModel();

            // Convert to usable data
            var importData = ImportDataConvertor
                .ConvertToImportData(request.FieldsOrder, request.FieldValues,
                    (failedToConvert, exception) =>
                    {
                        resultModel.Failures++;
                        resultModel.NotImportedContacts.FieldValues.Add(failedToConvert);
                        resultModel.NotImportedContacts.Messages.Add(GetMessageFromException(exception));
                    })
                .Select(ImportDataEmailFixer.TrimSpaces)
                .ToList();

            // Split the sets into valid and invalid emails
            var dataSplitByEmailValidity = importData
                .GroupBy(contactImportData => ValidationHelper.IsEmail(contactImportData.Email))
                .ToDictionary(g => g.Key, g => g.ToList());
            var contactDataWithValidEmails = dataSplitByEmailValidity.ContainsKey(true) ? dataSplitByEmailValidity[true] : new List<ContactImportData>();
            var contactDataWithInvalidEmails = dataSplitByEmailValidity.ContainsKey(false) ? dataSplitByEmailValidity[false] : new List<ContactImportData>();

            // Update result
            if (contactDataWithInvalidEmails.Any())
            {
                resultModel.Failures += contactDataWithInvalidEmails.Count;
                resultModel.NotImportedContacts.FieldValues.AddRange(contactDataWithInvalidEmails.Select(r => r.Data.Values.ToList()));
                resultModel.NotImportedContacts.Messages.AddRange(Enumerable.Repeat(ResHelper.GetString("om.contact.importcsv.invalidemailrow"), contactDataWithInvalidEmails.Count));
            }

            // Get only unique contacts
            var uniqueEmailData = GetUniqueImportData(contactDataWithValidEmails, resultModel);

            // Import data
            ContactImporter mContactImporter = new ContactImporter(Service.Resolve<IContactChangeRepository>());
            var importResults = mContactImporter.ImportContacts(uniqueEmailData.Select(d => d.Value).ToList());
            
            // Update result
            resultModel.Imported = importResults.Imported;
            resultModel.Duplicities += importResults.Duplicities;
            resultModel.Failures += importResults.Failures;
            resultModel.NotImportedContacts.FieldValues.AddRange(importResults.FailedData.Select(r => r.Data.Values.ToList()));
            resultModel.NotImportedContacts.Messages.AddRange(importResults.Exceptions.Select(GetMessageFromException));

            // Add contacts to a contact group, if needed
            if (contactGroupGuid.HasValue)
            {
                AddToContactGroup(contactGroupGuid.Value, importResults);
            }

            if (resultModel.NotImportedContacts.FieldValues.Any())
            {
                resultModel.NotImportedContacts.FieldsOrder = request.FieldsOrder;
            }

            return resultModel;
        }


        #region "Private methods"


        /// <summary>
        /// For <see cref="ContactImportException"/> returns UIMessage otherwise exception message.
        /// </summary>
        private string GetMessageFromException(Exception e)
        {
            return e is ContactImportException ? ((ContactImportException)e).UIMessage : e.Message;
        }


        /// <summary>
        /// Filter out duplicates, the last one is one that's needed, because of consistency when duplicates are in more than one batch.
        /// Also updates <paramref name="resultModel"/> duplicities.
        /// </summary>
        private Dictionary<string, ContactImportData> GetUniqueImportData(List<ContactImportData> contactDataWithValidEmails, ResultModel resultModel)
        {
            var uniqueEmailData = new Dictionary<string, ContactImportData>(ContactImporter.EmailComparer);
            foreach (var contactImportData in contactDataWithValidEmails)
            {
                if (uniqueEmailData.ContainsKey(contactImportData.Email))
                {
                    resultModel.Duplicities++;
                }
                uniqueEmailData[contactImportData.Email] = contactImportData;
            }
            return uniqueEmailData;
        }


        private static void CheckRequestDataIntegrity(RequestModel request)
        {            
            if (request.FieldsOrder == null)
            {
                throw new ArgumentException("request.FieldsOrder cannot be null.");
            }

            if (request.FieldValues == null)
            {
                throw new ArgumentException("request.FieldValues cannot be null.");
            }

            // Duplicated fields are not allowed
            if (request.FieldsOrder.Count != request.FieldsOrder.Distinct().Count())
            {
                throw new ContactImportException("Duplicated fields in request are not allowed.", "om.contact.importcsv.duplicatefields");
            }

            // Validate received fields with existing Contact class
            var fieldsProvider = new ContactImportFieldsProvider();
            if (!request.FieldsOrder.All(fieldsProvider.IsFieldAvailableForImport))
            {
                throw new ContactImportException("Some of the fields you mapped are not meant for import.", "om.contact.importcsv.fieldnotallowed");
            }

            // Checks that the email address field exists
            if (request.FieldsOrder.IndexOf("ContactEmail") < 0)
            {
                throw new ContactImportException("ContactEmail field not found.", "om.contact.importcsv.emailfieldnotfound");
            }
        }


        private static void AddToContactGroup(Guid contactGroupGuid, ContactImportResults importResults)
        {
            // Contact group must be on given site. Global groups are not allowed.
            var contactGroup = ContactGroupInfoProvider.GetContactGroups()
                                                   .WhereEquals("ContactGroupGuid", contactGroupGuid)
                                                   .FirstOrDefault();

            if (contactGroup == null)
            {
                throw new ContactImportException("Contact group not found for given site.", "om.contact.importcsv.cgnotfound");
            }

            using (new CMSConnectionScope(DatabaseSeparationHelper.OM_CONNECTION_STRING, true))
            {
                AddExistingContactsToContactGroup(contactGroup, importResults.UpdatedContacts);
                AddNewContactsToContactGroup(contactGroup, importResults.CreatedContactGuids);
            }
        }

        
        private bool IsAuthorized()
        {
            var user = MembershipContext.AuthenticatedUser;

            if (user == null)
            {
                return false;
            }

            string[] requiredPermissions =
            {
                "Read",
                "Modify"
            };

            return requiredPermissions.All(permission => user.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, permission));
        }


        private static void AddNewContactsToContactGroup(ContactGroupInfo contactGroup, List<Guid> contactsGuids)
        {
            // Add only those contacts which are not part of contact group yet.
            var contactIDs = ContactInfoProvider.GetContacts()
                                                .WhereIn("ContactGuid", contactsGuids.Select(g => g.ToString()).ToList())
                                                .Column("ContactID");

            CreateManualContactGroupMembers(contactGroup, contactIDs);
        }


        /// <summary>
        /// Adds contacts represented by <paramref name="contacts"/> to the contact group specified by <paramref name="contactGroup"/>. 
        /// Only those contacts which are not part of contact group should be passed in <paramref name="contacts"/>. If some contact who is already
        /// a member of the group will be added, the whole operation will be unsuccessful.
        /// Contacts will be added as manual, not automatic and not from account.
        /// </summary>
        /// <param name="contactGroup">Contact group where contacts will be assigned</param>
        /// <param name="contacts">Contacts which will be added</param>
        private static void CreateManualContactGroupMembers(ContactGroupInfo contactGroup, IEnumerable<ContactInfo> contacts)
        {
            var dataTable = ClassStructureInfo.GetClassInfo("om.contactgroupmember").GetNewDataSet().Tables[0];

            foreach (var contact in contacts)
            {
                var row = dataTable.NewRow();
                row["ContactGroupMemberContactGroupID"] = contactGroup.ContactGroupID;
                row["ContactGroupMemberType"] = (int)ContactGroupMemberTypeEnum.Contact;
                row["ContactGroupMemberRelatedID"] = contact.ContactID;
                row["ContactGroupMemberFromCondition"] = false;
                row["ContactGroupMemberFromAccount"] = false;
                row["ContactGroupMemberFromManual"] = true;

                dataTable.Rows.Add(row);
            }

            ConnectionHelper.BulkInsert(dataTable, "OM_ContactGroupMember", new BulkInsertSettings
            {
                Options = SqlBulkCopyOptions.CheckConstraints
            });
        }


        private static void AddExistingContactsToContactGroup(ContactGroupInfo contactGroup, IEnumerable<ContactInfo> contacts)
        {
            foreach (var contact in contacts)
            {
                ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(
                    contactGroup.ContactGroupID,
                    contact.ContactID,
                    ContactGroupMemberTypeEnum.Contact,
                    MemberAddedHowEnum.Manual);
            }
        }

        #endregion
    }
}