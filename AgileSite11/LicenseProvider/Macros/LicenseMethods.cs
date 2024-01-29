using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Wrapper class to provide basic licensing methods in the MacroEngine.
    /// </summary>
    internal class LicenseMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns the cached data context
        /// </summary>
        /// <param name="parameters">Macro functions parameters ([0] - feature type</param>
        [MacroMethod(typeof(bool), "Returns true if the feature is available.", 1)]
        [MacroMethodParam(0, "feature", typeof(FeatureEnum), "Feature type")]
        public static object CheckFeature(params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    FeatureEnum feature = (FeatureEnum)parameters[0];
                    return LicenseHelper.CheckFeature(RequestContext.CurrentDomain, feature);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}