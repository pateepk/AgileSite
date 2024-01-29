namespace CMS.Base
{
    /// <summary>
    /// Lazy data container
    /// </summary>
    internal class CMSLazyData<TValue> : ICloneThreadItem
    {
        #region "Variables"

        private bool mAllowCloneForNewThread = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the value is already created (initialized)
        /// </summary>
        public bool IsValueCreated
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public virtual TValue Value
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the value is currently initializing (flag to prevent infinite loop)
        /// </summary>
        public int InitializingFromThreadId
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the item is copied for the new thread
        /// </summary>
        public bool AllowCloneForNewThread
        {
            get
            {
                return mAllowCloneForNewThread;
            }
            set
            {
                mAllowCloneForNewThread = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Resets the state of the object to re-initialize the value on the next request
        /// </summary>
        public void Reset()
        {
            lock (this)
            {
                IsValueCreated = false;
                Value = default(TValue);
            }
        }


        /// <summary>
        /// Clones the object for the new thread
        /// </summary>
        public object CloneForNewThread()
        {
            if (!AllowCloneForNewThread || !IsValueCreated)
            {
                // Do not clone if value not created or not allowed to clone
                return null;
            }

            if (Value is INotCopyThreadItem)
            {
                // Internal item not allowed to be cloned, do not clone
                return null;
            }
            else
            {
                var item = Value as ICloneThreadItem;
                if (item != null)
                {
                    // Clone the lazy data and clone its value
                    var newData = new CMSLazyData<TValue>();

                    newData.Value = (TValue)item.CloneForNewThread();
                    newData.IsValueCreated = true;

                    return newData;
                }
            }

            // Clone the lazy data and its value
            var newValueData = new CMSLazyData<TValue>();

            newValueData.Value = Value;
            newValueData.IsValueCreated = true;

            return newValueData;
        }

        #endregion
    }
}
