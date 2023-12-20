using CMS;
using CMS.Personas.Web.UI;

[assembly: RegisterImplementation(typeof(IPersonaViewModelService), typeof(PersonaViewModelService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Provides contacts persona view model.
    /// </summary>
    internal interface IPersonaViewModelService
    {
        /// <summary>
        /// Provides contacts persona view model.
        /// </summary>
        /// <param name="contactId">Contact ID.</param>
        /// <returns>View model of given contact persona.</returns>
        ContactPersonaViewModel GetPersonaViewModel(int contactId);
    }
}