using System;
using System.Collections.Generic;

using CMS;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IPersonaChangesPropagator), typeof(PersonaChangesPropagator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{
    /// <summary>
    /// Handles propagation of changes after persona is updated.
    /// </summary>
    /// <remarks>
    /// Use <see cref="PersonaChangesPropagator"/> to obtain implementation of this interface.
    /// Changes are either propagated to underlying score or reevaluation of contact persona is executed.
    /// </remarks>
    internal interface IPersonaChangesPropagator
    {
        /// <summary>
        /// Checks changed columns of persona and propagate changes either to underlying score or contacts the persona is assigned to.
        /// </summary>
        /// <remarks>
        /// If persona was enabled, evaluation is executed for all contacts, but score is not recalculated.
        /// If persona was disabled, evaluation is executed for all contacts assigned to current persona.
        /// If persona point threshold increased, evaluation is executed for all contacts assigned to the persona.
        /// If persona point threshold decreased, evaluation is executed for all contacts not assigned to the persona.
        /// If persona display name was changed, change is propagated to underlying score display name.
        /// </remarks>
        /// <param name="changedColumns">Columns that changed in persona</param>
        /// <param name="originalPersonaScoreLimit">Points limit for persona before persona was changed</param>
        /// <param name="persona">Persona to be evaluated</param>
        void PropagatePersonaChanges(List<string> changedColumns, int originalPersonaScoreLimit, PersonaInfo persona);


        /// <summary>
        /// Reevaluates all contacts and delete persona underlying score.
        /// </summary>
        /// <param name="persona">Persona being deleted</param>
        void PropagatePersonaDeletion(PersonaInfo persona);


        /// <summary>
        /// Creates new underlying score and assign it to the persona.
        /// </summary>
        /// <param name="persona">Persona being created</param>
        void PropagatePersonaCreation(PersonaInfo persona);
    }
}