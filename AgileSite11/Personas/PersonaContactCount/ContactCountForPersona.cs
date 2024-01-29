namespace CMS.Personas.Internal
{
    /// <summary>
    /// Container responsible for storing what number of contacts belong to a persona or don't belong to any persona.
    /// </summary>
    public class ContactCountForPersona
    {
        /// <summary>
        /// ID of a persona the contacts belong to. Is <c>null</c> if the contacts don't belong to any persona.
        /// </summary>
        public int? PersonaID
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts that belong to a given persona.
        /// </summary>
        public int ContactsCount
        {
            get;
            set;
        }
    }
}
