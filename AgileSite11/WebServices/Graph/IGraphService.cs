using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers.UniGraphConfig;

namespace CMS.WebServices
{
    /// <summary>
    /// Graph service contract.
    /// </summary>
    [ServiceContract]
    public interface IGraphService
    {
        /// <summary>
        /// Get node specification for refresh purpose.
        /// </summary>
        /// <param name="id">Node ID</param>
        [OperationContract]
        ServiceResponse<Node> GetNode(string id);


        /// <summary>
        /// Sets new position to specified node.
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        [OperationContract]
        ServiceResponse SetNodePosition(string id, int x, int y);


        /// <summary>
        /// Sets node name.
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="name">New name</param>
        [OperationContract]
        ServiceResponse SetNodeName(string id, string name);


        /// <summary>
        /// Sets switch case name.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="caseId">Switch case ID</param>
        /// <param name="name">New name</param>
        [OperationContract]
        ServiceResponse SetSwitchCaseName(string nodeId, string caseId, string name);


        /// <summary>
        /// Creates a new connection between specified nodes.
        /// </summary>
        /// <param name="startNodeId">Start node ID</param>
        /// <param name="endNodeId">End node ID</param>
        /// <param name="sourcePointGuid">Source point GUID</param>
        [OperationContract]
        ServiceResponse<int> CreateConnection(string startNodeId, string endNodeId, string sourcePointGuid);


        /// <summary>
        /// Deletes specified connection.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        [OperationContract]
        ServiceResponse RemoveConnection(string connectionId);


        /// <summary>
        /// Moves connection start to specified node.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="startNodeId">New start node ID</param>
        /// <param name="sourcePointGuid">New source point GUID</param>
        [OperationContract]
        ServiceResponse EditConnectionStart(string connectionId, string startNodeId, string sourcePointGuid);


        /// <summary>
        /// Moves connection end to specified node.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="endNodeId">End node ID</param>
        [OperationContract]
        ServiceResponse EditConnectionEnd(string connectionId, string endNodeId);


        /// <summary>
        /// Adds switch case to multi-choice node.
        /// </summary> 
        /// <param name="nodeId">Node ID</param>
        [OperationContract]
        ServiceResponse<Node> AddSwitchCase(string nodeId);


        /// <summary>
        /// Removes switch case from multi-choice node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="caseId">Case ID</param>
        [OperationContract]
        ServiceResponse<Node> RemoveSwitchCase(string nodeId, string caseId);


        /// <summary>
        /// Creates new node of specified type.
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="actionId">Action type</param>
        /// <param name="parentId">Parent ID</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        [OperationContract]
        ServiceResponse<Node> CreateNode(string type, string actionId, string parentId, int x, int y);


        /// <summary>
        /// Creates new node of specified type.
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="actionId">Action type</param>
        /// <param name="parentId">Parent ID</param>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <param name="splitConnectionsIDs">Connections new node should be attached on</param>
        [OperationContract]
        GraphPartialRefresh CreateNodeOnConnections(string type, string actionId, string parentId, int x, int y, List<string> splitConnectionsIDs);


        /// <summary>
        /// Removes specified node and all its connections.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        [OperationContract]
        ServiceResponse RemoveNode(string nodeId);
    }
}
