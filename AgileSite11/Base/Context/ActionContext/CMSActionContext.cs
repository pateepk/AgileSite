using System;
using System.Globalization;
using System.Threading;

using CMS.Core;
using CMS.Base;

[assembly: ThreadRequiredContext(typeof(CMSActionContext))]

namespace CMS.Base
{
    /// <summary>
    /// Ensures context for the actions block.
    /// </summary>
    /// <example>
    /// using(var ctx = new CMSActionContext() { LogEvents = false })
    /// {
    ///     DoSomeAction();
    /// }
    /// </example>
    public sealed class CMSActionContext : AbstractActionContext<CMSActionContext>
    {
        #region "Variables"

        // User of the context.
        private IUserInfo mUser;

        // Indicates whether the currently running code executes on the live site.
        private bool? mIsLiveSite;

        // Site of the context.
        private ISiteInfo mSite;

        // Indicates if the staging tasks should be logged.
        private bool? mLogSynchronization;

        // Indicates if the integration tasks should be logged.
        private bool? mLogIntegration;

        // Indicates whether logging through log context (LogContext) is currently enabled.
        private bool? mEnableLogContext;

        // Indicates if the events should be logged.
        private bool? mLogEvents;

        // Indicates if the web farm tasks should be logged.
        private bool? mLogWebFarmTasks;

        // Indicates if the objects cache dependencies should be touched.
        private bool? mTouchCacheDependencies;

        // Indicates if the objects time stamps should be updated.
        private bool? mUpdateTimeStamp;

        // Indicates if the search task should be created for the objects.
        private bool? mCreateSearchTask;

        // Indicates if the export tasks should be logged.
        private bool? mLogExport;

        // Indicates if the object version should be created.
        private bool? mCreateVersion;

        // Indicates if the actions can run asynchronously.
        private bool? mAllowAsyncActions;

        // CMSThread guid.
        private Guid? mThreadGuid;

        // Indicates if the notifications should be sent.
        private bool? mSendNotifications;

        // Indicates whether a rating should be updated.
        private bool? mUpdateRating;

        // Indicates if the system fields should be updated.
        private bool? mUpdateSystemFields;

        // Indicates if cache should be used for data operations.
        private bool? mUseCacheForSynchronizationXMLs;

        // Indicates if the current user should be initialized.
        private bool? mAllowInitUser;

        // Indicates if the objects within current context should behave as disconnected
        private bool? mDisconnected;

        // Indicates if the objects within current context should touch parent object
        private bool? mTouchParent;

        // Indicates if email should be sent.
        private bool? mSendEmails;

        // Indicates if global admin context should be used.
        private bool? mUseGlobalAdminContext;

        // Indicates whether a search task should be executed when it is created.
        private bool? mEnableSmartSearchIndexer;

        // Indicates whether a physical files should be deleted for attachments, meta files and media files.
        private bool? mDeletePhysicalFiles;

        // Indicates whether a user activity points should be updated.
        private bool? mUpdateUserCounts;

        // Indicates whether a redirect is allowed in case the license check fails. If not, an exception is thrown instead.
        private bool? mAllowLicenseRedirect;

        // Indicates whether a warning is logged when license check for feature availability fails.
        private bool? mLogLicenseWarnings;

        // Indicates whether a license check should be performed when managing objects.
        private bool? mCheckLicence;

        // Indicates whether a get data queries for object that are not allowed under current license returns empty data set or throw an error.
        private bool? mEmptyDataForInvalidLicense; 
        
        // Indicates whether the changes made to the object instance should be reseted.
        private bool? mResetChanges;

        // Original thread culture.
        private CultureInfo mThreadCulture;

        // Original thread UI culture.
        private CultureInfo mThreadUICulture;

        // Indicates if object serialization within continuous integration is allowed in the given context
        private bool? mContinuousIntegrationAllowObjectSerialization;

        #endregion


        #region "Public properties"

        /// <summary>
        /// User of the context.
        /// </summary>
        public IUserInfo User
        {
            set
            {
                // Keep current user
                StoreOriginalValue(ref OriginalData.mUser, CurrentUser);

                // Ensure requested user context
                CurrentUser = value;
            }
        }


