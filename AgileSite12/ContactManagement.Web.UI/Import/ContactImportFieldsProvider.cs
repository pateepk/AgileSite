using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.FormEngine;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Class responsible for filtering out contact fields that are not supported by contact import functionality.
    /// </summary>
    public class ContactImportFieldsProvider
    {
        private const string CONTACT_IMPORT_ALTERNATIVE_FORM_NAME = ContactInfo.OBJECT_TYPE + ".CMSImportContacts";
        private const string CONTACT_IMPORT_ALTERNATIVE_FORM_NAME_SIMPLE = ContactInfo.OBJECT_TYPE + ".CMSImportContactsSimple";

        private HashSet<string> mAllowedFieldNames;

        private readonly ILicenseService mLicenseService;

        /// <summary>
        /// Contains data types that are supported by contact import functionality
        /// </summary>
        private static readonly HashSet<string> SupportedDataTypes = new HashSet<string>()
        {
            FieldDataType.Text,
            FieldDataType.LongText,
            FieldDataType.Integer,
            FieldDataType.LongInteger
        };


        internal ContactImportFieldsProvider(ILicenseService licenseService)
        {
            mLicenseService = licenseService;
        }


        /// <summary>
        /// Creates instance.
        /// </summary>
        public ContactImportFieldsProvider() : this(ObjectFactory<ILicenseService>.StaticSingleton())
        {
        }



        /// <summary>
        /// Checks whether contact field is available for import.
        /// </summary>
        /// <param name="fieldName">Name of contact field</param>
        /// <returns>True when field is supported</returns>
        public bool IsFieldAvailableForImport(string fieldName)
        {
            var fields = GetCachedAllowedFieldNames();
            return fields.Contains(fieldName);
        }


        /// <summary>
        /// Contains contact field that are available for import.
        /// </summary>
        private HashSet<string> GetCachedAllowedFieldNames()
        {
            return mAllowedFieldNames ?? (
                mAllowedFieldNames = GetCategoriesAndFieldsAvailableForImport()
                .SelectMany(kvp => kvp.Value)
                .Select(f => f.Name)
                .ToHashSetCollection());            
        }


        /// <summary>
        /// Gets fields and categories from "om.contact.CMSImportContacts" alternative form without fields that are not supported by contact import (eg. DateTime).
        /// <remarks>Supported data types are text, longtext, integer, longinteger.</remarks>
        /// <remarks>Categories without visible fields are not returned.</remarks>
        /// <remarks>Hidden categories with visible fields are not returned.</remarks>
        /// </summary>
        /// <returns>Categories and fields that are available for contact import.</returns>
        /// <remarks>For instances running on lower than <see cref="FeatureEnum.FullContactManagement"/> it return "om.contact.CMSImportContactsSimple" alternative form</remarks>
        public Dictionary<FormCategoryInfo, List<FormFieldInfo>> GetCategoriesAndFieldsAvailableForImport()
        {
            var contactImportFieldsAlternativeForm = GetContactImportFieldsAlternativeForm();
            var fields = contactImportFieldsAlternativeForm.GetHierarchicalFormElements(IsFieldAvailableForImport);

            // Remove hidden categories and visible categories without any visible field
            return fields.Where(pair => pair.Key.Properties["visible"].ToBoolean(true) && pair.Value.Exists(field => field.Visible)).ToDictionary(pair => pair.Key, pair => pair.Value);
        }


        private FormInfo GetContactImportFieldsAlternativeForm()
        {
            string name = InstanceHasFullContactManagementLicense() ? CONTACT_IMPORT_ALTERNATIVE_FORM_NAME : CONTACT_IMPORT_ALTERNATIVE_FORM_NAME_SIMPLE;
            return FormHelper.GetFormInfo(name, false);
        }


        private bool InstanceHasFullContactManagementLicense()
        {
            return mLicenseService.IsFeatureAvailable(FeatureEnum.FullContactManagement);
        }


        /// <summary>
        /// Decides whether given <see cref="FormFieldInfo"/> should be available for import.
        /// </summary>
        private bool IsFieldAvailableForImport(FormFieldInfo formFieldInfo)
        {
            return !formFieldInfo.IsDummyField
                   && formFieldInfo.Visible
                   && SupportedDataTypes.Contains(formFieldInfo.DataType.ToLowerCSafe());
        }
    }
}
