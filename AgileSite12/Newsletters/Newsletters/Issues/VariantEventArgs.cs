using System;

namespace CMS.Newsletters
{
    /// <summary>
    /// Additional arguments for variant slider and variant dialog events.
    /// </summary>
    public class VariantEventArgs : EventArgs
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName 
        {
            get;
            private set;
        }


        /// <summary>
        /// Issue ID
        /// </summary>
        public int ID
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="displayName">Display name</param>
        public VariantEventArgs(string displayName)
        {
            DisplayName = displayName;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="displayName">Display name</param>
        /// <param name="id">ID</param>
        public VariantEventArgs(string displayName, int id)
        {
            DisplayName = displayName;
            ID = id;
        }
    }
}
