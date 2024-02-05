

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Portal context menu base class.
    /// </summary>
    public abstract class CMSContextMenuControl : AbstractUserControl
    {
        #region "Properties"

        /// <summary>
        /// Context menu.
        /// </summary>
        public ContextMenu ContextMenu
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reload data.
        /// </summary>
        public virtual void ReloadData()
        {
        }

        #endregion
    }
}