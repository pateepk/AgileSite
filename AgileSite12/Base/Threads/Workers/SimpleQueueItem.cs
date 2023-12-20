using System;

namespace CMS.Base
{
    /// <summary>
    /// Item containing <see cref="Action"/> and identificator used for checking duplicity in queue of <see cref="SimpleQueueWorker{T}"/>. 
    /// </summary>
    public class SimpleQueueItem
    {
        private string mKey;

        /// <summary>
        /// <see cref="Action"/> to be run by <see cref="SimpleQueueWorker{T}"/>.
        /// </summary>
        public Action Action
        {
            get;
            set;
        }


        /// <summary>
        /// Key for checking duplicity in queue of <see cref="SimpleQueueWorker{T}"/>. 
        /// Default value is unique <see cref="Guid"/>.
        /// </summary>
        public string Key
        {
            get
            {
                return mKey ?? (mKey = Guid.NewGuid().ToString());
            }
            set
            {
                mKey = value;
            }
        }


        /// <summary>
        /// Equals override checking equality of actions by <see cref="Key"/> property.
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            var action = obj as SimpleQueueItem;
            if (action != null)
            {
                return action.Key.Equals(Key, StringComparison.Ordinal);
            }
            return base.Equals(obj);
        }


        /// <summary>
        /// Override required by compiler.
        /// </summary>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
