using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(ActivitiesModule))]

namespace CMS.Activities
{
    /// <summary>
    /// Represents the Activities module.
    /// </summary>
    internal class ActivitiesModule : Module
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivitiesModule()
            : base(new ActivitiesModuleMetadata())
        {
        }


        protected override void OnInit()
        {
            base.OnInit();

            RegisterActivityModifier();
           
            if (SystemContext.IsWebSite)
            {
                RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => ActivityLogWorker.Current.EnsureRunningThread();
            }
        }


        private void RegisterActivityModifier()
        {
            var activityLogService = Service.Resolve<IActivityLogService>();
            var siteService = Service.Resolve<ISiteService>();
            var activityUrlService = Service.Resolve<IActivityUrlService>();

            var modifier = new ActivityModifier(siteService, activityUrlService);
            activityLogService.RegisterModifier(modifier);
        }
    }
}
