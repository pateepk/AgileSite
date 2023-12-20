using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Base automation action
    /// </summary>
    public abstract class AutomationAction : BaseWorkflowAction<BaseInfo, AutomationStateInfo, AutomationActionEnum>
    {
        #region "Properties"

        /// <summary>
        /// Automation manager
        /// </summary>
        public AutomationManager AutomationManager
        {
            get
            {
                return Manager as AutomationManager;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Refreshes object instance
        /// </summary>
        protected void RefreshObject()
        {
            InfoObject = ProviderHelper.GetInfoById(InfoObject.TypeInfo.ObjectType, InfoObject.Generalized.ObjectID);
        }


        /// <summary>
        /// Gets the default macro resolver.
        /// </summary>
        protected override MacroResolver GetDefaultMacroResolver()
        {
            var resolver = base.GetDefaultMacroResolver();

            var activityDetailItemID = StateObject.StateCustomData[TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEM_DETAILID];
            if (activityDetailItemID != null)
            {
                resolver.SetNamedSourceData("ActivityDetailItemID", activityDetailItemID);
            }

            var activityItemID = StateObject.StateCustomData[TriggerDataConstants.TRIGGER_DATA_ACTIVITY_ITEMID];
            if (activityItemID != null)
            {
                resolver.SetNamedSourceData("ActivityItemID", activityItemID);
            }

            var activityValue = StateObject.StateCustomData[TriggerDataConstants.TRIGGER_DATA_ACTIVITY_VALUE];
            if (activityValue != null)
            {
                resolver.SetNamedSourceData("ActivityValue", activityValue);
            }

            var activitySiteId = StateObject.StateCustomData[TriggerDataConstants.TRIGGER_DATA_ACTIVITY_SITEID];
            if (activitySiteId != null)
            {
                resolver.SetNamedSourceData("ActivitySiteID", activitySiteId);
            }

            return resolver;
        }

        #endregion
    }
}
