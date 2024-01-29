namespace CMS.UIControls
{
    /// <summary>
    /// Specifies the scope mass action will be used for.
    /// </summary>
    public enum MassActionScopeEnum
    {
        /// <summary>
        /// Limits the scope only to the selected items.
        /// </summary>
        SelectedItems = 0,
        

        /// <summary>
        /// Mass action will be applied for every item.
        /// </summary>
        AllItems = 1
    }
}