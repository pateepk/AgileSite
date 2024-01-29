using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Event arguments for UniSelector.OnGetSelectionDialogScript handler.
    /// </summary>
    public class GetSelectionDialogScriptEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Script used to get selection dialog
        /// </summary>
        public string Script
        {
            get;
            set;
        }
    }
}