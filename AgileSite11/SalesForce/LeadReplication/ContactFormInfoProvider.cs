using System.Collections.Generic;
using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides contact form info suitable for mapping.
    /// </summary>
    public sealed class ContactFormInfoProvider
    {

        #region "Private members"

        private Dictionary<string, string> mFieldCaptions = new Dictionary<string, string> {
            { "ContactFirstName", "{$om.contact.firstname$}" },
            { "ContactMiddleName", "{$om.contact.middlename$}" },
            { "ContactLastName", "{$om.contact.lastname$}" },
            { "ContactJobTitle", "{$om.contact.jobtitle$}" },
            { "ContactAddress1", "{$om.contact.address1$}" },
            { "ContactCity", "{$general.city$}" },
            { "ContactZIP", "{$general.zip$}" },
            { "ContactMobilePhone", "{$om.contact.mobilephone$}" },
            { "ContactBusinessPhone", "{$om.contact.businessphone$}" },
            { "ContactEmail", "{$general.email$}" },
            { "ContactBirthday", "{$om.contact.birthday$}" },
            { "ContactNotes", "{$om.contact.notes$}" },
            { "ContactCampaign", "{$analytics.campaign$}"},
            { "ContactMonitored", "{$om.contact.tracking$}"}
        };

        private HashSet<string> mExcludedFieldNames = new HashSet<string> {
            "ContactID",
            "ContactOwnerUserID",
            "ContactGUID",
            "ContactLastModified",
            "ContactCreated",
            "ContactBounces",
            "ContactCountryID",
            "ContactGender",
            "ContactStatusID",
            "ContactStateID",
            "ContactCompanyName"
        };

        #endregion

        #region "Public methods"

        /// <summary>
        /// Creates a new instance of the contact form info suitable for mapping, and returns it.
        /// </summary>
        /// <returns>A new instance of the contact form info suitable for mapping.</returns>
        public FormInfo GetFormInfo()
        {
            FormInfo form = FormHelper.GetFormInfo("OM.Contact", true);
            List<IDataDefinitionItem> fields = new List<IDataDefinitionItem>();
            foreach (FormFieldInfo field in form.GetFields(true, false))
            {
                if (!mExcludedFieldNames.Contains(field.Name))
                {
                    string caption = null;
                    if (mFieldCaptions.TryGetValue(field.Name, out caption))
                    {
                        field.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, caption);
                    }
                    fields.Add(field);
                }
            }
            form.ItemsList.Clear();
            form.ItemsList.AddRange(fields);

            return form;
        }

        #endregion

    }

}