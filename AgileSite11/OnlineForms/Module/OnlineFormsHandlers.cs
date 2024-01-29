using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.FormEngine;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Synchronization;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Online forms events handlers.
    /// </summary>
    internal class OnlineFormsHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            BizFormInfo.TYPEINFO.Events.Update.Before += UpdateDisplayName;

            AlternativeFormInfo.TYPEINFO.Events.Update.After += LogChange;
            AlternativeFormInfo.TYPEINFO.Events.Insert.After += LogChange;
            AlternativeFormInfo.TYPEINFO.Events.Delete.After += LogChange;

            var formEvents = FormClassInfo.TYPEINFOFORM.Events;

            // To create WF task after inserting new dynamic TypeInfo with possible cached zombie TI on another farms due to updating class name value
            formEvents.Insert.After += Insert;

            formEvents.Update.After += Update;
            formEvents.Delete.After += Delete;

            // Clear data before the delete since the provider object is removed after the delete
            // It needs to be used to properly delete the cached data
            formEvents.Delete.Before += ClearHashtables;

            BizFormItemEvents.Insert.After += LogFormSubmitActivity;

            BizFormItemEvents.Insert.After += UpdateContactFromForm;
            BizFormItemEvents.Update.After += UpdateContactFromForm;
        }


        /// <summary>
        /// Updates data in <see cref="ContactInfo"/> from the submitted <see cref="BizFormItem"/>.
        /// </summary>
        private static void UpdateContactFromForm(object sender, BizFormItemEventArgs e)
        {
            if (ShouldUpdateContact())
            {
                var contactProvider = Service.Resolve<ICurrentContactProvider>();
                var contact = contactProvider.GetCurrentContact(MembershipContext.AuthenticatedUser, false);
                var dataClass = DataClassInfoProvider.GetDataClassInfo(e.Item.BizFormInfo.FormClassID);

                if (string.IsNullOrEmpty(contact.ContactEmail) || dataClass.ClassContactOverwriteEnabled)
                {
                    MergeContactIfEmailWasFilledInForm(e.Item, dataClass, contact);
                }

                ContactInfoProvider.UpdateContactFromExternalData(e.Item, dataClass.ClassContactOverwriteEnabled, contact);
            }
        }


        private static void MergeContactIfEmailWasFilledInForm(BizFormItem bizFormItem, DataClassInfo dataClass, ContactInfo currentContact)
        {
            var emailField = new FormInfo(dataClass.ClassContactMapping).ItemsList.OfType<FormFieldInfo>().FirstOrDefault(item => item.Name == "ContactEmail");
            if (emailField != null)
            {
                string email = bizFormItem.GetStringValue(emailField.MappedToField, string.Empty);
                if (!string.IsNullOrEmpty(email))
                {
                    // In this case we want to merge contacts before we perform contact update. 
                    // Otherwise we could overwrite contacts data even if overwrite is disabled on form.
                    MergeCurrentContactByEmail(currentContact, email);
                }
            }
        }


        private static void MergeCurrentContactByEmail(ContactInfo targetContact, string email)
        {
            var contactMergeService = Service.Resolve<IContactMergeService>();
            targetContact.ContactEmail = email;
            contactMergeService.MergeContactByEmail(targetContact);
        }


        private static bool ShouldUpdateContact()
        {
            return Service.Resolve<ISiteService>().IsLiveSite && CMSHttpContext.Current != null && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement) && Service.Resolve<IContactProcessingChecker>().CanProcessContactInCurrentContext();
        }


        /// <summary>
        /// Logs form submission activity.
        /// </summary>
        private static void LogFormSubmitActivity(object sender, BizFormItemEventArgs e)
        {
            if (Service.Resolve<ISiteService>().IsLiveSite && CMSHttpContext.Current != null)
            {
                if (!e.Item.BizFormInfo.FormLogActivity)
                {
                    return;
                }
                
                var activityLogService = Service.Resolve<IActivityLogService>();
                var formSubmitActivity = SystemContext.IsCMSRunningAsMainApplication ? 
                    new FormSubmitActivityInitializer(e.Item, DocumentContext.CurrentDocument) : 
                    new FormSubmitActivityInitializer(e.Item);
                activityLogService.Log(formSubmitActivity, CMSHttpContext.Current.Request);
            }
        }


        /// <summary>
        /// Handles additional actions when inserting a form
        /// </summary>
        private static void Insert(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = BizFormItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            BizFormItemProvider.InvalidateProvider(objectType);
            // Remove cached TypeInfo
            BizFormItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }


        /// <summary>
        /// Handles additional actions when deleting a form
        /// </summary>
        private static void Delete(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = BizFormItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            BizFormItemProvider.InvalidateProvider(objectType);
            // Remove cached TypeInfo
            BizFormItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }
        

        /// <summary>
        /// Clears data in hashtables
        /// </summary>
        private static void ClearHashtables(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = BizFormItemProvider.GetObjectType(className);

            // Clear data in hashtables since the data structure can be changed
            ProviderHelper.ClearHashtables(objectType, true);
        }


        /// <summary>
        /// Handles additional actions when updating a form
        /// </summary>
        private static void Update(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var className = classInfo.ClassName;
            var objectType = BizFormItemProvider.GetObjectType(className);

            // Invalidate provider since the data structure can be changed
            BizFormItemProvider.InvalidateProvider(objectType);
            // Update cached TypeInfo
            BizFormItemProvider.InvalidateTypeInfo(className, true);
            // Remove read-only object
            ModuleManager.RemoveReadOnlyObject(objectType, true);
        }


        /// <summary>
        /// Updates display name of DataClass when BizFormInfo display name changes.
        /// </summary>
        private static void UpdateDisplayName(object sender, ObjectEventArgs e)
        {
            var bizFormInfo = (BizFormInfo)e.Object;
            if (bizFormInfo == null)
            {
                return;
            }

            if (!bizFormInfo.ChangedColumns().Contains("FormDisplayName"))
            {
                return;
            }

            var classInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            if (classInfo == null)
            {
                return;
            }

            classInfo.ClassDisplayName = bizFormInfo.FormDisplayName;
            DataClassInfoProvider.SetDataClassInfo(classInfo);
        }


        /// <summary>
        /// Logs the change of the biz form for the alternative form event handler
        /// </summary>
        private static void LogChange(object sender, ObjectEventArgs e)
        {
            // Get alternative form
            var altFormInfo = e.Object as AlternativeFormInfo;
            if (altFormInfo == null)
            {
                return;
            }

            // Get data class
            var dci = DataClassInfoProvider.GetDataClassInfo(altFormInfo.FormClassID);
            if ((dci == null) || !dci.ClassIsForm)
            {
                return;
            }

            // Get BizForm
            var bfi = BizFormInfoProvider.GetBizFormInfoForClass(dci.ClassID);
            if (bfi == null)
            {
                return;
            }

            // Required to log staging task, alternative form is not bound to bizform as child
            using (CMSActionContext context = new CMSActionContext())
            {
                context.CreateVersion = false;

                // Log synchronization
                SynchronizationHelper.LogObjectChange(bfi, TaskTypeEnum.UpdateObject);
            }
        }
    }
}
