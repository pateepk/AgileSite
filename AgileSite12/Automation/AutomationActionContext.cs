using CMS.Base;

namespace CMS.Automation
{
    /// <summary>
    /// Automation action context. Ensures context for the automation actions block.
    /// </summary>
    [RegisterAllProperties]
    public sealed class AutomationActionContext : ExtendedActionContext<AutomationActionContext>
    {
        #region "Variables"

        /// <summary>
        /// Indicates if step permissions should be checked.
        /// </summary>
        private bool? mCheckStepPermissions;
        
        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if step permissions should be checked.
        /// </summary>
        public bool CheckStepPermissions
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mCheckStepPermissions, CurrentCheckStepPermissions);

                // Ensure requested settings
                CurrentCheckStepPermissions = value;
            }
        }
        
        #endregion


        #region "Static properties"

        /// <summary>
        /// Indicates if step permissions should be checked.
        /// </summary>
        public static bool CurrentCheckStepPermissions
        {
            get
            {
                return Current.mCheckStepPermissions ?? true;
            }
            private set
            {
                Current.mCheckStepPermissions = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore settings
            var o = OriginalData;
            if (o.mCheckStepPermissions.HasValue)
            {
                CurrentCheckStepPermissions = o.mCheckStepPermissions.Value;
            }

            base.RestoreOriginalValues();
        }

        #endregion
    }
}