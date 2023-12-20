using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document action context. Ensures context for the document actions block.
    /// </summary>
    [RegisterAllProperties]
    public sealed class DocumentActionContext : ExtendedActionContext<DocumentActionContext>
    {
        #region "Variables"

        private bool? mHandleACLs;
        private bool? mResetChanges;
        private bool? mUseAutomaticOrdering;
        private bool? mPreserveACLHierarchy;
        private bool? mResetIsWaitingForTranslationFlag;
        private bool? mGenerateDocumentAliases;
        private bool? mSynchronizeFieldValues;
        private DocumentOrderEnum? mDocumentNewOrder;
        private bool? mForceDestroyHistory;
        private bool? mAllowRootDeletion;

        #endregion


        #region "Static properties"

        /// <summary>
        /// Defines default order for new document creation.
        /// </summary>
        internal static DocumentOrderEnum CurrentDocumentNewOrder
        {
            get
            {
                return Current.mDocumentNewOrder ?? DocumentOrderEnum.Unknown;
            }
            private set
            {
                Current.mDocumentNewOrder = value;
            }
        }


        /// <summary>
        /// Indicates whether a root document can be completely deleted. By default at least one culture is kept to ensure data structure integrity.
        /// </summary>
        internal static bool CurrentAllowRootDeletion
        {
            get
            {
                return Current.mAllowRootDeletion ?? false;
            }
            private set
            {
                Current.mAllowRootDeletion = value;
            }
        }


        /// <summary>
        /// Indicates whether field values should be synchronized on set for the document (SKU mappings etc.)
        /// </summary>
        public static bool CurrentSynchronizeFieldValues
        {
            get
            {
                return Current.mSynchronizeFieldValues ?? true;
            }
            private set
            {
                Current.mSynchronizeFieldValues = value;
            }
        }


        /// <summary>
        /// Indicates if document aliases should be generated for processed documents.
        /// </summary>
        public static bool CurrentGenerateDocumentAliases
        {
            get
            {
                return Current.mGenerateDocumentAliases ?? true;
            }
            private set
            {
                Current.mGenerateDocumentAliases = value;
            }
        }


        /// <summary>
        /// Indicates whether the ACL hierarchy should be preserved on processed documents.
        /// </summary>
        public static bool CurrentPreserveACLHierarchy
        {
            get
            {
                return Current.mPreserveACLHierarchy ?? false;
            }
            private set
            {
                Current.mPreserveACLHierarchy = value;
            }
        }


        /// <summary>
        /// If true, automatic ordering is used for new nodes.
        /// </summary>
        public static bool CurrentUseAutomaticOrdering
        {
            get
            {
                return Current.mUseAutomaticOrdering ?? true;
            }
            private set
            {
                Current.mUseAutomaticOrdering = value;
            }
        }


        /// <summary>
        /// Indicates whether the ACL settings should be processed during the document operations.
        /// </summary>
        public static bool CurrentHandleACLs
        {
            get
            {
                return Current.mHandleACLs ?? true;
            }
            private set
            {
                Current.mHandleACLs = value;
            }
        }


        /// <summary>
        /// Indicates whether the changes made to the document instance should be reseted.
        /// </summary>
        public static bool CurrentResetChanges
        {
            get
            {
                return Current.mResetChanges ?? true;
            }
            private set
            {
                Current.mResetChanges = value;
            }
        }


        /// <summary>
        /// Indicates whether the 'Document is waiting for translation' flag should be cleared.
        /// </summary>
        public static bool CurrentResetIsWaitingForTranslationFlag
        {
            get
            {
                return Current.mResetIsWaitingForTranslationFlag ?? true;
            }
            private set
            {
                Current.mResetIsWaitingForTranslationFlag = value;
            }
        }


        /// <summary>
        /// Indicates whether document is always destroyed, or can be deleted into recycle bin. False by default.
        /// </summary>
        internal static bool CurrentForceDestroyHistory
        {
            get
            {
                return Current.mForceDestroyHistory ?? false;
            }
            private set
            {
                Current.mForceDestroyHistory = value;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Defines default order for new document creation.
        /// </summary>
        public DocumentOrderEnum DocumentNewOrder
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mDocumentNewOrder, CurrentDocumentNewOrder);

                // Ensure requested settings
                CurrentDocumentNewOrder = value;
            }
        }


        /// <summary>
        /// Indicates whether a root document can be completely deleted. By default at least one culture is kept to ensure data structure integrity.
        /// </summary>
        internal bool AllowRootDeletion
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mAllowRootDeletion, CurrentAllowRootDeletion);

                // Ensure requested settings
                CurrentAllowRootDeletion = value;
            }
        }


        /// <summary>
        /// Indicates whether field values should be synchronized on set for the document (SKU mappings etc.)
        /// </summary>
        public bool SynchronizeFieldValues
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mSynchronizeFieldValues, CurrentSynchronizeFieldValues);

                // Ensure requested settings
                CurrentSynchronizeFieldValues = value;
            }
        }


        /// <summary>
        /// Indicates if document aliases should be generated for processed documents.
        /// </summary>
        public bool GenerateDocumentAliases
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mGenerateDocumentAliases, CurrentGenerateDocumentAliases);

                // Ensure requested settings
                CurrentGenerateDocumentAliases = value;
            }
        }


        /// <summary>
        /// Indicates whether the ACL hierarchy should be preserved on processed documents.
        /// </summary>
        public bool PreserveACLHierarchy
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mPreserveACLHierarchy, CurrentPreserveACLHierarchy);

                // Ensure requested settings
                CurrentPreserveACLHierarchy = value;
            }
        }


        /// <summary>
        /// If true, automatic ordering is used for new nodes.
        /// </summary>
        public bool UseAutomaticOrdering
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mUseAutomaticOrdering, CurrentUseAutomaticOrdering);

                // Ensure requested settings
                CurrentUseAutomaticOrdering = value;
            }
        }


        /// <summary>
        /// Indicates whether the ACL settings should be processed during the document operations.
        /// </summary>
        public bool HandleACLs
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mHandleACLs, CurrentHandleACLs);

                // Ensure requested settings
                CurrentHandleACLs = value;
            }
        }


        /// <summary>
        /// Indicates whether the changes made to the document instance should be reseted.
        /// </summary>
        public bool ResetChanges
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mResetChanges, CurrentResetChanges);

                // Ensure requested settings
                CurrentResetChanges = value;
            }
        }


        /// <summary>
        /// Indicates whether the 'Document is waiting for translation' flag should be cleared.
        /// </summary>
        public bool ResetIsWaitingForTranslationFlag
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mResetIsWaitingForTranslationFlag, CurrentResetIsWaitingForTranslationFlag);

                // Ensure requested settings
                CurrentResetIsWaitingForTranslationFlag = value;
            }
        }

        #endregion


        #region "Internal properties"

        /// <summary>
        /// Indicates whether document is always destroyed, or can be deleted into recycle bin. False by default.
        /// </summary>
        internal bool ForceDestroyHistory
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mForceDestroyHistory, CurrentForceDestroyHistory);

                // Ensure requested settings
                CurrentForceDestroyHistory = value;
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

            if (o.mHandleACLs.HasValue)
            {
                CurrentHandleACLs = o.mHandleACLs.Value;
            }

            if (o.mResetChanges.HasValue)
            {
                CurrentResetChanges = o.mResetChanges.Value;
            }

            if (o.mUseAutomaticOrdering.HasValue)
            {
                CurrentUseAutomaticOrdering = o.mUseAutomaticOrdering.Value;
            }

            if (o.mSynchronizeFieldValues.HasValue)
            {
                CurrentSynchronizeFieldValues = o.mSynchronizeFieldValues.Value;
            }

            if (o.mGenerateDocumentAliases.HasValue)
            {
                CurrentGenerateDocumentAliases = o.mGenerateDocumentAliases.Value;
            }

            if (o.mPreserveACLHierarchy.HasValue)
            {
                CurrentPreserveACLHierarchy = o.mPreserveACLHierarchy.Value;
            }

            if (o.mResetIsWaitingForTranslationFlag.HasValue)
            {
                CurrentResetIsWaitingForTranslationFlag = o.mResetIsWaitingForTranslationFlag.Value;
            }

            if (o.mDocumentNewOrder.HasValue)
            {
                CurrentDocumentNewOrder = o.mDocumentNewOrder.Value;
            }

            if (o.mForceDestroyHistory.HasValue)
            {
                CurrentForceDestroyHistory = o.mForceDestroyHistory.Value;
            }

            if (o.mAllowRootDeletion.HasValue)
            {
                CurrentAllowRootDeletion = o.mAllowRootDeletion.Value;
            }

            base.RestoreOriginalValues();
        }

        #endregion
    }
}