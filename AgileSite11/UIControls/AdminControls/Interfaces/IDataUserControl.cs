using System;


namespace CMS.UIControls
{
    /// <summary>
    /// DataUserControl interface
    /// </summary>
    public interface IDataUserControl
    {
        #region "Methods"

        /// <summary>
        /// Reloads control's data.
        /// </summary>
        /// <param name="forceLoad">Indicates whether the load should be forced</param>
        void ReloadData(bool forceLoad);

        #endregion
    }
}

