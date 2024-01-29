using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Personas
{
    /// <summary>
    /// Implementation of mass action for assigning multiple personas to multiple nodes.
    /// </summary>
    internal class MultipleDocumentsTagger : IMultipleDocumentsAction
    {
        /// <summary>
        /// Assigns all given personas to the given nodes.
        /// </summary>
        /// <param name="nodeIDs">Personas in <paramref name="personasIDs"/> will be assigned to nodes in this paramter</param>
        /// <param name="personasIDs">Personas in this parameter will be assigned to nodes in <paramref name="nodeIDs"/></param>
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


            var newPersonaNodeInfos = from nodeID in nodeIDs
                                      from personaID in personasIDs
                                      select new PersonaNodeInfo()
                                      {
                                          NodeID = nodeID,
                                          PersonaID = personaID
                                      };

            foreach (var personaNodeInfo in newPersonaNodeInfos)
            {
                personaNodeInfo.Insert();
            }
        }
    }
}