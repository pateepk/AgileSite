namespace CMS.UIControls
{
    /// <summary>
    /// Defines contract foreign modules can use to add new mass actions.
    /// </summary>
    public interface IExtensibleMassActions
    {
        /// <summary>
        /// Adds new action to the Mass actions control. Has to be called before Page_Load event.
        /// </summary>
        /// <param name="massActionItems">Action to be added</param>
        void AddMassActions(params MassActionItem[] massActionItems);
    }
}