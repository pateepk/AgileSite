using System;
using System.Collections;

using CMS.Core;

namespace CMS.Tests
{
    /// <summary>
    /// Fake settings service
    /// </summary>
    public class FakeSettingsService : ISettingsService
    {
        private readonly Hashtable mKeys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Always returns true
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Settings key
        /// </summary>
        /// <param name="keyName">Settings key</param>
        public string this[string keyName]
        {
            get
            {
                return mKeys[keyName].ToString();
            }
            set
            {
                mKeys[keyName] = value;
            }
        }
    }
}
