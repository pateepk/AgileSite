namespace Kentico.Forms.Web.Mvc
{
    internal static class FormBuilderConstants
    {
        /// <summary>
        /// Name of the route value holding the edited form identifier.
        /// </summary>
        internal const string ROUTE_NAME = "formId";


        /// <summary>
        /// Source for the purpose of event logging.
        /// </summary>
        internal const string EVENT_LOG_SOURCE = "FormBuilder";


        /// <summary>
        /// View data key used to provide section view model.
        /// </summary>
        internal const string SECTION_MODEL_DATA_KEY = "Kentico.FormBuilder.SectionModel";


        /// <summary>
        /// Route data key used to provide zone index within the section.
        /// </summary>
        internal const string ZONE_INDEX_ROUTE_DATA_KEY = "Kentico.FormBuilder.ZoneIndex";
    }
}
