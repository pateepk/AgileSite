namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// Represents view model for number of contacts in persona
    /// </summary>
    public class ContactsGroupedByPersonaViewModel
    {
        /// <summary>
        /// Name of a persona the contacts belong to.
        /// </summary>
        public string PersonaName
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts in a persona.
        /// </summary>
        public int NumberOfContacts
        {
            get;
            set;
        }
    }
}
