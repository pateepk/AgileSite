using System;

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// View model for contacts persona.
    /// </summary>
    public class ContactPersonaViewModel
    {
        /// <summary>
        /// Persona name
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Persona description
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Persona image url
        /// </summary>
        public string ImageUrl
        {
            get;
            set;
        }
    }
}