namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Represents view model containing the <see cref="PersonaInfo"/> configuration data suitable for the persona report.
    /// </summary>
    public sealed class PersonaReportConfigurationViewModel
    {
        /// <summary>
        /// Gets or sets value representing <see cref="PersonaInfo.PersonaID"/>.
        /// </summary>
        public int? PersonaID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value representing <see cref="PersonaInfo.PersonaName"/>.
        /// </summary>
        public string PersonaName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets string containing the image tag with source directing to the image of <see cref="PersonaInfo"/>.
        /// </summary>
        public string PersonaImage
        {
            get;
            set;
        }
    }
}