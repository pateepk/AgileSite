using System.Collections.Generic;

using CMS.Base;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc.Widgets
{
    /// <summary>
    /// Arguments of event represented by <see cref="GetFormWidgetRenderingConfigurationHandler"/>.
    /// </summary>
    public class GetFormWidgetRenderingConfigurationEventArgs : CMSEventArgs
    {
        private FormWidgetRenderingConfiguration mConfiguration;


        /// <summary>
        /// Gets or sets the configuration to be used for rendering.
        /// </summary>
        public FormWidgetRenderingConfiguration Configuration
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
        internal FormWidgetRenderingConfiguration SourceConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the target configuration. Based on whether the <see cref="Configuration"/> property
        /// has been materialized returns the modified <see cref="Configuration"/> or the original <see cref="SourceConfiguration"/>.
        /// </summary>
        internal FormWidgetRenderingConfiguration TargetConfiguration
        {
            get
            {
                return mConfiguration == null ? SourceConfiguration : mConfiguration;
            }
        }


        /// <summary>
        /// Gets the <see cref="BizFormInfo"/> for which the rendering configuration is being retrieved.
        /// </summary>
        public BizFormInfo Form
        {
            get;
            internal set;
        }


        /// <summary>
        /// Collection of actually rendered form components.
        /// </summary>
        public IEnumerable<FormComponent> FormComponents
        {
            get;
            internal set;
        }


        /// <summary>
        /// <see cref="FormWidgetProperties"/> of currently displayed form.
        /// </summary>
        /// <remarks>Property is <c>null</c> within a form submit action.</remarks>
        public FormWidgetProperties FormWidgetProperties
        {
            get;
            internal set;
        }
    }
}
