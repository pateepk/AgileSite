using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.FormEngine;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.MacroEngine;
using CMS.ContactManagement.Web.UI.Internal;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Provides service methods used in <see cref="ContactDetailsController"/>.
    /// </summary>
    internal class ContactDetailsControllerService : IContactDetailsControllerService
    {
        internal readonly IDictionary<string, IContactDetailsFieldResolver> mResolvers = new Dictionary<string, IContactDetailsFieldResolver>();
        private readonly IEventLogService mEventLogService;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactDetailsControllerService"/>.
        /// </summary>
        /// <param name="eventLogService">Service for logging messages to the event log</param>
        public ContactDetailsControllerService(IEventLogService eventLogService)
        {
            mEventLogService = eventLogService;
        }


        /// <summary>
        /// Registers given <paramref name="fieldResolver"/> for given <paramref name="fieldName"/>. Once the field with <paramref name="fieldName"/> will be 
        /// proceeded, result of <paramref name="fieldResolver"/> will be used as the field value.
        /// </summary>
        /// <param name="fieldName">Name of the field the <paramref name="fieldResolver"/> is registered for</param>
        /// <param name="fieldResolver">Implementation fo <see cref="IContactDetailsFieldResolver"/> resolving the value for given <paramref name="fieldName"/></param>
        public void RegisterContactDetailsFieldResolver(string fieldName, IContactDetailsFieldResolver fieldResolver)
        {
            mResolvers.Add(fieldName, fieldResolver);
        }


        /// <summary>
        /// Gets collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact the collection of <see cref="ContactDetailsViewModel"/> is obtained for</param>
        /// <returns>Collection of <see cref="ContactDetailsViewModel"/> for the given <paramref name="contactID"/></returns>
        public IEnumerable<ContactDetailsViewModel> GetContactDetailsViewModel(int contactID)
        {
            var contact = ContactInfoProvider.GetContactInfo(contactID);
            var visibleFormFields = GetVisibleFormFields();
            var macroResolver = MacroResolver.GetInstance();

            foreach (var visibleField in visibleFormFields)
            {
                string fieldName = visibleField.Name;
                object detailValue = GetFieldValue(contact, fieldName);

                if (detailValue is string)
                {
                    detailValue = detailValue.ToString().Trim();
                }

                if (!IsValueAvailable(detailValue))
                {
                    continue;
                }

                yield return new ContactDetailsViewModel
                {
                    FieldValue = detailValue,
                    FieldCaption = visibleField.GetPropertyValue(FormFieldPropertyEnum.FieldCaption, macroResolver, resolveAllMacros: true),
                    FieldName = fieldName,
                    FieldType = detailValue.GetType().Name
                };
            }
        }


        /// <summary>
        /// Gets field value of given <paramref name="contact"/> for given <paramref name="fieldName"/>. If some <see cref="IContactDetailsFieldResolver"/> was registered for
        /// <paramref name="fieldName"/>, resolver is used for obtaining the value. If dependency for <paramref name="fieldName "/>is specified in <see cref="ContactInfo.TYPEINFO"/>,
        /// related object is loaded and its <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectDisplayName"/> is returned. Otherwise returns value of the field within <paramref name="contact"/>.
        /// </summary>
        /// <param name="contact">Contact the field value is obtained for</param>
        /// <param name="fieldName">Name of the field the value is obtained for</param>
        /// <returns>Desired field value of <paramref name="contact"/> identified by given <paramref name="fieldName"/></returns>
        private object GetFieldValue(ContactInfo contact, string fieldName)
        {
            if (mResolvers.ContainsKey(fieldName))
            {
                return mResolvers[fieldName].ResolveField(contact);
            }
            
            object fieldValue = contact.GetValue(fieldName);
            
            var objectDepencency = ContactInfo.TYPEINFO.DependsOn.SingleOrDefault(dependency => dependency.DependencyColumn == fieldName);
            if (objectDepencency == null || !(fieldValue is int))
            {
                return fieldValue;
            }

            var relatedObject = ProviderHelper.GetInfoById(objectDepencency.DependencyObjectType, (int)fieldValue);
            if (relatedObject == null)
            {
                mEventLogService.LogEvent(EventType.ERROR, "Contact details", "GET", string.Format("No related object of type '{0}' was found for the ID '{1}'", objectDepencency.DependencyObjectType, fieldValue));
                return fieldValue;
            }
            return relatedObject.Generalized.ObjectDisplayName;
        }

        
        /// <summary>
        /// Determines whether the given <paramref name="detailValue"/> is available or not. 
        /// </summary>
        /// <param name="detailValue">Value to be determined</param>
        /// <returns><c>False</c> is <paramref name="detailValue"/> is <c>null</c> or type of the <paramref name="detailValue"/> is <see cref="string"/> and the value is empty; otherwise, <c>true</c></returns>
        private bool IsValueAvailable(object detailValue)
        {
            return !(detailValue == null || (detailValue is string && string.IsNullOrEmpty(detailValue.ToString())));
        }


        /// <summary>
        /// Gets all visible field defined in the ContactProfile <see cref="AlternativeFormInfo"/>.
        /// </summary>
        /// <returns>Collection of visible form fields obtained from the <see cref="AlternativeFormInfo"/></returns>
        private IEnumerable<FormFieldInfo> GetVisibleFormFields()
        {
            var alternativeForm = AlternativeFormInfoProvider.GetAlternativeFormInfo(string.Format("{0}.ContactProfile", ContactInfo.OBJECT_TYPE));
            var dataClassForm = DataClassInfoProvider.GetDataClassInfo(alternativeForm.FormClassID);
            string mergedFormDefinition = FormHelper.MergeFormDefinitions(dataClassForm.ClassFormDefinition, alternativeForm.FormDefinition);

            var form = new FormInfo(mergedFormDefinition);

            // ContactCampaign needs to be displayed even though it is hidden in edit form
            return form.ItemsList.OfType<FormFieldInfo>().Where(formField => formField.Visible || formField.Name == "ContactCampaign");
        }
    }
}