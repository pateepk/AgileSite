using System;
using System.Globalization;
using System.Linq;

using CMS;
using CMS.Activities;
using CMS.Activities.Internal;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.OnlineForms;
using CMS.Core;
using CMS.Core.Internal;

[assembly: RegisterExtension(typeof(ContactOnlineFormsMacroMethods), typeof(ContactInfo))]
namespace CMS.OnlineForms
{
    internal class ContactOnlineFormsMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if the contact filled a form with the given field at least once so that the value contains expected value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact filled a form with the given field at least once so that the value contains expected value.", 3)]
        [MacroMethodParam(0, "contact", typeof(ContactInfo), "Contact info object.")]
        [MacroMethodParam(1, "formNameField", typeof(string), "Form name combined with form field name as '&lt;site name&gt;;&lt;form name&gt;;&lt;form field name&gt;'.")]
        [MacroMethodParam(2, "expectedValue", typeof(string), "Expected value.")]
        [MacroMethodParam(3, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object FilledFormFieldWithValue(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 3 || parameters.Length > 4)
            {
                throw new NotSupportedException();
            }

            var split = ValidationHelper.GetString(parameters[1], string.Empty).Split(';');

            if (split.Length != 3)
            {
                throw new ArgumentException("Second parameter is expected in form <site name>;<form name>;<form field name>");
            }

            string siteName = split[0];
            string formName = split[1];
            string formField = split[2];
            string expectedValue = ValidationHelper.GetString(parameters[2], string.Empty);

            try
            {
                return FilledFormFieldWithValue(parameters[0] as ContactInfo, siteName, formName, formField, expectedValue, parameters.Length == 4 ? ValidationHelper.GetInteger(parameters[3], 0) : 0);
            }
            catch (ArgumentException ex)
            {
                Service.Resolve<IEventLogService>().LogException("FilledFormFieldWithValue", "RESOLVEMACRO", ex, new LoggingPolicy(TimeSpan.FromMinutes(10)));
            }

            return false;
        }


        /// <summary>
        /// Returns true if the contact submitted specified form.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if the contact submitted specified form.", 2)]
        [MacroMethodParam(0, "contact", typeof(object), "Contact info object.")]
        [MacroMethodParam(1, "formName", typeof(string), "Name of the on-line form.")]
        [MacroMethodParam(2, "lastXDays", typeof(int), "Constraint for last X days (if zero or negative value is given, no constraint is applied).")]
        public static object SubmittedForm(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return SubmittedForm(parameters[0], ValidationHelper.GetString(parameters[1], ""), 0);

                case 3:
                    return SubmittedForm(parameters[0], ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetInteger(parameters[2], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if the contact filled a form with the given field at least once so that the value contains expected value.
        /// </summary>
        /// <param name="contact">Contact info</param>
        /// <param name="siteName">Name of site the macro is evaluated for</param>
        /// <param name="formName">Form name</param>
        /// <param name="fieldName">Filed name</param>
        /// <param name="expectedValue">Expected value</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        internal static bool FilledFormFieldWithValue(ContactInfo contact, string siteName, string formName, string fieldName, string expectedValue, int lastXDays)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (!ValidationHelper.IsCodeName(formName))
            {
                throw new ArgumentException("Is not valid code name", "formName");
            }

            if (!ValidationHelper.IsCodeName(fieldName))
            {
                throw new ArgumentException("Is not valid code name", "fieldName");
            }

            var onlineFormInfo = BizFormInfoProvider.GetBizFormInfo(ValidationHelper.GetCodeName(formName), siteName);

            if (onlineFormInfo == null)
            {
                throw new ArgumentException(string.Format("Form with given name '{0}' cannot be found.", formName), "formName");
            }

            var classInfo = DataClassInfoProvider.GetDataClassInfo(onlineFormInfo.FormClassID);
            var formInfo = new FormInfo(classInfo.ClassFormDefinition);

            if (formInfo.GetFields<FormFieldInfo>().All(x => x.Name != fieldName))
            {
                throw new ArgumentException(string.Format("Form '{0}' does not contains given field '{1}'.", formName, fieldName), "fieldName");
            }

            if (!formInfo.GetFields<FormFieldInfo>().Any(f => (f.DataType == FieldDataType.Text) && (f.Name == fieldName)))
            {
                throw new ArgumentException("Only text fields are allowed.", "fieldName");
            }

            var formRecordsIds = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityContactID", contact.ContactID)
                                                 .WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                                                 .WhereEquals("ActivityItemID", onlineFormInfo.FormID)
                                                 .OnSite(onlineFormInfo.FormSiteID)
                                                 .Column("ActivityItemDetailID");

            if (lastXDays > 0)
            {
                formRecordsIds.NewerThan(TimeSpan.FromDays(lastXDays));
            }

            var formIdColumnName = formInfo.GetFields<FormFieldInfo>().First(x => x.PrimaryKey).Name;

            return BizFormItemProvider.GetItems(classInfo.ClassName)
                                      .WhereIn(formIdColumnName, formRecordsIds)
                                      .WhereContains(fieldName, expectedValue)
                                      .Any();
        }


        /// <summary>
        /// Returns true if the contact submitted specified form.
        /// </summary>
        /// <param name="contact">Contact which should be checked</param>
        /// <param name="formName">Form name</param>
        /// <param name="lastXDays">Constraint for last X days (if zero or negative value is given, no constraint is applied)</param>
        private static bool SubmittedForm(object contact, string formName, int lastXDays)
        {
            var contactInfo = contact as ContactInfo;
            if (contactInfo == null)
            {
                return false;
            }

            var formIDs = BizFormInfoProvider.GetBizForms()
                .WhereEquals("FormName", formName)
                .AsIDQuery();

            var activity = ActivityInfoProvider.GetActivities()
                .WhereEquals("ActivityContactID", contactInfo.ContactID)
                .WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                .WhereIn("ActivityItemID", formIDs);

            if (lastXDays > 0)
            {
                var dateTimeNow = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                activity = activity.Where("ActivityCreated >= cast('" + dateTimeNow.AddDays(-lastXDays).ToString(CultureInfo.InvariantCulture) + "' as datetime)");
            }

            return activity.Count > 0;
        }
    }
}
