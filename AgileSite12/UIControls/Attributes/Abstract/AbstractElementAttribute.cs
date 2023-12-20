using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract class for the element attributes.
    /// </summary>
    public abstract class AbstractElementAttribute : AbstractSecurityAttribute
    {
        #region "Properties"

        /// <summary>
        /// Feature to check.
        /// </summary>
        public FeatureEnum Feature
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractElementAttribute()
        {
            Feature = FeatureEnum.Unknown;
        }


        /// <summary>
        /// Checks the license, returns true if the license is valid against current domain.
        /// </summary>
        public bool CheckLicense()
        {
            if (Feature == FeatureEnum.Unknown)
            {
                return true;
            }

            // Check the license
            string currentDomain = RequestContext.CurrentDomain;
            if (!String.IsNullOrEmpty(currentDomain))
            {
                return LicenseHelper.CheckFeature(currentDomain, Feature);
            }

            return true;
        }


        /// <summary>
        /// Checks if the element is visible. Returns true if it is allowed to be shown.
        /// </summary>
        public bool CheckVisibility()
        {
            return CheckLicense() && CheckSecurity();
        }

        #endregion
    }
}