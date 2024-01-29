using CMS.Base;
using CMS.DataEngine;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Event arguments for handler <see cref="ObjectCountCheckHandler"/>.
    /// </summary>
    internal class ObjectCountCheckEventArgs: CMSEventArgs
    {
        /// <summary>
        /// Number of <see cref="Feature"/>-related objects on current website.
        /// </summary>
        public int ObjectCount
        {
            get;
            set;
        }


        /// <summary>
        /// Feature to be checked.
        /// </summary>
        public FeatureEnum Feature
        {
            get;
            set;
        }
    }
}
