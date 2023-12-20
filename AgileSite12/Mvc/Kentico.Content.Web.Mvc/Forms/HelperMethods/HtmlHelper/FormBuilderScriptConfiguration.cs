using Kentico.Builder.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates data sent to client and used by Form builder's script.
    /// </summary>
    /// <seealso cref="Internal.HtmlHelperExtensions.FormBuilderScripts"/>
    internal sealed class FormBuilderScriptConfiguration : BuilderScriptConfiguration
    {
        /// <summary>
        /// Identifier of the form for which the Form builder is initialized.
        /// </summary>
        public int FormIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Client ID of the element to render the properties editor into.
        /// </summary>
        public string PropertiesEditorClientId
        {
            get;
            set;
        }


        /// <summary>
        /// Client ID of the element to render the save message into.
        /// </summary>
        public string SaveMessageClientId
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving the properties editor markup.
        /// </summary>
        public string PropertiesEditorEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving all the registered <see cref="ValidationRule"/>s.
        /// </summary>
        public string ValidationRuleMetadataEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving <see cref="ValidationRule"/>'s markup.
        /// </summary>
        public string ValidationRuleMarkupEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving Visibility condition markup.
        /// </summary>
        public string VisibilityConditionMarkupEndpoint
        {
            get;
            set;
        }
    }
}
