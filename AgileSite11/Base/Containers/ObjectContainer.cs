using System;

namespace CMS.Base
{
    /// <summary>
    /// Object container for the Hashtable records.
    /// </summary>
    public class ObjectContainer
    {
        #region "Variables"

        /// <summary>
        /// Stored object.
        /// </summary>
        protected object mValue;

        /// <summary>
        /// Time when the object was modified the last time.
        /// </summary>
        protected DateTime mLastModified;

        /// <summary>
        /// Object key.
        /// </summary>
        protected object mKey;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the time when the object was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                return mLastModified;
            }
        }


        /// <summary>
        /// Gets / sets the object value
        /// </summary>
        public object Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                mLastModified = DateTime.Now;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Object container.
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="value">Object value</param>
        public ObjectContainer(object key, object value)
        {
            mKey = key;
            mValue = value;
            mLastModified = DateTime.Now;
        }

        #endregion
    }
}