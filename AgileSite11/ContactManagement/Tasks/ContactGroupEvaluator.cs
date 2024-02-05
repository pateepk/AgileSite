using System;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Scheduler;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Task for evaluation of dynamic contact group.
    /// Contacts' (site or all for global contact group) membership to a contact group depends on meeting the dynamic condition.
    /// </summary>
    public class ContactGroupEvaluator : ITask
    {
        #region "Properties"

        /// <summary>
        /// Contact group. Optional, should be set if this class is used outside the scheduler.
        /// </summary>
        public ContactGroupInfo ContactGroup
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Evaluates the membership of contacts to given contact group.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            // Check license
            if (InsufficientLicense())
            {
                return String.Format(ResHelper.GetString("licenselimitation.featurenotavailable"), "Dynamic Contact Groups");
            }

            if ((task != null) && task.TaskEnabled)
            {
                // Get GUID from task data
                var groupGuid = ValidationHelper.GetGuid(task.TaskData, Guid.Empty);
                ContactGroup = ContactGroupInfoProvider.GetContactGroups()
                                                       .WhereEquals("ContactGroupGUID", groupGuid)
                                                       .FirstOrDefault();
            }

            if (ContactGroup == null)
            {
                return "Task data not provided.";
            }

            Run();

            return string.Empty;
        }


        private static bool InsufficientLicense()
        {
            return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement, ModuleName.CONTACTMANAGEMENT);
        }


        /// <summary>
        /// Starts contact groups members evaluation.
        /// </summary>
        public void Run()
        {
            if (ContactGroup == null)
            {
                return;
            }

            try
            {
                if (ContactGroup.ContactGroupStatus != ContactGroupStatusEnum.Rebuilding)
                {
                    // Set status that the contact group is being rebuilt
                    ContactGroup.ContactGroupStatus = ContactGroupStatusEnum.Rebuilding;
                    ContactGroupInfoProvider.SetContactGroupInfo(ContactGroup);
                }

                new ContactGroupRebuilder().RebuildGroup(ContactGroup);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("OnlineMarketing", "ContactGroupEvaluator", ex);
            }
            finally
            {
                // Set status that the contact group is ready
                ContactGroup.ContactGroupStatus = ContactGroupStatusEnum.Ready;
                ContactGroupInfoProvider.SetContactGroupInfo(ContactGroup);
            }
        }

        #endregion
    }
}