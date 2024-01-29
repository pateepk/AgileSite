using System;
using System.Collections.Generic;

namespace CMS.Personas
{
    /// <summary>
    /// Implementation of mass action for deleting assignment between multiple personas and multiple nodes.
    /// </summary>
    internal class MultipleDocumentsUntagger : IMultipleDocumentsAction
    {
        /// <summary>
        /// Deletes all bindings between specified nodes and personas.
        /// </summary>
        /// <param name="nodeIDs">Bindings between these nodes and personas in <paramref name="personasIDs"/> will be deleted</param>
        /// <param name="personasIDs">Bindings between these personas and nodes in <paramref name="nodeIDs"/> will be deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="nodeIDs"/> or <paramref name="personasIDs"/> is null</exception>
        public void PerformAction(List<int> nodeIDs, List<int> personasIDs)
        {
            if (nodeIDs == null)
            {
                throw new ArgumentNullException("nodeIDs");
            }
            if (personasIDs == null)
            {
                throw new ArgumentNullException("personasIDs");
            }

            PersonaNodeInfoProvider.RemovePersonasFromDocuments(nodeIDs, personasIDs);
        }
    }
}