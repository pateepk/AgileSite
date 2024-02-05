using System.Collections.Generic;

using CMS;
using CMS.Personas;
using CMS.Personas.Internal;

[assembly: RegisterImplementation(typeof(IPersonaSnapshooter), typeof(PersonaSnapshooter), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas.Internal
{
    /// <summary>
    /// Provides method for obtaining current state of contact/persona distribution.
    /// </summary>
    public interface IPersonaSnapshooter
    {
        /// <summary>
        /// Gets collection of <see cref="PersonaContactHistoryInfo"/> containing the statistics of contact/persona distribution.
        /// </summary>
        /// <returns>Collection of <see cref="PersonaContactHistoryInfo"/> containing the statistics of contact/persona distribution</returns>
        IEnumerable<PersonaContactHistoryInfo> GetSnapshotOfCurrentState();
    }
}