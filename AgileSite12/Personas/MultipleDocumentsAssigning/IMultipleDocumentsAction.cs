using System.Collections.Generic;

namespace CMS.Personas
{
    /// <summary>
    /// Denotes contract for performing actions on multiple personas and multiple nodes.
    /// </summary>
    public interface IMultipleDocumentsAction
    {
        /// <summary>
        /// Performs action on multiple personas and multiple nodes.
        /// </summary>
        /// <param name="nodeIDs">Nodes action will be performed on</param>
        /// <param name="personasIDs">Personas the action will be performed on</param>
        void PerformAction(List<int> nodeIDs, List<int> personasIDs);
    }
}