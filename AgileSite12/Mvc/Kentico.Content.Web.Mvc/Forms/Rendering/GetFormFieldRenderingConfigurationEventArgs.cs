using CMS.Base;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Arguments of event represented by <see cref="GetFormFieldRenderingConfigurationHandler"/>.
    /// </summary>
    public class GetFormFieldRenderingConfigurationEventArgs : CMSEventArgs
    {
        private FormFieldRenderingConfiguration mConfiguration;


        /// <summary>
        /// Gets or sets the configuration to be used for rendering.
        /// </summary>
        public FormFieldRenderingConfiguration Configuration
        {
            get
            {
                if (mConfiguration == null)
                {
                    mConfiguration = SourceConfiguration.Copy();
                }
                return mConfiguration;
            }
            set
            {
                mConfiguration = value;
            }
        }


        /// <summary>
        /// Gets or sets the source configuration. The <see cref="Configuration"/> property getter makes a copy
        /// of the source configuration, if an event handler requests the configuration.
        /// </summary>
        internal FormFieldRenderingConfiguration SourceConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the target configuration. Based on whether the <see cref="Configuration"/> property
        /// has been materialized returns the modified <see cref="Configuration"/> or the original <see cref="SourceConfiguration"/>.
        /// </summary>
        internal FormFieldRenderingConfiguration TargetConfiguration
        {
            get
            {
                return mConfiguration == null ? SourceConfiguration : mConfiguration;
            }
        }


        /// <summary>
        /// Gets the form component for which the rendering configuration is being retrieved.
        /// </summary>
        public FormComponent FormComponent
        {
            get;
            internal set;
        }
    }
}
