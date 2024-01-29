using System;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.MacroEngine;
using CMS.OnlineMarketing;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides transformation of contacts to SalesForce leads using the specified mapping.
    /// </summary>
    public sealed class ContactMapper : Mapper
    {

        #region "Private members"

        private string mDescriptionMacro;
        private string mDefaultCompanyName;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the ContactMapper class.
        /// </summary>
        /// <param name="attributeValueConverterFactory">A factory that creates instances of entity attribute value converters.</param>
        /// <param name="mapping">The mapping of contacts to SalesForce leads.</param>
        /// <param name="entityModel">The lead entity model.</param>
        /// <param name="formInfo">The contact form info.</param>
        /// <param name="descriptionMacro">The macro that provides the description of the transformed contact.</param>
        /// <param name="defaultCompanyName">The company name to use when there is no account associated with the transformed contact.</param>
        public ContactMapper(AttributeValueConverterFactory attributeValueConverterFactory, Mapping mapping, EntityModel entityModel, FormInfo formInfo, string descriptionMacro, string defaultCompanyName) : base(attributeValueConverterFactory, mapping, entityModel, formInfo)
        {
            mDescriptionMacro = descriptionMacro;
            mDefaultCompanyName = defaultCompanyName;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Maps meta property of CMS object to SalesForce entity field.
        /// </summary>
        /// <param name="item">An item that maps meta property of CMS object.</param>
        /// <param name="baseInfo">The object to transform.</param>
        /// <param name="entity">The SalesForce entity to update.</param>
        protected override void MapMetaField(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            switch (item.SourceName)
            {
                case "Description":
                    MapDescription(item, baseInfo, entity);
                    break;
                case "CompanyName":
                    MapCompanyName(item, baseInfo, entity);
                    break;
                case "Country":
                    MapCountry(item, baseInfo, entity);
                    break;
                case "State":
                    MapState(item, baseInfo, entity);
                    break;
                default:
                    throw new Exception("[ContactMapper.MapMetaField]: Unsupported meta field name.");
            }
        }

        #endregion


        #region "Private methods"

        private void MapDescription(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            ContactInfo contact = baseInfo as ContactInfo;
            if (attributeModel != null && contact != null && !String.IsNullOrEmpty(mDescriptionMacro))
            {
                MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
                resolver.SetNamedSourceData("Contact", contact);
                entity[attributeModel.Name] = resolver.ResolveMacros(mDescriptionMacro);
            }
        }


        private void MapCompanyName(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            ContactInfo contact = baseInfo as ContactInfo;
            if (attributeModel != null && contact != null)
            {
                entity[attributeModel.Name] = mDefaultCompanyName;
                if (!String.IsNullOrEmpty(contact.ContactCompanyName))
                {
                    entity[attributeModel.Name] = contact.ContactCompanyName;
                }
                else
                {
                    AccountInfo account = GetContactAccount(contact);
                    if (account != null && !String.IsNullOrEmpty(account.AccountName))
                    {
                        entity[attributeModel.Name] = account.AccountName;
                    }
                }
            }
        }


        private void MapCountry(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            ContactInfo contact = baseInfo as ContactInfo;
            CountryInfo country = CountryInfoProvider.GetCountryInfo(contact.ContactCountryID);
            if (country != null)
            {
                entity[attributeModel.Name] = country.CountryDisplayName;
            }
            else
            {
                entity[attributeModel.Name] = String.Empty;
            }
        }


        private void MapState(MappingItem item, BaseInfo baseInfo, Entity entity)
        {
            EntityAttributeModel attributeModel = mEntityModel.GetAttributeModel(item.AttributeName);
            ContactInfo contact = baseInfo as ContactInfo;
            StateInfo state = StateInfoProvider.GetStateInfo(contact.ContactStateID);
            if (state != null)
            {
                entity[attributeModel.Name] = state.StateDisplayName;
            }
            else
            {
                entity[attributeModel.Name] = String.Empty;
            }
        }


        private AccountInfo GetContactAccount(ContactInfo contactInfo)
        {
            AccountInfo account = null;
            if (contactInfo.Accounts.Count > 0)
            {
                if (contactInfo.Accounts.Count == 1)
                {
                    account = contactInfo.Accounts[0] as AccountInfo;
                }
                else
                {
                    account = contactInfo.Accounts.Cast<AccountInfo>().OrderBy(x => x.AccountID).FirstOrDefault(x => x.AccountPrimaryContactID == contactInfo.ContactID);
                    if (account == null)
                    {
                        account = contactInfo.Accounts.Cast<AccountInfo>().OrderBy(x => x.AccountID).FirstOrDefault(x => x.AccountSecondaryContactID == contactInfo.ContactID);
                        if (account == null)
                        {
                            account = contactInfo.Accounts.Cast<AccountInfo>().OrderBy(x => x.AccountID).FirstOrDefault();
                        }
                    }
                }
            }

            return account;
        }

        #endregion

    }

}