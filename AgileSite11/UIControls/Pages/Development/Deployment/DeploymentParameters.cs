using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls.Internal
{
    /// <summary>
    /// Deployment parameters object
    /// </summary>
    /// <remarks>This class is not indented to be used in custom code.</remarks>
    public class DeploymentParameters
    {
        /// <summary>
        /// Indicates whether CSS StyleSheets should be processed
        /// </summary>
        public bool SaveCss
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Layouts should be processed
        /// </summary>
        public bool SaveLayout
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Page templates should be processed
        /// </summary>
        public bool SavePageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Transformations should be processed
        /// </summary>
        public bool SaveTransformation
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Web part layout should be processed
        /// </summary>
        public bool SaveWebPartLayout
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Web part container should be processed
        /// </summary>
        public bool SaveWebPartContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Alternative form layout should be processed
        /// </summary>
        public bool SaveAlternativeFormLayout
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Form layout should be processed
        /// </summary>
        public bool SaveFormLayout
        {
            get;
            set;
        }
    }
}
