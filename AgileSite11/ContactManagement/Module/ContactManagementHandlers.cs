using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Activities.Internal;
using CMS.Automation;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OnlineMarketing.Internal;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact management events handlers.
    /// </summary>
    internal class ContactManagementHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            ActivityEvents.ActivityBulkInsertPerformed.Before += UpdateActivitiesWithCorrectContactID;
            ActivityEvents.ActivityBulkInsertPerformed.After += AfterActivityBulkInserted;

            ActivityEvents.ActivityProcessedInLogService.Execute += UpdateContactFromUser;

            ContactManagementEvents.ProcessContactActionsBatch.Before += MergeContacts;
            ContactManagementEvents.ProcessContactActionsBatch.After += RunTriggers;

            ActivityInfo.TYPEINFO.Events.Insert.After += StoresActivityForFurtherProcessing;

            // When email campaign activity is logged, current contact could be different than the one activity was logged for.
            // Merge these contacts when they meet specific conditions (e.g. they don't represent different visitors).
            // Since the current contact is used the merging must be done during the request in the context of logged activity.
            ActivityEvents.ActivityProcessedInLogService.Execute += MergeCurrentAnonymousContactWithGivenContact;
        }


        internal static void UpdateActivitiesWithCorrectContactID(object sender, CMSEventArgs<IList<IActivityInfo>> e)
        {
            var activities = e.Parameter;

            // Order GUIDs to improve select performance on OM_VisitorToContact table, there is non-clustered index upon VisitorToContactVisitorGUID column
            var guids = activities.Select(a => a.ActivityContactGUID).OrderBy(g => g).ToList();
            if (!guids.Any())
            {
                return;
            }

            var query = VisitorToContactInfoProvider.GetVisitorToContacts();

            if (query.Parameters == null)
            {
                query.Parameters = new QueryDataParameters();
            }

            using (var selectCondition = new SelectCondition(query.Parameters))
            {
                selectCondition.InlineLimit = SelectCondition.ALL_TABLE_VALUED_PARAMETER;

                // Prepare select condition with GUID table-valued parameter
                selectCondition.PrepareCondition("VisitorToContactVisitorGUID", guids);
                var visitorToContacts = query.Where(selectCondition.WhereCondition).ToList();

                foreach (var visitorToContact in visitorToContacts)
                {
                    UpdateActivitiesWithGivenContactToVisitor(activities, visitorToContact);
                }
            }
        }


        private static void UpdateActivitiesWithGivenContactToVisitor(IList<IActivityInfo> activities, VisitorToContactInfo visitorToContact)
        {
            foreach (var activity in activities.Where(a => a.ActivityContactGUID == visitorToContact.VisitorToContactVisitorGUID))
            {
                activity.ActivityContactID = visitorToContact.VisitorToContactContactID;
            }
        }


        /// <summary>
        /// Merges current contact with contact given in <paramref name="e"/> when current contact does not have email address filled.
        /// Contact should not be impersonated in case the email was forwarded to known contact.
        /// </summary>
        private static void MergeCurrentAnonymousContactWithGivenContact(object sender, CMSEventArgs<IActivityInfo> e)
        {
            var activity = e.Parameter;

            if ((activity.ActivityType == PredefinedActivityType.NEWSLETTER_CLICKTHROUGH ||
               activity.ActivityType == PredefinedActivityType.NEWSLETTER_SUBSCRIBING) && Service.Resolve<IContactProcessingChecker>().CanProcessContactInCurrentContext())
            {
                var activityContact = ContactInfoProvider.GetContactInfo(activity.ActivityContactID);
                if (activityContact != null && !string.IsNullOrEmpty(activityContact.ContactEmail))
                {
                    Service.Resolve<ICurrentContactMergeService>().MergeCurrentContactWithContact(activityContact);
                }
            }
        }


        /// <summary>
        /// Calls methods to perform after activities are processed with bulk insert.
        /// </summary>
        private static void AfterActivityBulkInserted(object sender, CMSEventArgs<IList<IActivityInfo>> e)
        {
            var activities = e.Parameter;

            TouchActivityDependencyCacheKeys(activities);
            StoresActivityForFurtherProcessingAfterBulkInsert(activities);

            foreach (var activity in activities)
            {
                ForumPostUpdateContactsInformations(activity);
                UpdateContactFromMessageBoard(activity);
                UpdateContactFromBlog(activity);
            }
        }


        /// <summary>
        /// Updates <see cref="ContactInfo"/> with the data provided by user in message boards.
        /// </summary>
        private static void UpdateContactFromMessageBoard(IActivityInfo activity)
        {
            if (activity.ActivityType != PredefinedActivityType.MESSAGE_BOARD_COMMENT)
            {
                return;
            }

            var boardMessageInfo = new ObjectQuery(PredefinedObjectType.BOARDMESSAGE).WhereID("MessageID", activity.ActivityItemDetailID)
                                                                                     .FirstOrDefault();

            if (boardMessageInfo == null)
            {
                return;
            }

            var contactData = new Dictionary<string, object>();
            contactData.Add("ContactLastName", boardMessageInfo.GetStringValue("MessageUserName", string.Empty));
            ContactInfoProvider.UpdateContactFromExternalSource(contactData, false, activity.ActivityContactID);
        }


        /// <summary>
        /// Updates <see cref="ContactInfo"/> with the data provided by contact in blogs.
        /// </summary>        
        private static void UpdateContactFromBlog(IActivityInfo activity)
        {
            if (activity.ActivityType != PredefinedActivityType.BLOG_COMMENT)
            {
                return;
            }

            var blogCommentInfo = new ObjectQuery(PredefinedObjectType.BLOGCOMMENT).WhereID("CommentID", activity.ActivityItemID)
                                                                                   .FirstOrDefault();

            if (blogCommentInfo == null)
            {
                return;
            }

            var contactData = new Dictionary<string, object>();
            contactData.Add("ContactLastName", blogCommentInfo.GetStringValue("CommentUserName", string.Empty));
            ContactInfoProvider.UpdateContactFromExternalSource(contactData, false, activity.ActivityContactID);
        }


        /// <summary>
        /// Updates contact's email and last name.
        /// </summary>
        private static void ForumPostUpdateContactsInformations(IActivityInfo activity)
        {
            if (activity.ActivityType != PredefinedActivityType.FORUM_POST)
            {
                return;
            }

            var forumPostInfo = new ObjectQuery(PredefinedObjectType.FORUMPOST).WhereID("PostID", activity.ActivityItemDetailID)
                                                                               .FirstOrDefault();

            if (forumPostInfo == null)
            {
                return;
            }

            var contactData = new Dictionary<string, object>();
            contactData.Add("ContactLastName", forumPostInfo.GetStringValue("PostUserName", string.Empty));
            ContactInfoProvider.UpdateContactFromExternalSource(contactData, false, activity.ActivityContactID);
        }


        /// <summary>
        /// Updates data in contact object when user is created on Live site.
        /// </summary>
        private static void UpdateContactFromUser(object sender, CMSEventArgs<IActivityInfo> e)
        {
            var activity = e.Parameter;
            if (activity.ActivityType == PredefinedActivityType.REGISTRATION)
            {
                var user = UserInfoProvider.GetUserInfo(activity.ActivityItemID);
                var classInfo = DataClassInfoProvider.GetDataClassInfo(user.TypeInfo.ObjectClassName);
                var mapper = new ContactDataMapper(classInfo.ClassName, classInfo.ClassContactOverwriteEnabled);
                var checker = new UserContactDataPropagationChecker();

                Service.Resolve<IContactDataInjector>().Inject(user, activity.ActivityContactID, mapper, checker);
            }
        }


        /// <summary>
        /// Stores all the activities from performed bulk insert to the queue for further processing.
        /// </summary>
        internal static void StoresActivityForFurtherProcessingAfterBulkInsert(IList<IActivityInfo> activities)
        {
            var activityQueue = Service.Resolve<IActivityQueueRecalculationProvider>();
            activityQueue.StoreRange(activities);
        }


        /// <summary>
        /// Touches the cache related to the <see cref="ActivityInfo"/> that were inserted in bulk to the database.
        /// </summary>
        private static void TouchActivityDependencyCacheKeys(IList<IActivityInfo> activities)
        {
            List<string> cacheKeys = new List<string>();
            cacheKeys.Add(PredefinedObjectType.ACTIVITY + "|all");

            var contactIDs = new HashSet<int>();
            contactIDs.AddRange(activities.Select(activity => activity.ActivityContactID));

            foreach (var contactID in contactIDs)
            {
                cacheKeys.Add(string.Format("{0}|byid|{1}|children", PredefinedObjectType.CONTACT, contactID));
                cacheKeys.Add(string.Format("{0}|byid|{1}|children|{2}", PredefinedObjectType.CONTACT, contactID, PredefinedObjectType.ACTIVITY));
            }

            CacheHelper.TouchKeys(cacheKeys);
        }


        /// <summary>
        /// Executes when individual activity is inserted.
        /// </summary>
        private static void StoresActivityForFurtherProcessing(object sender, ObjectEventArgs e)
        {
            var info = (ActivityInfo)e.Object;
            var activityQueue = Service.Resolve<IActivityQueueRecalculationProvider>();

            activityQueue.Store(new ActivityDto(info));
        }


        /// <summary>
        /// Runs marketing automation triggers connected to activities.
        /// </summary>
        private static void RunTriggers(object sender, ProcessContactActionsBatchEventArgs e)
        {
            RunActivityTriggers(e.LoggedActivities.Cast<ActivityDto>().Select(activity => activity.ToActivityInfo()));
            RunContactTriggers(e.LoggedContactChanges);
        }


        private static void RunContactTriggers(IList<ContactChangeData> contactChanges)
        {
            if (!TriggerHelper.HasTriggerTypes(ContactInfo.OBJECT_TYPE))
            {
                return;
            }

            var notMergedContactChanges = contactChanges.Where(x => !x.ContactWasMerged).ToList();
            var contactIds = notMergedContactChanges.Select(x => x.ContactID).ToHashSet();

            // Get all affected contacts
            var contactsById = ContactInfoProvider.GetContacts().WhereIn("ContactID", contactIds).ToDictionary(c => c.ContactID);

            // Create trigger options only for existing contacts
            var triggerOptions = notMergedContactChanges.Where(change => contactsById.ContainsKey(change.ContactID))
                                                        .Select(change => PrepareTriggerOptions(contactsById[change.ContactID], change.ContactIsNew));

            TriggerHelper.ProcessTriggers(triggerOptions);
        }


        private static TriggerOptions PrepareTriggerOptions(ContactInfo contact, bool contactIsNew)
        {
            // Create custom resolver to set proper "Contact" object in macros - by default CurrentContact is used
            var resolver = MacroResolver.GetInstance().CreateChild();
            resolver.SetNamedSourceData("Contact", contact);

            return new TriggerOptions
            {
                Resolver = resolver,
                ObjectType = contact.TypeInfo.ObjectType,
                Types = new List<WorkflowTriggerTypeEnum>
                {
                    contactIsNew ? WorkflowTriggerTypeEnum.Creation : WorkflowTriggerTypeEnum.Change
                },
                Info = contact
            };
        }


        private static void RunActivityTriggers(IEnumerable<ActivityInfo> activities)
        {
            if (!TriggerHelper.HasTriggerTypes(ActivityInfo.OBJECT_TYPE))
            {
                return;
            }

            // The trigger and other actions (e.g. CMSWorkerQueue) should not create other threads. 
            // These actions are called  outside of the request and there is no mechanism that would process them. 
            // This behavior is achieved by synchronous evaluation of MA process.
            using (new WorkflowActionContext { AllowAsyncActions = false })
            {
                foreach (var activitiesGroup in activities.GroupBy(a => a.ActivitySiteID))
                {
                    // Some marketing automation actions (such as SendNewsletterIssueAction) rely on SiteContext, so it has to be manually filled before processing triggers.
                    // Other place where actions are run is from scheduled tasks where SiteContext is set in the same way by SchedulingExecutor.
                    var site = SiteInfoProvider.GetSiteInfo(activitiesGroup.Key);
                    if (site == null)
                    {
                        continue;
                    }

                    string originalSiteName = SiteContext.CurrentSiteName;
                    SiteContext.CurrentSiteName = site.SiteName;

                    try
                    {
                        ProcessActivityTriggers(activitiesGroup);
                    }
                    finally
                    {
                        SiteContext.CurrentSiteName = originalSiteName;
                    }
                }
            }
        }


        private static void ProcessActivityTriggers(IEnumerable<ActivityInfo> activities)
        {
            var options = activities.Select(activity =>
            {
                // Options lazy loading
                var contact = ContactInfoProvider.GetContactInfo(activity.ActivityContactID);

                // Skip execution when contact was not found (it could have been deleted in the meantime)
                if (contact == null)
                {
                    return null;
                }

                var activityType = ActivityTypeInfoProvider.GetActivityTypeInfo(activity.ActivityType);

                var resolver = MacroResolver.GetInstance();
                resolver.SetNamedSourceData("Contact", contact);
                resolver.SetNamedSourceData("Activity", activity);
                resolver.SetNamedSourceData("ActivityType", activityType);

                var additionalData = new StringSafeDictionary<object>
                {
                    { TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEMID, activity.ActivityItemID },
                    { TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEM_DETAILID, activity.ActivityItemDetailID},
                    { TriggerDataConstants.TRIGGER_DATA_ACTIVITY_VALUE, activity.ActivityValue },
                    { TriggerDataConstants.TRIGGER_DATA_ACTIVITY_SITEID, activity.ActivitySiteID }
                };

                var opts = new TriggerOptions
                {
                    Resolver = resolver,
                    ObjectType = ActivityInfo.OBJECT_TYPE,
                    Types = new List<WorkflowTriggerTypeEnum> { WorkflowTriggerTypeEnum.Creation },
                    Info = contact,
                    PassFunction = info => (info.TriggerTargetObjectID == 0) || (activityType.ActivityTypeID == info.TriggerTargetObjectID),
                    AdditionalData = additionalData
                };

                return opts;
            }).Where(o => o != null);

            TriggerHelper.ProcessTriggers(options);
        }


        private static void MergeContacts(object sender, ProcessContactActionsBatchEventArgs e)
        {
            var contactMergeService = Service.Resolve<IContactMergeService>();
            var contactIdsWithChangedEmail = GetContactIdsWithChangedEmail(e.LoggedContactChanges);

            if (contactIdsWithChangedEmail.Any())
            {
                var contactsWithChangedEmail = GetContacts(contactIdsWithChangedEmail);

                foreach (var contact in RemoveDuplicatedEmails(contactsWithChangedEmail))
                {
                    contactMergeService.MergeContactByEmail(contact);
                }
            }
        }


        private static IList<int> GetContactIdsWithChangedEmail(IEnumerable<ContactChangeData> contactChanges)
        {
            return contactChanges.Where(change => change.ChangedColumns != null && change.ChangedColumns.Contains("ContactEmail"))
                                 .Select(change => change.ContactID)
                                 .ToList();
        }


        private static ObjectQuery<ContactInfo> GetContacts(IList<int> contactIDs)
        {
            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactID", contactIDs)
                                      .WhereNotEmpty("ContactEmail");
        }


        private static IEnumerable<ContactInfo> RemoveDuplicatedEmails(ObjectQuery<ContactInfo> contactsWithChangedEmail)
        {
            return contactsWithChangedEmail.ToList()
                                           .GroupBy(c => c.ContactEmail)
                                           .Select(c => c.First());
        }
    }
}
