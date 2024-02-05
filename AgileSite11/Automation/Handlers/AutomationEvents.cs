namespace CMS.Automation
{
    /// <summary>
    /// Automation events
    /// </summary>
    public static class AutomationEvents
    {
        #region "Automation"

        /// <summary>
        /// Fires when the object is moved to next step
        /// </summary>
        public static AutomationHandler MoveToNextStep = new AutomationHandler { Name = "AutomationEvents.MoveToNextStep" };


        /// <summary>
        /// Fires when the object is moved to previous step
        /// </summary>
        public static AutomationHandler MoveToPreviousStep = new AutomationHandler { Name = "AutomationEvents.MoveToPreviousStep" };

        #endregion


        #region "Automation action"
        
        /// <summary>
        /// Fires when an action is executed
        /// </summary>
        public static AutomationActionHandler Action = new AutomationActionHandler { Name = "AutomationEvents.Action" };

        #endregion


        #region "Triggers"

        /// <summary>
        /// Fires when a trigger is processed
        /// </summary>
        public static AutomationProcessTriggerHandler ProcessTrigger = new AutomationProcessTriggerHandler { Name = "AutomationEvents.ProcessTrigger" };

        #endregion
    }
}