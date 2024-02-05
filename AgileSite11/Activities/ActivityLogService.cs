using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Activities
{
    /// <summary>
    /// Provides possibility to log activities.
    /// </summary>
    /// <threadsafety static="false" instance="false"/>
    internal class ActivityLogService : IActivityLogService
    {
        internal static readonly IList<IActivityLogFilter> mActivityLogFilters = new List<IActivityLogFilter>();
        internal static readonly IList<IActivityModifier> mActivityModifiers = new List<IActivityModifier>();
        internal static readonly IList<IActivityLogValidator> mActivityLogValidators = new List<IActivityLogValidator>();

        private readonly IActivityRepository mActivityRepository;
        private readonly ISiteService mSiteService;
        private readonly IActivitySettings mActivitySettings;
        private readonly IActivityFactory mActivityFactory;


        /// <summary>
        /// Creates a new instance of <see cref="ActivityLogService"/>
        /// </summary>
        /// <param name="activityRepository">Represents activity repository</param>
        /// <param name="siteService">Site service</param>
        /// <param name="activitySettings">Provides access to activities settings</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="activityRepository"/> is <c>null</c> -or-
        /// <paramref name="siteService"/> is <c>null</c> -or-
        /// <paramref name="activitySettings"/> is <c>null</c> -or-
        /// </exception> 
        public ActivityLogService(IActivityRepository activityRepository, ISiteService siteService, IActivitySettings activitySettings)
        {
            if (activityRepository == null)
            {
                throw new ArgumentNullException("activityRepository");
            }
            if (siteService == null)
            {
                throw new ArgumentNullException("siteService");
            }
            if (activitySettings == null)
            {
                throw new ArgumentNullException("activitySettings");
            }

            mActivityRepository = activityRepository;
            mSiteService = siteService;
            mActivitySettings = activitySettings;
            mActivityFactory = Service.Resolve<IActivityFactory>();
        }


        /// <summary>
        /// Logs given activity. Registered <see cref="IActivityLogFilter"/> are checked, activity is further processed only if all filters allow it.
        /// Then <see cref="IActivityInfo"/> is created and <paramref name="activityInitializer"/> is called. 
        /// Afterwards all registered modifiers are applied to the <see cref="IActivityInfo"/>. Finally the <see cref="IActivityInfo"/> is validated by registered <see cref="IActivityLogValidator"/>.
        /// Activity is stored only if it is valid. After the activity is stored, two events are raised.
        /// <see cref="ActivityEvents.ActivityProcessedInLogService"/> which should be used to hook on and also legacy activity which is there due to compatibility reasons (should not be used).
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity.</param>
        /// <param name="currentRequest">Current request</param>
        /// <param name="loggingDisabledInAdministration"><c>True</c> if activities should not be logged in administration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activityInitializer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentRequest"/> is <c>null</c>.</exception>
        public void Log(IActivityInitializer activityInitializer, HttpRequestBase currentRequest, bool loggingDisabledInAdministration = true)
        {
            if (activityInitializer == null)
            {
                throw new ArgumentNullException("activityInitializer");
            }

            if (currentRequest == null)
            {
                throw new ArgumentNullException("currentRequest");
            }

            if (IsSiteUnavailable() ||
                InsufficientLicense() ||
                IsHeadRequest(currentRequest) ||
                IsActivityDenied() ||
                IsActivityLoggingDisabledOnSite(mSiteService.CurrentSite.SiteName, activityInitializer.SettingsKeyName, activityInitializer.ActivityType) ||
                ActivitiesDisabledOrOnlineMarketingModuleNotLoaded() ||
                (!mSiteService.IsLiveSite && loggingDisabledInAdministration))
            {
                return;
            }

            var activityInfo = mActivityFactory.Create(activityInitializer);

            ApplyModifications(activityInfo);

            if (!ValidateActivity(activityInfo))
            {
                return;
            }

            mActivityRepository.Save(activityInfo);
            RaiseActivityLogServiceProcessedEvents(activityInfo);
        }


        /// <summary>
        /// Logs activity initialized by <paramref name="activityInitializer"/> directly to the database. Implementation does not check <see cref="IActivityLogFilter"/> 
        /// and does not use <see cref="IActivityModifier"/>. If you have <see cref="HttpRequestBase"/> available please 
        /// use <see cref="Log(IActivityInitializer, HttpRequestBase, bool)"/> instead.
        /// Method is designed to be used in environment without request e. g. scheduled tasks. After the activity is stored, two events are raised.
        /// <see cref="ActivityEvents.ActivityProcessedInLogService"/> which should be used to hook on and also legacy activity which is there due to compatibility reasons (should not be used).
        /// </summary>
        /// <param name="activityInitializer">Activity initializer used to initialize logged activity.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activityInitializer"/> is <c>null</c>.</exception>
        public void LogWithoutModifiersAndFilters(IActivityInitializer activityInitializer)
        {
            if (activityInitializer == null)
            {
                throw new ArgumentNullException("activityInitializer");
            }

            if (InsufficientLicense() ||
                IsActivityLoggingDisabledOnSite(mSiteService.CurrentSite.SiteName, activityInitializer.SettingsKeyName, activityInitializer.ActivityType) ||
                ActivitiesDisabledOrOnlineMarketingModuleNotLoaded())
            {
                return;
            }

            var activityInfo = mActivityFactory.Create(activityInitializer);

            if (!ValidateActivity(activityInfo))
            {
                return;
            }

            mActivityRepository.Save(activityInfo);
            RaiseActivityLogServiceProcessedEvents(activityInfo);
        }


        /// <summary>
        /// Registers filter that can filter out logging of activities. Filters are called before <see cref="ActivityInfo"/> is created.
        /// Activity is not logged if at least one filter denies activity.
        /// </summary>
        /// <param name="filter">Activity filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> is <c>null</c></exception>
        public void RegisterFilter(IActivityLogFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            mActivityLogFilters.Add(filter);
        }


        /// <summary>
        /// Registers activity modifier. Modifier is used to update every newly created <see cref="IActivityInfo"/>. Modifier is called after activity is created
        /// and successfully initialized. Modifiers are global, thus they are shared across all instances of the service.
        /// </summary>
        /// <param name="activityModifier">Activity modifier to be registered</param>
        /// <exception cref="ArgumentNullException"><paramref name="activityModifier"/> is <c>null</c></exception>
        public void RegisterModifier(IActivityModifier activityModifier)
        {
            if (activityModifier == null)
            {
                throw new ArgumentNullException("activityModifier");
            }

            mActivityModifiers.Add(activityModifier);
        }


        /// <summary>
        /// Registers activity validator. Validators are used to check whether the processed <see cref="IActivityInfo"/> is valid and can be logged.
        /// Validators are global, therefore they are shared across all instances of the service.
        /// </summary>
        /// <param name="activityLogValidator">Activity log validator to be registered</param>
        /// <exception cref="ArgumentNullException"><paramref name="activityLogValidator"/> is <c>null</c></exception>
        public void RegisterValidator(IActivityLogValidator activityLogValidator)
        {
            if (activityLogValidator == null)
            {
                throw new ArgumentNullException("activityLogValidator");
            }

            mActivityLogValidators.Add(activityLogValidator);
        }


        /// <summary>
        /// Used to check current license. Override is used only for test purposes.
        /// </summary>
        /// <returns></returns>
        internal virtual bool InsufficientLicense()
        {
            if (string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement, ModuleName.CONTACTMANAGEMENT);
            }
            return !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement);
        }


        private bool IsSiteUnavailable()
        {
            return mSiteService.CurrentSite == null;
        }


        private bool IsActivityDenied()
        {
            return mActivityLogFilters.Any(filter => filter.IsDenied());
        }


        private bool ActivitiesDisabledOrOnlineMarketingModuleNotLoaded()
        {
            return !mActivitySettings.ActivitiesEnabledAndModuleLoadedForSite(mSiteService.CurrentSite.SiteName);
        }


        private static bool IsHeadRequest(HttpRequestBase currentRequest)
        {
            return string.Equals(currentRequest.HttpMethod, System.Net.WebRequestMethods.Http.Head, StringComparison.OrdinalIgnoreCase);
        }


        private bool IsActivityLoggingDisabledOnSite(string siteName, string settingsKeyName, string activityType)
        {
            return !(string.IsNullOrEmpty(settingsKeyName) || ActivitySettingsHelper.GetLoggingEnabled(siteName, settingsKeyName, activityType));
        }


        private void ApplyModifications(IActivityInfo activity)
        {
            foreach (var activityModifier in mActivityModifiers)
            {
                activityModifier.Modify(activity);
            }
        }


        /// <summary>
        /// Checks whether the given <paramref name="activity"/> passes all the validators.
        /// </summary>
        /// <param name="activity">Activity to be validated</param>
        /// <returns>True, if <paramref name="activity"/> passes all the validators; otherwise, false</returns>
        private bool ValidateActivity(IActivityInfo activity)
        {
            return mActivityLogValidators.All(validator => validator.IsValid(activity));
        }


        private void RaiseActivityLogServiceProcessedEvents(IActivityInfo activity)
        {
            // New activity service performed event
            ActivityEvents.ActivityProcessedInLogService.StartEvent(activity);
        }
    }
}