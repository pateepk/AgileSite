using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Membership;

namespace CMS.ContactManagement.Web.UI
{
    internal class ContactDemographicsGroupService : IContactDemographicsGroupService
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        public ContactDemographicsGroupService(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }


        public IEnumerable<ContactsGroupedByLocationViewModel> GroupContactsByCountry(ObjectQuery<ContactInfo> contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var contactGroups = contacts.Columns("ContactCountryID")
                                        .AddColumn(new CountColumn().As("ContactCount"))
                                        .WhereGreaterThan("ContactCountryID", 0)
                                        .GroupBy("ContactCountryID")
                                        .Select(dataRow => new
                                         {
                                             CountryID = dataRow["ContactCountryID"].ToInteger(0),
                                             NumberOfContacts = dataRow["ContactCount"].ToInteger(0)
                                         })
                                        .ToList();

            return CountryInfoProvider.GetCountries()
                                      .ToList()
                                      .Join(contactGroups,
                                            country => country.CountryID,
                                            contactGroup => contactGroup.CountryID,
                                            (country, contactGroup) => new ContactsGroupedByLocationViewModel
                                            {
                                                LocationKey = country.CountryTwoLetterCode,
                                                NumberOfContacts = contactGroup.NumberOfContacts
                                            });
        }


        public IEnumerable<ContactsGroupedByLocationViewModel> GroupContactsByState(ObjectQuery<ContactInfo> contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var contactGroups = contacts.Columns("ContactStateID")
                                        .AddColumn(new CountColumn().As("ContactCount"))
                                        .WhereGreaterThan("ContactStateID", 0)
                                        .GroupBy("ContactStateID")
                                        .Select(dataRow => new
                                        {
                                            StateID = dataRow["ContactStateID"].ToInteger(0),
                                            NumberOfContacts = dataRow["ContactCount"].ToInteger(0)
                                        })
                                        .ToList();

            return StateInfoProvider.GetStates()
                                    .ToList()
                                    .Join(contactGroups,
                                          state => state.StateID,
                                          contactGroup => contactGroup.StateID,
                                          (state, contactGroup) => new ContactsGroupedByLocationViewModel
                                          {
                                              LocationKey = state.StateCode,
                                              NumberOfContacts = contactGroup.NumberOfContacts
                                          });
        }


        public IEnumerable<ContactsGroupedByGenderViewModel> GroupContactsByGender(ObjectQuery<ContactInfo> contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var contactGroups = contacts.Columns("ContactGender")
                                        .AddColumn(new CountColumn().As("ContactCount"))
                                        .GroupBy("ContactGender")
                                        .Select(GetContactsGroupedByGenderViewModel)
                                        .ToList();

            if (contactGroups.Count(group => group.Gender == UserGenderEnum.Unknown) > 1)
            {
                // contacts contained some contacts with NULL gender
                MergeUnknownGenderGroups(contactGroups);
            }

            AddGendersWithoutContacts(contactGroups);
            contactGroups.Sort(new ContactGroupedByGenderViewModelComparer());
            return contactGroups;
        }


        public IEnumerable<ContactsGroupedByAgeViewModel> GroupContactsByAge(ObjectQuery<ContactInfo> contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            var currentDate = mDateTimeNowService.GetDateTimeNow().Date;
            var queryBuilder = new ContactGroupedByAgeQueryBuilder(currentDate);

            var dataRow = queryBuilder.AddAgeColumn(18, 25, "_18_24", AgeCategoryEnum._18_24)
                                      .AddAgeColumn(25, 35, "_25_34", AgeCategoryEnum._25_34)
                                      .AddAgeColumn(35, 45, "_35_44", AgeCategoryEnum._35_44)
                                      .AddAgeColumn(45, 55, "_45_54", AgeCategoryEnum._45_54)
                                      .AddAgeColumn(55, 65, "_55_64", AgeCategoryEnum._55_64)
                                      .AddAgeColumn(65, currentDate.Year - 1, "_65plus", AgeCategoryEnum._65plus)
                                      .GetQuery(contacts).Result.Tables[0].Rows[0];

            foreach (var mapping in queryBuilder.AgeColumnMapping)
            {
                yield return BuildContactsGroupedByAgeViewModel(dataRow, mapping.Key, mapping.Value);
            }

            yield return BuildContactsGroupedByAgeViewModel(dataRow, AgeCategoryEnum.Unknown, "Unknown");
        }


        private ContactsGroupedByAgeViewModel BuildContactsGroupedByAgeViewModel(DataRow dataRow, AgeCategoryEnum category, string columnName)
        {
            return new ContactsGroupedByAgeViewModel
            {
                Category = category,
                NumberOfContacts = dataRow[columnName].ToInteger(0)
            };
        }


        private void MergeUnknownGenderGroups(List<ContactsGroupedByGenderViewModel> contactGroups)
        {
            int numberOfContacts = contactGroups.Where(group => group.Gender == UserGenderEnum.Unknown)
                                                .Sum(group => group.NumberOfContacts);

            var groupedContactsWithUnknownGender = GetContactsGroupedByGenderViewModel(UserGenderEnum.Unknown, numberOfContacts);

            contactGroups.RemoveAll(group => group.Gender == UserGenderEnum.Unknown);
            contactGroups.Add(groupedContactsWithUnknownGender);
        }


        private void AddGendersWithoutContacts(List<ContactsGroupedByGenderViewModel> contactGroups)
        {
            var gendersWithContacts = contactGroups.Select(cg => cg.Gender);
            var allGenders = (UserGenderEnum[])Enum.GetValues(typeof(UserGenderEnum));

            var groupedGendersWithoutContacts = allGenders.Except(gendersWithContacts)
                                                          .Select(gender => GetContactsGroupedByGenderViewModel(gender, 0));

            contactGroups.AddRange(groupedGendersWithoutContacts);
        }


        private ContactsGroupedByGenderViewModel GetContactsGroupedByGenderViewModel(UserGenderEnum gender, int numberOfContacts)
        {
            return new ContactsGroupedByGenderViewModel()
            {
                Gender = gender,
                NumberOfContacts = numberOfContacts
            };
        }


        private ContactsGroupedByGenderViewModel GetContactsGroupedByGenderViewModel(DataRow dataRow)
        {
            var gender = (UserGenderEnum)dataRow["ContactGender"].ToInteger((int)UserGenderEnum.Unknown);
            int numberOfContacts = dataRow["ContactCount"].ToInteger(0);
            return GetContactsGroupedByGenderViewModel(gender, numberOfContacts);
        }
    }
}