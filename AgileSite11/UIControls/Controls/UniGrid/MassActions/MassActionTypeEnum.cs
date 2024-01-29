namespace CMS.UIControls
{
    /// <summary>
    /// Type of action which will be performed when performing the mass action.
    /// </summary>
    public enum MassActionTypeEnum
    {
        /// <summary>
        /// Current document will be redirected to obtained URL.
        /// </summary>
        Redirect = 0,


        /// <summary>
        /// Opens modal dialog with the obtained URL.
        /// </summary>
        OpenModal = 1,
    }
}