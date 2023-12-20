using System;
using System.Linq;

using CMS.DataEngine;
using CMS.LicenseProvider;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Limits access to the element annotated with this attribute if current license is insufficient to use some of the requested features.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class RequiresFeaturesAttribute : CheckLicenseAttribute
    {
        private FeatureEnum[] RequiredFeatures
        {
            get;
        }


        public RequiresFeaturesAttribute(params FeatureEnum[] features)
        {
            RequiredFeatures = features ?? throw new ArgumentNullException(nameof(features));
        }


        /// <summary>
        /// Performs license check.
        /// </summary>
        public override bool HasValidLicense()
        {
            return RequiredFeatures.All(f => LicenseHelper.IsFeatureAvailableInBestLicense(f));
        }
    }
}