        /// <summary>
        /// Indicates whether the currently running code executes on the live site.
        /// </summary>
        public bool IsLiveSite
        {
            set
            {
                StoreOriginalValue(ref OriginalData.mIsLiveSite, CurrentIsLiveSite);

                CurrentIsLiveSite = value;
            }
        }


        /// <summary>
        /// Indicates if the staging tasks should be logged.
        /// </summary>
        public bool LogSynchronization
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogSynchronization, CurrentLogSynchronization);

                // Ensure requested settings
                CurrentLogSynchronization = value;
            }
        }


        /// <summary>
        /// Indicates if the integration tasks should be logged.
        /// </summary>
        public bool LogIntegration
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogIntegration, CurrentLogIntegration);

                // Ensure requested settings
                CurrentLogIntegration = value;
            }
        }


        /// <summary>
        /// Indicates whether logging through log context (LogContext) is currently enabled.
        /// </summary>
        public bool EnableLogContext
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mEnableLogContext, CurrentEnableLogContext);

                // Ensure requested settings
                CurrentEnableLogContext = value;
            }
        }


        /// <summary>
        /// Indicates if the events should be logged.
        /// </summary>
        public bool LogEvents
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogEvents, CurrentLogEvents);

                // Ensure requested settings
                CurrentLogEvents = value;
            }
        }


        /// <summary>
        /// Indicates if the web farm tasks should be logged.
        /// </summary>
        public bool LogWebFarmTasks
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogWebFarmTasks, CurrentLogWebFarmTasks);

                // Ensure requested settings
                CurrentLogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// Indicates if the objects cache dependencies should be touched.
        /// </summary>
        public bool TouchCacheDependencies
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mTouchCacheDependencies, CurrentTouchCacheDependencies);

                // Ensure requested settings
                CurrentTouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// Indicates if the objects time stamps should be updated.
        /// </summary>
        public bool UpdateTimeStamp
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUpdateTimeStamp, CurrentUpdateTimeStamp);

                // Ensure requested settings
                CurrentUpdateTimeStamp = value;
            }
        }


        /// <summary>
        /// Indicates if the search task should be created for the objects.
        /// </summary>
        public bool CreateSearchTask
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref  OriginalData.mCreateSearchTask, CurrentCreateSearchTask);

                // Ensure requested settings
                CurrentCreateSearchTask = value;
            }
        }


        /// <summary>
        /// Indicates if the export tasks should be logged.
        /// </summary>
        public bool LogExport
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogExport, CurrentLogExport);

                // Ensure requested settings
                CurrentLogExport = value;
            }
        }

        /// <summary>
        /// Indicates if the object version should be created. In the context of Delete, false means the object will be destroyed, true means it will be in the recycle bin.
        /// </summary>
        public bool CreateVersion
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mCreateVersion, CurrentCreateVersion);

                // Ensure requested settings
                CurrentCreateVersion = value;
            }
        }


        /// <summary>
        /// Indicates if the actions can run asynchronously. Default is true, if the application is web site, otherwise false.
        /// </summary>
        public bool AllowAsyncActions
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mAllowAsyncActions, CurrentAllowAsyncActions);

                // Ensure requested settings
                CurrentAllowAsyncActions = value;
            }
        }


        /// <summary>
        /// Guid of the current CMSTread. Guid.Empty when execution is not running under CMSThread.
        /// </summary>
        public Guid ThreadGuid
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mThreadGuid, CurrentThreadGuid);

                // Ensure requested settings
                CurrentThreadGuid = value;
            }
        }


        /// <summary>
        /// Indicates if the notifications should be sent.
        /// </summary>
        public bool SendNotifications
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mSendNotifications, CurrentSendNotifications);

                // Ensure requested settings
                CurrentSendNotifications = value;
            }
        }


        /// <summary>
        /// Indicates if the system fields should be updated.
        /// </summary>
        public bool UpdateSystemFields
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUpdateSystemFields, CurrentUpdateSystemFields);

                // Ensure requested settings
                CurrentUpdateSystemFields = value;
            }
        }


        /// <summary>
        /// Indicates if XMLs in synchronization module should be cached/obtained from cache.
        /// </summary>
        public bool UseCacheForSynchronizationXMLs
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUseCacheForSynchronizationXMLs, CurrentUseCacheForSynchronizationXMLs);

                // Ensure requested settings
                CurrentUseCacheForSynchronizationXMLs = value;
            }
        }


        /// <summary>
        /// Indicates if the current user should be initialized.
        /// </summary>
        public bool AllowInitUser
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mAllowInitUser, CurrentAllowInitUser);

                // Ensure requested settings
                CurrentAllowInitUser = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should behave as disconnected
        /// </summary>
        public bool Disconnected
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mDisconnected, CurrentDisconnected);

                // Ensure requested settings
                CurrentDisconnected = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should touch parent object
        /// </summary>
        public bool TouchParent
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mTouchParent, CurrentTouchParent);

                // Ensure requested settings
                CurrentTouchParent = value;
            }
        }


        /// <summary>
        /// Indicates if email should be sent.
        /// </summary>
        public bool SendEmails
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mSendEmails, CurrentSendEmails);

                // Ensure requested settings
                CurrentSendEmails = value;
            }
        }


        /// <summary>
        /// Indicates if global admin context should be used.
        /// </summary>
        public bool UseGlobalAdminContext
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUseGlobalAdminContext, CurrentUseGlobalAdminContext);

                // Ensure requested settings
                CurrentUseGlobalAdminContext = value;
            }
        }


        /// <summary>
        /// Indicates whether a search task should be executed when it is created.
        /// </summary>
        public bool EnableSmartSearchIndexer
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mEnableSmartSearchIndexer, CurrentEnableSmartSearchIndexer);

                // Ensure requested settings
                CurrentEnableSmartSearchIndexer = value;
            }
        }


        /// <summary>
        /// Indicates whether a physical files should be deleted for attachments, meta files and media files.
        /// </summary>
        public bool DeletePhysicalFiles
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mDeletePhysicalFiles, CurrentDeletePhysicalFiles);

                // Ensure requested settings
                CurrentDeletePhysicalFiles = value;
            }
        }


        /// <summary>
        /// Indicates whether a user activity points should be updated.
        /// </summary>
        public bool UpdateUserCounts
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUpdateUserCounts, CurrentUpdateUserCounts);

                // Ensure requested settings
                CurrentUpdateUserCounts = value;
            }
        }


        /// <summary>
        /// Indicates whether a user activity points should be updated.
        /// </summary>
        public bool UpdateRating
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUpdateRating, CurrentUpdateRating);

                // Ensure requested settings
                CurrentUpdateRating = value;
            }
        }


        /// <summary>
        /// Indicates whether a redirect is allowed in case the license check fails. If not, an exception is thrown instead.
        /// </summary>
        public bool AllowLicenseRedirect
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mAllowLicenseRedirect, CurrentAllowLicenseRedirect);

                // Ensure requested settings
                CurrentAllowLicenseRedirect = value;
            }
        }


        /// <summary>
        /// Indicates whether a warning is logged when license check for feature availability fails.
        /// </summary>
        public bool LogLicenseWarnings
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mLogLicenseWarnings, CurrentLogLicenseWarnings);

                // Ensure requested settings
                CurrentLogLicenseWarnings = value;
            }
        }


        /// <summary>
        /// Indicates whether a license check should be performed when managing objects.
        /// </summary>
        internal bool CheckLicense
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mCheckLicence, CurrentCheckLicense);

                // Ensure requested settings
                CurrentCheckLicense = value;
            }
        }


        /// <summary>
        /// Indicates whether the changes made to the object instance should be reseted.
        /// </summary>
        public bool ResetChanges
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mResetChanges, CurrentResetChanges);

                // Ensure requested settings
                CurrentResetChanges = value;
            }
        }


        /// <summary>
        /// Indicates whether a get data queries for object that are not allowed under current license returns empty data set or throw an error.
        /// </summary>
        public bool EmptyDataForInvalidLicense
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mEmptyDataForInvalidLicense, CurrentEmptyDataForInvalidLicense);

                // Ensure requested settings
                CurrentEmptyDataForInvalidLicense = value;
            }
        }


        /// <summary>
        /// Current thread culture.
        /// </summary>
        public CultureInfo ThreadCulture
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mThreadCulture, Thread.CurrentThread.CurrentCulture);
                StoreOriginalValue(ref OriginalData.mThreadUICulture, Thread.CurrentThread.CurrentUICulture);

                // Ensure requested settings
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
            }
        }


        /// <summary>
        /// Configures if object serialization within continuous integration is allowed in the given context
        /// </summary>
        public bool ContinuousIntegrationAllowObjectSerialization
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mContinuousIntegrationAllowObjectSerialization, CurrentContinuousIntegrationAllowObjectSerialization);

                CurrentContinuousIntegrationAllowObjectSerialization = value;
            }
        }

        #endregion


        #region "Static properties"

        /// <summary>
        /// Current site of the context.
        /// </summary>
        public static ISiteInfo CurrentSite
        {
            get
            {
                // Init user context
                var c = Current;

                return c.mSite ?? (c.mSite = GetCurrentSite());
            }
            set
            {
                Current.mSite = value;
            }
        }


        /// <summary>
        /// Current user of the context.
        /// </summary>
        public static IUserInfo CurrentUser
        {
            get
            {
                // If user is explicitly set, use that user, otherwise get the user from current context
                var c = Current;

                return c.mUser ?? GetCurrentUser();
            }
            set
            {
                Current.mUser = value;
            }
        }


        /// <summary>
        /// Indicates whether the currently running code executes on the live site.
        /// </summary>
        public static bool CurrentIsLiveSite
        {
            get
            {
                // Init user context
                var c = Current;
                if (c.mIsLiveSite == null)
                {
                    c.mIsLiveSite = GetCurrentIsLiveSite();
                }

                return (bool)c.mIsLiveSite;
            }
            set
            {
                Current.mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Indicates if the staging tasks should be logged within the context.
        /// </summary>
        public static bool CurrentLogSynchronization
        {
            get
            {
                return Current.mLogSynchronization ?? true;
            }
            private set
            {
                Current.mLogSynchronization = value;
            }
        }


        /// <summary>
        /// Indicates if the integration tasks should be logged within the context.
        /// </summary>
        public static bool CurrentLogIntegration
        {
            get
            {
                return Current.mLogIntegration ?? true;
            }
            private set
            {
                Current.mLogIntegration = value;
            }
        }


        /// <summary>
        /// Indicates whether logging through log context (LogContext) is currently enabled.
        /// </summary>
        public static bool CurrentEnableLogContext
        {
            get
            {
                return Current.mEnableLogContext ?? true;
            }
            private set
            {
                Current.mEnableLogContext = value;
            }
        }


        /// <summary>
        /// Indicates if the events should be logged within the context.
        /// </summary>
        public static bool CurrentLogEvents
        {
            get
            {
                return Current.mLogEvents ?? true;
            }
            private set
            {
                Current.mLogEvents = value;
            }
        }


        /// <summary>
        /// Indicates if the web farm tasks should be logged within the context.
        /// </summary>
        public static bool CurrentLogWebFarmTasks
        {
            get
            {
                return Current.mLogWebFarmTasks ?? true;
            }
            private set
            {
                Current.mLogWebFarmTasks = value;
            }
        }


        /// <summary>
        /// Indicates if the objects cache dependencies should be touched within the context.
        /// </summary>
        public static bool CurrentTouchCacheDependencies
        {
            get
            {
                return Current.mTouchCacheDependencies ?? true;
            }
            private set
            {
                Current.mTouchCacheDependencies = value;
            }
        }


        /// <summary>
        /// Indicates if the objects time stamps should be updated within the context.
        /// </summary>
        public static bool CurrentUpdateTimeStamp
        {
            get
            {
                return Current.mUpdateTimeStamp ?? true;
            }
            private set
            {
                Current.mUpdateTimeStamp = value;
            }
        }


        /// <summary>
        /// Indicates if the search task should be created for the objects within the context.
        /// </summary>
        public static bool CurrentCreateSearchTask
        {
            get
            {
                return Current.mCreateSearchTask ?? true;
            }
            private set
            {
                Current.mCreateSearchTask = value;
            }
        }


        /// <summary>
        /// Indicates if the export tasks should be logged within the context.
        /// </summary>
        public static bool CurrentLogExport
        {
            get
            {
                return Current.mLogExport ?? true;
            }
            private set
            {
                Current.mLogExport = value;
            }
        }


        /// <summary>
        /// Indicates if the object version should be created.
        /// </summary>
        public static bool CurrentCreateVersion
        {
            get
            {
                return Current.mCreateVersion ?? true;
            }
            private set
            {
                Current.mCreateVersion = value;
            }
        }


        /// <summary>
        /// Indicates if the notifications should be sent.
        /// </summary>
        public static bool CurrentSendNotifications
        {
            get
            {
                return Current.mSendNotifications ?? true;
            }
            private set
            {
                Current.mSendNotifications = value;
            }
        }


        /// <summary>
        /// Indicates if the actions can run asynchronously. Default is true.
        /// </summary>
        public static bool CurrentAllowAsyncActions
        {
            get
            {
                return Current.mAllowAsyncActions ?? true;
            }
            private set
            {
                Current.mAllowAsyncActions = value;
            }
        }


        /// <summary>
        /// Guid of the current CMSTread. Empty guid when execution is not running under CMSThread.
        /// </summary>
        public static Guid CurrentThreadGuid
        {
            get
            {
                return Current.mThreadGuid ?? Guid.Empty;
            }
            private set
            {
                Current.mThreadGuid = value;
            }
        }


        /// <summary>
        /// Indicates if the system fields should be updated.
        /// </summary>
        public static bool CurrentUpdateSystemFields
        {
            get
            {
                return Current.mUpdateSystemFields ?? true;
            }
            private set
            {
                Current.mUpdateSystemFields = value;
            }
        }


        /// <summary>
        /// Indicates if XMLs in synchronization module should be cached/obtained from cache.
        /// </summary>
        public static bool CurrentUseCacheForSynchronizationXMLs
        {
            get
            {
                return Current.mUseCacheForSynchronizationXMLs ?? true;
            }
            private set
            {
                Current.mUseCacheForSynchronizationXMLs = value;
            }
        }


        /// <summary>
        /// Indicates if the current user should be initialized.
        /// </summary>
        public static bool CurrentAllowInitUser
        {
            get
            {
                return Current.mAllowInitUser ?? true;
            }
            private set
            {
                Current.mAllowInitUser = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should behave as disconnected
        /// </summary>
        public static bool CurrentDisconnected
        {
            get
            {
                return Current.mDisconnected ?? false;
            }
            private set
            {
                Current.mDisconnected = value;
            }
        }


        /// <summary>
        /// Indicates if the objects within current context should touch parent object
        /// </summary>
        public static bool CurrentTouchParent
        {
            get
            {
                return Current.mTouchParent ?? true;
            }
            private set
            {
                Current.mTouchParent = value;
            }
        }


        /// <summary>
        /// Indicates if the emails should be sent.
        /// </summary>
        public static bool CurrentSendEmails
        {
            get
            {
                return Current.mSendEmails ?? true;
            }
            private set
            {
                Current.mSendEmails = value;
            }
        }


        /// <summary>
        /// Indicates if global admin context should be used.
        /// </summary>
        public static bool CurrentUseGlobalAdminContext
        {
            get
            {
                return Current.mUseGlobalAdminContext ?? false;
            }
            private set
            {
                Current.mUseGlobalAdminContext = value;
            }
        }


        /// <summary>
        /// Indicates whether a search task should be executed when it is created.
        /// </summary>
        public static bool CurrentEnableSmartSearchIndexer
        {
            get
            {
                return Current.mEnableSmartSearchIndexer ?? true;
            }
            private set
            {
                Current.mEnableSmartSearchIndexer = value;
            }
        }


        /// <summary>
        /// Indicates whether a physical files should be deleted for attachments, meta files and media files.
        /// </summary>
        public static bool CurrentDeletePhysicalFiles
        {
            get
            {
                return Current.mDeletePhysicalFiles ?? true;
            }
            private set
            {
                Current.mDeletePhysicalFiles = value;
            }
        }


        /// <summary>
        /// Indicates whether a user activity points should be updated.
        /// </summary>
        public static bool CurrentUpdateUserCounts
        {
            get
            {
                return Current.mUpdateUserCounts ?? true;
            }
            private set
            {
                Current.mUpdateUserCounts = value;
            }
        }


        /// <summary>
        /// Indicates whether a rating should be updated.
        /// </summary>
        public static bool CurrentUpdateRating
        {
            get
            {
                return Current.mUpdateRating ?? true;
            }
            private set
            {
                Current.mUpdateRating = value;
            }
        }


        /// <summary>
        /// Indicates whether a redirect is allowed in case the license check fails. If not, an exception is thrown instead.
        /// </summary>
        public static bool CurrentAllowLicenseRedirect
        {
            get
            {
                return Current.mAllowLicenseRedirect ?? SystemContext.IsCMSRunningAsMainApplication;
            }
            private set
            {
                Current.mAllowLicenseRedirect = value;
            }
        }


        /// <summary>
        /// Indicates whether a warning is logged when license check for feature availability fails.
        /// </summary>
        public static bool CurrentLogLicenseWarnings
        {
            get
            {
                return Current.mLogLicenseWarnings ?? true;
            }
            private set
            {
                Current.mLogLicenseWarnings = value;
            }
        }


        /// <summary>
        /// Indicates whether a license check should be performed when managing objects.
        /// </summary>
        public static bool CurrentCheckLicense
        {
            get
            {
                return Current.mCheckLicence ?? true;
            }
            private set
            {
                Current.mCheckLicence = value;
            }
        }


        /// <summary>
        /// Indicates whether a get data queries for object that are not allowed under current license returns empty data set or throw an error.
        /// </summary>
        public static bool CurrentEmptyDataForInvalidLicense
        {
            get
            {
                return Current.mEmptyDataForInvalidLicense ?? false;
            }
            private set
            {
                Current.mEmptyDataForInvalidLicense = value;
            }
        }


        /// <summary>
        /// Indicates whether the changes made to the object instance should be reseted.
        /// </summary>
        public static bool CurrentResetChanges
        {
            get
            {
                return Current.mResetChanges ?? true;
            }
            private set
            {
                Current.mResetChanges = value;
            }
        }


        /// <summary>
        /// Indicates if object serialization within continuous integration is allowed in the given context
        /// </summary>
        public static bool CurrentContinuousIntegrationAllowObjectSerialization
        {
            get
            {
                return Current.mContinuousIntegrationAllowObjectSerialization ?? true;
            }
            private set
            {
                Current.mContinuousIntegrationAllowObjectSerialization = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public CMSActionContext()
            : this(null)
        {
        }


        /// <summary>
        /// Constructor. Ensures that all actions during the life of this object will use the given user context.
        /// </summary>
        /// <param name="user">User to use within all actions</param>
        public CMSActionContext(IUserInfo user)
        {
            // Initialize user context
            if (user != null)
            {
                User = user;
            }
        }


        /// <summary>
        /// Initialize context, ensures values in current thread, if the values are already set, they are not overwritten.
        /// </summary>
        /// <remarks>
        /// Used when new CMSThread is started, so the thread has all the values it needs.
        /// </remarks>
        private static void EnsureRequiredValues()
        {
            if (CurrentUser == null)
            {
                IUserInfo user = GetCurrentUser();
                if ((user != null) && !user.IsPublic())
                {
                    // Do not init context with public user
                    CurrentUser = user;
                }
            }
        }


        /// <summary>
        /// Disables all automated actions except for the ones necessary to keep the system consistent
        /// 
        /// Disables:
        ///     Logging for synchronization, event log, web farm tasks, versioning and cache dependencies 
        ///     Timestamp update 
        ///     Object invalidation
        ///     Cache usage
        ///     Asynchronous actions
        ///     Sending of emails
        ///     Smart search indexing
        ///     Updating ratings
        /// 
        /// Does not disable:
        ///     Object serialization
        /// </summary>
        public CMSActionContext DisableAll()
        {
            LogSynchronization = false;
            LogIntegration = false;
            EnableLogContext = false;
            LogExport = false;
            LogWebFarmTasks = false;

            LogEvents = false;

            TouchCacheDependencies = false;
            TouchParent = false;
            UpdateTimeStamp = false;
            CreateVersion = false;
            AllowAsyncActions = false;
            UpdateSystemFields = false;
            UseCacheForSynchronizationXMLs = false;
            SendEmails = false;
            SendNotifications = false;
            UseGlobalAdminContext = false;
            EnableSmartSearchIndexer = false;
            UpdateUserCounts = false;
            UpdateRating = false;
            CreateSearchTask = false;
            
            // Not disabled on purpose to keep system data consistent.
            // If there would be need to disable this as well, all locations calling DisableAll must be reviewed and there may be need to explicitly handle serialization

            //AllowObjectSerialization = false;

            return this;
        }


        /// <summary>
        /// Disables logging of event log, staging and export tasks
        /// </summary>
        public CMSActionContext DisableLogging()
        {
            LogSynchronization = false;
            LogExport = false;
            LogEvents = false;

            return this;
        }


        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore current data context
            var o = OriginalData;

            if (o.mUser != null)
            {
                CurrentUser = o.mUser;
            }

            if (o.mIsLiveSite != null)
            {
                CurrentIsLiveSite = o.mIsLiveSite.Value;
            }

            if (o.mThreadCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = o.mThreadCulture;
            }

            if (o.mThreadUICulture != null)
            {
                Thread.CurrentThread.CurrentUICulture = o.mThreadUICulture;
            }

            if (o.mLogSynchronization.HasValue)
            {
                CurrentLogSynchronization = o.mLogSynchronization.Value;
            }

            if (o.mLogIntegration.HasValue)
            {
                CurrentLogIntegration = o.mLogIntegration.Value;
            }

            if (o.mEnableLogContext.HasValue)
            {
                CurrentEnableLogContext = o.mEnableLogContext.Value;
            }

            if (o.mLogExport.HasValue)
            {
                CurrentLogExport = o.mLogExport.Value;
            }

            if (o.mLogWebFarmTasks.HasValue)
            {
                CurrentLogWebFarmTasks = o.mLogWebFarmTasks.Value;
            }

            if (o.mLogEvents.HasValue)
            {
                CurrentLogEvents = o.mLogEvents.Value;
            }

            if (o.mTouchCacheDependencies.HasValue)
            {
                CurrentTouchCacheDependencies = o.mTouchCacheDependencies.Value;
            }

            if (o.mTouchParent.HasValue)
            {
                CurrentTouchParent = o.mTouchParent.Value;
            }

            if (o.mUpdateTimeStamp.HasValue)
            {
                CurrentUpdateTimeStamp = o.mUpdateTimeStamp.Value;
            }

            if (o.mCreateVersion.HasValue)
            {
                CurrentCreateVersion = o.mCreateVersion.Value;
            }

            if (o.mSendNotifications.HasValue)
            {
                CurrentSendNotifications = o.mSendNotifications.Value;
            }

            if (o.mUpdateSystemFields.HasValue)
            {
                CurrentUpdateSystemFields = o.mUpdateSystemFields.Value;
            }

            if (o.mUseCacheForSynchronizationXMLs.HasValue)
            {
                CurrentUseCacheForSynchronizationXMLs = o.mUseCacheForSynchronizationXMLs.Value;
            }

            if (o.mSendEmails.HasValue)
            {
                CurrentSendEmails = o.mSendEmails.Value;
            }

            if (o.mUseGlobalAdminContext.HasValue)
            {
                CurrentUseGlobalAdminContext = o.mUseGlobalAdminContext.Value;
            }

            if (o.mEnableSmartSearchIndexer.HasValue)
            {
                CurrentEnableSmartSearchIndexer = o.mEnableSmartSearchIndexer.Value;
            }

            if (o.mDeletePhysicalFiles.HasValue)
            {
                CurrentDeletePhysicalFiles = o.mDeletePhysicalFiles.Value;
            }

            if (o.mUpdateUserCounts.HasValue)
            {
                CurrentUpdateUserCounts = o.mUpdateUserCounts.Value;
            }

            if (o.mUpdateRating.HasValue)
            {
                CurrentUpdateRating = o.mUpdateRating.Value;
            }

            if (o.mCreateSearchTask.HasValue)
            {
                CurrentCreateSearchTask = o.mCreateSearchTask.Value;
            }

            if (o.mAllowInitUser.HasValue)
            {
                CurrentAllowInitUser = o.mAllowInitUser.Value;
            }

            if (o.mDisconnected.HasValue)
            {
                CurrentDisconnected = o.mDisconnected.Value;
            }

            if (o.mAllowAsyncActions.HasValue)
            {
                CurrentAllowAsyncActions = o.mAllowAsyncActions.Value;
            }

            if (o.mThreadGuid.HasValue)
            {
                CurrentThreadGuid = o.mThreadGuid.Value;
            }

            if (o.mAllowLicenseRedirect.HasValue)
            {
                CurrentAllowLicenseRedirect = o.mAllowLicenseRedirect.Value;
            }

            if (o.mLogLicenseWarnings.HasValue)
            {
                CurrentLogLicenseWarnings = o.mLogLicenseWarnings.Value;
            }

            if (o.mCheckLicence.HasValue)
            {
                CurrentCheckLicense = o.mCheckLicence.Value;
            }

            if (o.mEmptyDataForInvalidLicense.HasValue)
            {
                CurrentEmptyDataForInvalidLicense = o.mEmptyDataForInvalidLicense.Value;
            }

            if (o.mResetChanges.HasValue)
            {
                CurrentResetChanges = o.mResetChanges.Value;
            }

            if (o.mContinuousIntegrationAllowObjectSerialization.HasValue)
            {
                CurrentContinuousIntegrationAllowObjectSerialization = o.mContinuousIntegrationAllowObjectSerialization.Value;
            }

            base.RestoreOriginalValues();
        }


        /// <summary>
        /// Indicates if the object change should be logged.
        /// </summary>
        public static bool LogObjectChange()
        {
            return (CurrentLogSynchronization || CurrentLogExport || CurrentCreateVersion || CurrentLogIntegration);
        }


        /// <summary>
        /// Indicates if the document change should be logged.
        /// </summary>
        public static bool LogDocumentChange()
        {
            return (CurrentLogSynchronization || CurrentLogIntegration);
        }


        /// <summary>
        /// Gets the current user from context
        /// </summary>
        private static IUserInfo GetCurrentUser()
        {
            if (CurrentAllowInitUser)
            {
                return Service.Resolve<IAuthenticationService>().CurrentUser;
            }

            return null;
        }


        /// <summary>
        /// Gets the current site from context
        /// </summary>
        private static ISiteInfo GetCurrentSite()
        {
            return Service.Resolve<ISiteService>().CurrentSite;
        }


        /// <summary>
        /// Gets the current live site flag from the context
        /// </summary>
        private static bool GetCurrentIsLiveSite()
        {
            return Service.Resolve<ISiteService>().IsLiveSite;
        }


        /// <summary>
        /// Clones the object for the new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            EnsureRequiredValues();

            var ac = new CMSActionContext
                {
                    RestoreOriginal = false
                };

            ac.mUser = mUser;
            ac.mSite = mSite;
            ac.mIsLiveSite = mIsLiveSite;

            // Do not allow license redirect for a new thread
            ac.mAllowLicenseRedirect = false;

            return ac;
        }

        #endregion
    }
}