using System;

namespace CMS.Personas.Web.UI.Internal
{
    /// <summary>
    /// <see cref="PersonaContactHistoryInfo"/> view model.
    /// </summary>
    public sealed class PersonaContactHistoryViewModel
    {
        /// <summary>
        /// Reference to <see cref="PersonaContactHistoryInfo.PersonaContactHistoryDate"/>.
        /// </summary>
        public DateTime Date
        {
            get;
            set;
        }


        /// <summary>
        /// Reference to <see cref="PersonaContactHistoryInfo.PersonaContactHistoryPersonaID"/>.
        /// </summary>
        public int? PersonaID
        {
            get;
            set;
        }


        /// <summary>
        /// Reference to <see cref="PersonaContactHistoryInfo.PersonaContactHistoryContacts"/>.
        /// </summary>
        public int Contacts
        {
            get;
            set;
        }
    }
}