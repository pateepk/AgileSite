using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;

namespace CMS.Personas
{
    /// <summary>
    /// Handles propagation of changes after persona is updated.
    /// </summary>
    /// <remarks>
    /// Changes are either propagated to underlying score or reevaluation of contact persona is executed.
    /// </remarks>
    internal class PersonaChangesPropagator : IPersonaChangesPropagator
    {
        #region "Public methods"

        /// <summary>
        /// Checks changed columns of persona and propagate changes either to underlying score or contacts the persona is assigned to.
        /// </summary>
        /// <remarks>
        /// If persona properties were changed, evaluation is executed for all contacts.
        /// </remarks>
        /// <param name="changedColumns">Columns that changed in persona</param>
        /// <param name="originalPersonaScoreLimit">Points limit for persona before persona was changed</param>
        /// <param name="persona">Persona to be evaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="changedColumns"/> or <paramref name="persona"/> is null</exception>
        public void PropagatePersonaChanges(List<string> changedColumns, int originalPersonaScoreLimit, PersonaInfo persona)
        {
            if (changedColumns == null)
            {
                throw new ArgumentNullException("changedColumns");
            }

            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            var evaluator = PersonasFactory.GetContactPersonaEvaluator();

            // If persona enabled state is changed, propagate the change to underlying score and reevaluate persona
            if (changedColumns.Contains("PersonaEnabled"))
            {
                PropagatePersonaEnabledChangeToScore(persona);

                evaluator.ReevaluateAllContactsAsync();
            }
            // If persona enable state changed, evaluation does not have to be triggered again after threshold changes
            // Otherwise evaluate the persona
            else if (changedColumns.Contains("PersonaPointsThreshold"))
            {
                evaluator.ReevaluateAllContactsAsync();
            }

            // If persona display name has changed, propagate the change to underlying score
            if (changedColumns.Contains("PersonaDisplayName"))
            {
                PropagatePersonaDisplayNameChangeToScore(persona);
            }
        }


        /// <summary>
        /// Reevaluates all contacts and delete persona underlying score.
        /// </summary>
        /// <param name="persona">Persona being deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        public void PropagatePersonaDeletion(PersonaInfo persona)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            IContactPersonaEvaluator evaluator = PersonasFactory.GetContactPersonaEvaluator();
            evaluator.ReevaluateAllContactsAsync();

            ScoreInfoProvider.DeleteScoreInfo(persona.PersonaScoreID);
        }


        /// <summary>
        /// Creates new underlying score and assign it to the persona.
        /// </summary>
        /// <param name="persona">Persona being created</param>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        public void PropagatePersonaCreation(PersonaInfo persona)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }

            var score = new ScoreInfo
            {
                ScoreEnabled = persona.PersonaEnabled,
                ScoreName = persona.PersonaName,
                ScoreDisplayName = persona.PersonaDisplayName,
                ScoreStatus = persona.PersonaEnabled ? ScoreStatusEnum.Ready : ScoreStatusEnum.RecalculationRequired,
                ScoreBelongsToPersona = true
            };

            ScoreInfoProvider.SetScoreInfo(score);
            persona.PersonaScoreID = score.ScoreID;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Copies changes of PersonaEnabled flag to ScoreInfo object.
        /// </summary>
        /// <param name="persona">PersonaInfo with changed properties</param>
        private void PropagatePersonaEnabledChangeToScore(PersonaInfo persona)
        {
            var personaScore = persona.RelatedScore;

            personaScore.ScoreStatus = ScoreStatusEnum.RecalculationRequired;
            personaScore.ScoreEnabled = persona.PersonaEnabled;

            ScoreInfoProvider.SetScoreInfo(personaScore);
        }


        /// <summary>
        /// Copies changes in PersonaDisplayName field to ScoreInfo object.
        /// </summary>
        /// <param name="persona">PersonaInfo with changed properties</param>
        private void PropagatePersonaDisplayNameChangeToScore(PersonaInfo persona)
        {
            persona.RelatedScore.ScoreDisplayName = persona.PersonaDisplayName;
            ScoreInfoProvider.SetScoreInfo(persona.RelatedScore);
        } 

        #endregion
    }
}