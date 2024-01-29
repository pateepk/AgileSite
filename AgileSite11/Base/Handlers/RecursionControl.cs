using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    using Storage = SafeDictionary<string, object>;

    /// <summary>
    /// Defines the section that can control and prevent recursion
    /// </summary>
    public class RecursionControl : IDisposable
    {
        #region "Variables"

        private Storage mStorage;
        private bool mContinue;

        private const string STORAGE_KEY = "storage|ControlledSection";
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Section key - Uniquely identifies the section entry
        /// </summary>
        public string Key
        {
            get;
            set;
        }


        /// <summary>
        /// If true, and the section key is set, the recursion is allowed for the calls with the same key
        /// </summary>
        public bool AllowRecursion
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the section is allowed to continue the execution
        /// </summary>
        public bool Continue
        {
            get
            {
                if (AllowRecursion)
                {
                    return true;
                }

                return mContinue;
            }
            protected set
            {
                mContinue = value;
            }
        }


        /// <summary>
        /// Storage that the section uses
        /// </summary>
        protected Storage Storage
        {
            get
            {
                return mStorage ?? (mStorage = RequestItems.Ensure<Storage>(STORAGE_KEY));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Section key - Uniquely identifies the section entry</param>
        /// <param name="allowRecursion">If true, and the section key is set, the recursion is allowed for the calls with the same key</param>
        public RecursionControl(string key, bool allowRecursion = false)
        {
            Key = key;
            AllowRecursion = allowRecursion;

            if (!allowRecursion && !String.IsNullOrEmpty(Key))
            {
                var storage = Storage;

                // Check if some code already entered the section
                if (storage[key] == null)
                {
                    storage[key] = true;

                    // First entry, allow to continue
                    Continue = true;
                }
            }
        }


        /// <summary>
        /// Disposes the object and releases recursion flag
        /// </summary>
        public void Dispose()
        {
            if (Continue)
            {
                // Remove the flag
                Storage[Key] = null;
            }
        }

        #endregion
    }
}
