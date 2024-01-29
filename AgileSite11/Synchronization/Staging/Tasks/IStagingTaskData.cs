using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Interface for objects that are used for sending staging task to target server.
    /// </summary>
    public interface IStagingTaskData
    {
        /// <summary>
        /// Type of task, for example update, delete, add to site etc...
        /// </summary>
        TaskTypeEnum TaskType { get; set; }


        /// <summary>
        /// Object type of object for which the given staging task was created.
        /// </summary>
        string TaskObjectType { get; set; }


        /// <summary>
        /// Data of object, which will be recreated on target server.
        /// </summary>
        string TaskData { get; set; }


        /// <summary>
        /// Binary data of object which will be recreated on target server.
        /// </summary>
        string TaskBinaryData { get; }


        /// <summary>
        /// List of tasks servers separated by ';', eg. ';server1;server2;server3;'.
        /// On which servers was staging task processed already.
        /// </summary>
        string TaskServers { get; set; }


        /// <summary>
        /// Current version of Kentico.
        /// Version should be the same on source and target.
        /// </summary>
        string SystemVersion { get; set; }


        /// <summary>
        /// Guid of user who has synchronized the task.
        /// </summary>
        Guid UserGuid { get; set; }


        /// <summary>
        /// Name of user who has synchronized the task.
        /// </summary>
        string UserName { get; set; }


        /// <summary>
        /// Task groups under which object encapsulated within staging task got modified.
        /// </summary>
        /// <remarks>
        /// Cannot be a generic collection since the SOAP serializer doesn't support those.
        /// </remarks>
        TaskGroupInfo[] TaskGroups { get; set; }


        /// <summary>
        /// Serializes current IStagingTaskData.
        /// </summary>
        /// <returns>String serialized IStagingTaskData</returns>
        string Serialize();
    }
}
