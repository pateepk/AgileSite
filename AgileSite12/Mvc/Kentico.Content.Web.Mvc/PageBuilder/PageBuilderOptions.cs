namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Page builder options.
    /// </summary>
    public class PageBuilderOptions
    {
        /// <summary>
        /// Identifier of a default section for all areas.
        /// </summary>
        /// <remarks>
        /// This property is required and needs to be provided to define default section for all editable areas.
        /// In case this property is not set and <see cref="RegisterDefaultSection"/> is set to <c>true</c>,
        /// built-in default section identifier is set.
        /// </remarks>
        public string DefaultSectionIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if built-in default section should be registered.
        /// </summary>
        /// <remarks>If set to true then default section is registered.</remarks>
        public bool RegisterDefaultSection
        {
            get;
            set;
        } = true;
    }
}
