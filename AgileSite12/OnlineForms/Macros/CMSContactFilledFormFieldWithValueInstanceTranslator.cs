using System;
using System.Linq;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Translates ContactHasEnteredASpecificValueInASpecificFormSFieldInTheLastXDays Macro rule.
    /// {_perfectum}Contact.FilledFormFieldWithValue("{field}", "{value}", {days});
    /// </summary>
    internal class CMSContactFilledFormFieldWithValueInstanceTranslator : SingleMacroRuleInstanceTranslatorBase
    {
        protected override ObjectQuery<ContactInfo> TranslateInternal(StringSafeDictionary<MacroRuleParameter> ruleParameters)
        {
            string perfectum = ruleParameters["_perfectum"].Value.ToString(string.Empty);
            string formAndFormField = ruleParameters["field"].Value.ToString(string.Empty);
            string expectedValue = ruleParameters["value"].Value.ToString(string.Empty);
            int days = ruleParameters["days"].Value.ToInteger(0);

            var split = ValidationHelper.GetString(formAndFormField, string.Empty).Split(';');

            if (split.Length != 3)
            {
                throw new ArgumentException("Parameter 'field' is expected in form <site name>;<form name>;<form field name>", "ruleParameters");
            }

            string siteName = split[0];
            string formName = split[1];
            string fieldName = split[2];

            if (!ValidationHelper.IsCodeName(siteName))
            {
                throw new ArgumentException("siteName is not valid code name", "ruleParameters");
            }

            if (!ValidationHelper.IsCodeName(formName))
            {
                throw new ArgumentException("formName is not valid code name", "ruleParameters");
            }

            if (!ValidationHelper.IsCodeName(fieldName))
            {
                throw new ArgumentException("fieldName name is not valid code name", "ruleParameters");
            }

            var onlineFormInfo = BizFormInfoProvider.GetBizFormInfo(formName, siteName);

            if (onlineFormInfo == null)
            {
                throw new ArgumentException(string.Format("Form with given name '{0}' cannot be found.", formName), "ruleParameters");
            }

            var classInfo = DataClassInfoProvider.GetDataClassInfo(onlineFormInfo.FormClassID);
            var formInfo = new FormInfo(classInfo.ClassFormDefinition);

            if (formInfo.GetFields<FormFieldInfo>().All(x => x.Name != fieldName))
            {
                throw new ArgumentException(string.Format("Form '{0}' does not contains given field '{1}'.", formName, fieldName), "ruleParameters");
            }

            if (!formInfo.GetFields<FormFieldInfo>().Any(f => (f.DataType == FieldDataType.Text) && (f.Name == fieldName)))
            {
                throw new ArgumentException("Only text fields are allowed.", "ruleParameters");
            }

            var formIdColumnName = formInfo.GetFields<FormFieldInfo>().First(x => x.PrimaryKey).Name;

            var formRecords = BizFormItemProvider.GetItems(classInfo.ClassName)
                                                 .Column(formIdColumnName)
                                                 .WhereContains(fieldName, expectedValue);

            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                                                 .WhereEquals("ActivityItemID", onlineFormInfo.FormID)
                                                 .WhereIn("ActivityItemDetailID", formRecords)
                                                 .OnSite(onlineFormInfo.FormSiteID)
                                                 .Column("ActivityContactID")
                                                 .GroupBy("ActivityContactID");
            if (days > 0)
            {
                activities.NewerThan(TimeSpan.FromDays(days));
            }

            var contacts = ContactInfoProvider.GetContacts();
            
            if (perfectum == "!")
            {
                contacts.WhereNotIn("ContactID", activities);
            }
            else
            {
                contacts.WhereIn("ContactID", activities);
            }

            return contacts;
        }
    }
}