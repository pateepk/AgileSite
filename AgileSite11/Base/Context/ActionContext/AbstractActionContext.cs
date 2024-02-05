using System;

namespace CMS.Base
{
    /// <summary>
    /// Base class for action context
    /// </summary>
    public abstract class AbstractActionContext<TContext> : AbstractContext<TContext>, IDisposable
        where TContext : AbstractActionContext<TContext>, new()
    {
        // Previous context (used to restore original values)
        private readonly CMSLazy<TContext> mOriginalData = new CMSLazy<TContext>(() => new TContext());

        /// If true, the original values are restored in the context
        private bool mRestoreOriginal = true;


        /// <summary>
        /// Previous context data
        /// </summary>
        protected TContext OriginalData
        {
            get
            {
                return mOriginalData;
            }
        }


        /// <summary>
        /// If true, the original values are restored in the context
        /// </summary>
        public bool RestoreOriginal
        {
            get
            {
                return mRestoreOriginal && !IsDefault;
            }
            set
            {
                mRestoreOriginal = value;
            }
        }

        
        /// <summary>
        /// Disposes the object.
        /// </summary>
        public virtual void Dispose()
        {
            if (RestoreOriginal && (mOriginalData != null))
            {
                RestoreOriginalValues();
            }
        }
        

        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected virtual void RestoreOriginalValues()
        {
            // No restore by default
        }


        /// <summary>
        /// Stores current value as the original value if original value hasn't been stored yet.
        /// </summary>
        /// <typeparam name="TValue">Type of the property</typeparam>
        /// <param name="originalValueProperty">Original value property inner field.</param>
        /// <param name="currentValue">Value of the property before it has been changed, hence the original value.</param>
        protected virtual void StoreOriginalValue<TValue>(ref TValue? originalValueProperty, TValue currentValue)
            where TValue : struct
        {
            if (!originalValueProperty.HasValue)
            {
                originalValueProperty = currentValue;
            }
        }


        /// <summary>
        /// Stores current value as the original value.
        /// </summary>
        /// <typeparam name="TValue">Type of the property</typeparam>
        /// <param name="originalValueProperty">Original value property inner field.</param>
        /// <param name="currentValue">Value of the property before it has been changed, hence the original value.</param>
        protected virtual void StoreOriginalValue<TValue>(ref TValue originalValueProperty, TValue currentValue)
            where TValue : class
        {
            originalValueProperty = currentValue;
        }
    }
}
