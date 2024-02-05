using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Personas;
using CMS.Personas.Handlers;

[assembly: RegisterModule(typeof(PersonasModule))]

namespace CMS.Personas
{
    /// <summary>
    /// Represents entry point of the Persona Based Recommendations (or shortly Personas) module. This module allows CMS users to define Personas representing
    /// groups of visitors/customer. Visitors are then automatically assigned to the defined Personas and different content can be displayed to them.
    /// </summary>
    internal class PersonasModule : Module
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PersonasModule() :
            base(new PersonasModuleMetadata())
        {
        }


        /// <summary>
        /// Handles the module initialization.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            PersonasHandlers.Init();

            // This macro is not used in system by default.
            // Selects only documents tagged with specified persona.
            // Use of where condition is faster than order by. Use where condition when there is enough documents tagged for all personas.
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("PersonaDocumentWhereCondition", context =>
            {
                ContactInfo currentContact = ContactManagementContext.CurrentContact;
                // Current contact can be null if Online marketing is not enabled or user does not have EMS license
                if (currentContact != null)
                {
                    var persona = PersonasFactory.GetPersonaService().GetPersonaForContact(currentContact);
                    if (persona != null)
                    {
                        return "NodeID IN (SELECT NodeID FROM Personas_PersonaNode WHERE PersonaID = " + persona.PersonaID + ")";
                    }
                }

                return "1=1";
            });

            // Is used in Persona-based Recommendations web part and widget.
            // Returns documents tagged with specified persona in the first place.
            // Adds documents from other personas or untagged documents when there is not enough documents for specified persona.
            MacroContext.GlobalResolver.SetHiddenNamedSourceData("PersonaDocumentOrderBy", context =>
            {
                ContactInfo currentContact = ContactManagementContext.CurrentContact;
                // Current contact can be null if Online marketing is not enabled or user does not have EMS license
                if (currentContact != null)
                {
                    var persona = PersonasFactory.GetPersonaService().GetPersonaForContact(currentContact);
                    if (persona != null)
                    {
                        return "CASE WHEN EXISTS (SELECT NodeID FROM Personas_PersonaNode WHERE PersonaID = " + persona.PersonaID + " AND NodeID = DocumentNodeID) THEN 1 ELSE 2 END";
                    }
                }

                // When user is not assigned to any persona, something still has to be returned to allow combining with another order by
                // (SELECT null) is returned, because it can be used in Window functions (OVER clause) to denote no particular ordering
                return "(SELECT null)";
            });

            // Register metadata containing macro->dataquery translator to speed up contact group recalculation
            MacroRuleMetadataContainer.RegisterMetadata(new MacroRuleMetadata("ContactIsInPersona", new ContactIsInPersonaInstanceTranslator(), 
                affectingActivities: null,
                affectingAttributes: new List<string>(1)
                {
                    "contactpersonaid"
                })
            );
        }
    }
}