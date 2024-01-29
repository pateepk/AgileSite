using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Personas
{
    /// <summary>
    /// Class providing PersonaNodeInfo management.
    /// </summary>
    public class PersonaNodeInfoProvider : AbstractInfoProvider<PersonaNodeInfo, PersonaNodeInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all PersonaNodeInfo bindings.
        /// </summary>
        public static ObjectQuery<PersonaNodeInfo> GetPersonaNodes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns PersonaNodeInfo binding structure.
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>  
        public static PersonaNodeInfo GetPersonaNodeInfo(int personaId, int nodeId)
        {
            return ProviderObject.GetPersonaNodeInfoInternal(personaId, nodeId);
        }


        /// <summary>
        /// Sets specified PersonaNodeInfo.
        /// </summary>
        /// <param name="infoObj">PersonaNodeInfo to set</param>
        public static void SetPersonaNodeInfo(PersonaNodeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified PersonaNodeInfo binding.
        /// </summary>
        /// <param name="infoObj">PersonaNodeInfo object</param>
        public static void DeletePersonaNodeInfo(PersonaNodeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes PersonaNodeInfo binding.
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>  
        public static void RemovePersonaFromNode(int personaId, int nodeId)
        {
            ProviderObject.RemovePersonaFromNodeInternal(personaId, nodeId);
        }


        /// <summary>
        /// Creates PersonaNodeInfo binding. 
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>   
        public static void AddPersonaToNode(int personaId, int nodeId)
        {
            ProviderObject.AddPersonaToNodeInternal(personaId, nodeId);
        }


        /// <summary>
        /// Deletes all bindings between specified nodes and personas.
        /// </summary>
        /// <param name="nodeIDs">Bindings between these nodes and personas in <paramref name="personasIDs"/> will be deleted</param>
        /// <param name="personasIDs">Bindings between these personas and nodes in <paramref name="nodeIDs"/> will be deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="nodeIDs"/> or <paramref name="personasIDs"/> is null</exception>
        public static void RemovePersonasFromDocuments(List<int> nodeIDs, List<int> personasIDs)
        {
            ProviderObject.RemovePersonasFromDocumentsInternal(nodeIDs, personasIDs);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the PersonaNodeInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>  
        protected virtual PersonaNodeInfo GetPersonaNodeInfoInternal(int personaId, int nodeId)
        {
            var bindings = GetPersonaNodes()
                .Where("PersonaID", QueryOperator.Equals, personaId)
                .Where("NodeID", QueryOperator.Equals, nodeId)
                .TopN(1);

            return bindings.FirstOrDefault();
        }


        /// <summary>
        /// Deletes PersonaNodeInfo binding.
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>  
        protected virtual void RemovePersonaFromNodeInternal(int personaId, int nodeId)
        {
            var infoObj = GetPersonaNodeInfo(personaId, nodeId);
            if (infoObj != null)
            {
                DeletePersonaNodeInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates PersonaNodeInfo binding. 
        /// </summary>
        /// <param name="personaId">Persona ID</param>
        /// <param name="nodeId">Node ID</param>   
        protected virtual void AddPersonaToNodeInternal(int personaId, int nodeId)
        {
            // Create new binding
            var infoObj = new PersonaNodeInfo();
            infoObj.PersonaID = personaId;
            infoObj.NodeID = nodeId;

            // Save to the database
            SetPersonaNodeInfo(infoObj);
        }


        /// <summary>
        /// Deletes all bindings between specified nodes and personas.
        /// </summary>
        /// <param name="nodeIDs">Bindings between these nodes and personas in <paramref name="personasIDs"/> will be deleted</param>
        /// <param name="personasIDs">Bindings between these personas and nodes in <paramref name="nodeIDs"/> will be deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="nodeIDs"/> or <paramref name="personasIDs"/> is null</exception>
        protected virtual void RemovePersonasFromDocumentsInternal(List<int> nodeIDs, List<int> personasIDs)
        {
            if (nodeIDs == null)
            {
                throw new ArgumentNullException("nodeIDs");
            }
            if (personasIDs == null)
            {
                throw new ArgumentNullException("personasIDs");
            }

            var where = 
                new WhereCondition()
                    .WhereIn("PersonaID", personasIDs)
                    .WhereIn("NodeID", nodeIDs);

            BulkDelete(where);
        }

        #endregion
    }
}