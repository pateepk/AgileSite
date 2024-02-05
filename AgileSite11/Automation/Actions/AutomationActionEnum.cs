namespace CMS.Automation
{
    /// <summary>
    /// Automation action enumeration.
    /// </summary>
    public enum AutomationActionEnum
    {
        /// <summary>
        /// Unknown action.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Object is moved to next step.
        /// </summary>
        MoveToNextStep = 1,
        
        /// <summary>
        /// Object is moved to specific step.
        /// </summary>
        MoveToSpecificStep = 2,

        /// <summary>
        /// Object is moved to previous step.
        /// </summary>
        MoveToPreviousStep = 3,
    }
}