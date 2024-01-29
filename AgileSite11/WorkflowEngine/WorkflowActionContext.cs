using CMS.Base;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow action context. Ensures context for the workflow actions block.
    /// </summary>
    [RegisterAllProperties]
    public sealed class WorkflowActionContext : ExtendedActionContext<WorkflowActionContext>
    {
        #region "Variables"

        /// <summary>
        /// Indicates if step permissions should be checked.
        /// </summary>
        private bool? mCheckStepPermissions;
        
        /// <summary>
        /// Indicates if the workflow actions should be processed asynchronously (in new thread).
        /// </summary>
        private bool? mProcessActionsAsync;

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


        /// <summary>
        /// Indicates if the workflow actions should be processed asynchronously (in new thread).
        /// </summary>
        public bool ProcessActionsAsync
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mProcessActionsAsync, CurrentProcessActionsAsync);

                // Ensure requested settings
                CurrentProcessActionsAsync = value;
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


        /// <summary>
        /// Indicates if the workflow actions should be processed asynchronously (in new thread).
        /// </summary>
        public static bool CurrentProcessActionsAsync
        {
            get
            {
                return Current.mProcessActionsAsync ?? true;
            }
            private set
            {
                Current.mProcessActionsAsync = value;
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

            if (o.mProcessActionsAsync.HasValue)
            {
                CurrentProcessActionsAsync = o.mProcessActionsAsync.Value;
            }

            base.RestoreOriginalValues();
        }

        #endregion
    }
}