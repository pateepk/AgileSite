using CMS.Base;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Context for the repository actions.
    /// </summary>
    [RegisterAllProperties]
    public sealed class RepositoryActionContext : AbstractActionContext<RepositoryActionContext>
    {
        #region "Variables"
  
        /// <summary>
        /// Indicates whether objects are being currently restored or not.
        /// </summary>
        private bool? mIsRestoreOperationRunning;
  
        #endregion
  
  
        #region "Public instance properties"

        /// <summary>
        /// Indicates whether objects are being restored or not.
        /// </summary>
        public bool IsRestoreOperationRunning
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mIsRestoreOperationRunning, CurrentIsRestoreOperationRunning);

                // Ensure requested settings
                CurrentIsRestoreOperationRunning = value;
            }
        }
  
        #endregion
  
  
        #region "Public static properties"

        /// <summary>
        /// Indicates whether objects are being restored or not.
        /// </summary>
        public static bool CurrentIsRestoreOperationRunning
        {
            get
            {
                return Current.mIsRestoreOperationRunning ?? false;
            }
            private set
            {
                Current.mIsRestoreOperationRunning = value;
            }
        }
  
        #endregion
  
  
        #region "Methods"
  
        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore current data context
            var o = OriginalData;

            if (o.mIsRestoreOperationRunning.HasValue)
            {
                IsRestoreOperationRunning = o.mIsRestoreOperationRunning.Value;
            }
  
            base.RestoreOriginalValues();
        }
  
        #endregion
    }
}
