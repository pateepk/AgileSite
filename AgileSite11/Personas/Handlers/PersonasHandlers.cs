using System;
using System.Linq;
using System.Text;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Personas.Handlers
{
    /// <summary>
    /// Personas events handlers.
    /// </summary>
    internal static class PersonasHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            MetaFileInfo.TYPEINFO.Events.Delete.After += DeletePersonaPicture;
            RuleInfo.PERSONARULETYPEINFO.Events.Delete.After += EvaluateContactsPersonaAfterRuleDelete;
            ScoringEvents.RecalculateScoreForAllContacts.After += ReevaluateAllContacts;
            ScoringEvents.RecalculateAfterContactActionsBatch.After += ReevaluateContacts;
        }


        /// <summary>
        /// Evaluate contacts for persona. If deleted rule has negative value, the persona contact base is expanded, if rule
        /// value is positive, the persona contact base is reduced.
        /// </summary>
        private static void EvaluateContactsPersonaAfterRuleDelete(object sender, ObjectEventArgs e)
        {
            RuleInfo rule = (RuleInfo)e.Object;

            if (!rule.RuleBelongsToPersona)
            {
                return;
            }

            ScoreInfo score = ScoreInfoProvider.GetScoreInfo(rule.RuleScoreID);
            if ((score == null) || !score.ScoreBelongsToPersona)
            {
                return;
            }

            PersonaInfo persona = PersonaInfoProvider.GetPersonaInfoByScoreId(score.ScoreID);
            if ((persona == null) || !persona.PersonaEnabled)
            {
                return;
            }

            // Reevaluate persona for all contacts
            PersonasFactory.GetContactPersonaEvaluator().ReevaluateAllContactsAsync();
        }


        /// <summary>
        /// Sets persona picture to empty Guid, if refers to recently deleted Meta file.
        /// </summary>
        private static void DeletePersonaPicture(object sender, ObjectEventArgs e)
        {
            MetaFileInfo metaFile = e.Object as MetaFileInfo;
            var personaDataClass = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.PERSONA);

            // Delete picture only if MetaFile was Thumbnail, because only thumbnails can be assigned as persona picture and only if MetaFile belonged to persona module
            if ((metaFile == null) || (personaDataClass == null) || (metaFile.MetaFileGroupName != ObjectAttachmentsCategories.THUMBNAIL) || 
                (metaFile.MetaFileObjectType != "cms.class") || (metaFile.MetaFileObjectID != personaDataClass.ClassID))
            {
                return;
            }

            var relatedPersonas = PersonaInfoProvider.GetPersonas().WhereEquals("PersonaPictureMetafileGUID", metaFile.MetaFileGUID);
            foreach (var persona in relatedPersonas)
            {
                persona.PersonaPictureMetafileGUID = null;
                PersonaInfoProvider.SetPersonaInfo(persona);
            }
        }


        /// <summary>
        /// Reevaluates persona for the contact given in event arguments.
        /// </summary>
        private static void ReevaluateContacts(object sender, RecalculateAfterContactActionsBatchEventArgs e)
        {
            if (!HasEnabledPersonas())
            {
                return;
            }

            var contactPersonaEvaluator = PersonasFactory.GetContactPersonaEvaluator();

            contactPersonaEvaluator.ReevaluateContacts(e.ContactIDs);
        }


        private static bool HasEnabledPersonas()
        {
            return CacheHelper.Cache((cs) =>
            {
                cs.CacheDependency = CacheHelper.GetCacheDependency(PredefinedObjectType.PERSONA + "|all");
                return PersonaInfoProvider.GetPersonas().WhereEquals("PersonaEnabled", true).Count > 0;
            }, new CacheSettings(10, "PersonasHandlers.HasPersona"));
        }


        /// <summary>
        /// Reevaluates persona for all contacts that are related to score given in event arguments.
        /// </summary>
        private static void ReevaluateAllContacts(object sender, ScoreEventArgs e)
        {
            var score = e.Score;
            if (score.ScoreBelongsToPersona)
            {
                var persona = PersonaInfoProvider.GetPersonaInfoByScoreId(score.ScoreID);
                if ((persona == null) || !persona.PersonaEnabled)
                {
                    return;
                }

                PersonasFactory.GetContactPersonaEvaluator().ReevaluateAllContactsAsync();
            }
        }
    }
}