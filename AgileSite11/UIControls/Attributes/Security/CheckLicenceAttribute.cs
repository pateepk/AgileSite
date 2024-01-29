using System;

using CMS.DataEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Checks the license for the given page and performs redirect if license is not valid. Works only with pages inherited from <see cref="CMSPage"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CheckLicenceAttribute : AbstractAttribute, ICMSSecurityAttribute
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


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public CheckLicenceAttribute(FeatureEnum feature)
        {
            Feature = feature;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Does the license check and performs redirect if license is not valid.
        /// </summary>
        /// <param name="page">Page from which is check performed</param>
        public void Check(CMSPage page)
        {
            page.CheckLicense(Feature);
        }

        #endregion
    }
}