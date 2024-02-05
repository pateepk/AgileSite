using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Automation
{
    using TypedDataSet = InfoDataSet<ObjectWorkflowTriggerInfo>;

    /// <summary>
    /// Class providing ObjectWorkflowTriggerInfo management.
    /// </summary>
    public class ObjectWorkflowTriggerInfoProvider : AbstractInfoProvider<ObjectWorkflowTriggerInfo, ObjectWorkflowTriggerInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all object workflow triggers.
        /// </summary>
        public static ObjectQuery<ObjectWorkflowTriggerInfo> GetObjectWorkflowTriggers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns ObjectWorkflowTrigger with specified ID.
        /// </summary>
        /// <param name="ObjectWorkflowTriggerId">ObjectWorkflowTrigger ID.</param>        
        public static ObjectWorkflowTriggerInfo GetObjectWorkflowTriggerInfo(int ObjectWorkflowTriggerId)
        {
            return ProviderObject.GetInfoById(ObjectWorkflowTriggerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified ObjectWorkflowTrigger.
        /// </summary>
        /// <param name="ObjectWorkflowTriggerObj">ObjectWorkflowTrigger to be set.</param>
        public static void SetObjectWorkflowTriggerInfo(ObjectWorkflowTriggerInfo ObjectWorkflowTriggerObj)
        {
            ProviderObject.SetInfo(ObjectWorkflowTriggerObj);
        }


        /// <summary>
        /// Deletes specified ObjectWorkflowTrigger.
        /// </summary>
        /// <param name="ObjectWorkflowTriggerObj">ObjectWorkflowTrigger to be deleted.</param>
        public static void DeleteObjectWorkflowTriggerInfo(ObjectWorkflowTriggerInfo ObjectWorkflowTriggerObj)
        {
            ProviderObject.DeleteInfo(ObjectWorkflowTriggerObj);
        }


        /// <summary>
        /// Deletes ObjectWorkflowTrigger with specified ID.
        /// </summary>
        /// <param name="ObjectWorkflowTriggerId">ObjectWorkflowTrigger ID.</param>
        public static void DeleteObjectWorkflowTriggerInfo(int ObjectWorkflowTriggerId)
        {
            ObjectWorkflowTriggerInfo ObjectWorkflowTriggerObj = GetObjectWorkflowTriggerInfo(ObjectWorkflowTriggerId);
            DeleteObjectWorkflowTriggerInfo(ObjectWorkflowTriggerObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Removes triggers associated to selected objects of given type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">List of IDs</param>
        public static void DeleteObjectsTriggers(string objectType, IList<int> ids)
        {
            var where = 
                new WhereCondition()
                    .WhereEquals("TriggerTargetObjectType", objectType)
                    .WhereIn("TriggerTargetObjectID", ids);

            ProviderObject.BulkDelete(where);

            // Clear all cached triggers
            Clear(null, true);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ObjectWorkflowTriggerInfo info)
        {
            var origType = info.GetOriginalValue("TriggerObjectType") as String;
            if (!String.IsNullOrEmpty(origType))
            {
                Clear(origType, true);
            }

            // Set trigger parameters
            info.SetValue("StepActionParameters", info.TriggerParameters.GetData());

            base.SetInfo(info);
            Clear(info.TriggerObjectType, true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ObjectWorkflowTriggerInfo info)
        {
            base.DeleteInfo(info);
            Clear(info.TriggerObjectType, true);
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Clears hash tables.
        /// </summary>
        public static void Clear(string objectType, bool logTask)
        {
            TriggerHelper.ClearHashtable(objectType);

            if (logTask)
            {
                ProviderObject.CreateWebFarmTask("ClearWorkflowTriggers", objectType);
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider.
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            switch (actionName)
            {
                case "ClearWorkflowTriggers":
                    Clear(data, false);
                    break;
                // If action name is not handled throw an exception
                default:
                    throw new Exception("[|" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion
    }
}
