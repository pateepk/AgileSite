using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    using TriggerList = List<ITrigger>;
    using TriggerDictionary = StringSafeDictionary<List<ITrigger>>;

    /// <summary>
    /// Class for managing automation triggers.
    /// </summary>
    public class TriggerHelper : AbstractHelper<TriggerHelper>
    {
        #region "Variables"

        private static volatile CMSStatic<TriggerDictionary> cachedTriggers = new CMSStatic<TriggerDictionary>(() => new TriggerDictionary());
        private static readonly CMSStatic<TriggerList> customTriggers = new CMSStatic<TriggerList>(() => new TriggerList());

        private readonly object syncLock = new object();

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Processes all triggers of given type on given BaseInfo.
        /// </summary>
        public static void ProcessTriggers(TriggerOptions options)
        {
            HelperObject.ProcessTriggersInternal(options);
        }


        /// <summary>
        /// Processes all triggers of given type on given options collection.
        /// </summary>
        public static void ProcessTriggers(IEnumerable<TriggerOptions> options)
        {
            HelperObject.ProcessTriggersInternal(options);
        }


        /// <summary>
        /// Registers custom trigger.
        /// </summary>
        /// <param name="trigger">Trigger to register</param>
        public static void RegisterCustomTrigger(ITrigger trigger)
        {
            HelperObject.RegisterCustomTriggerInternal(trigger);
        }


        /// <summary>
        /// Checks if triggers of given types are present.
        /// </summary>
        public static bool HasTriggerTypes(string triggerType)
        {
            return GetTriggers(triggerType).Any();
        }


        /// <summary>
        /// Gets all cached triggers.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static TriggerList GetTriggers(string objectType)
        {
            return HelperObject.GetTriggersInternal(objectType);
        }


        /// <summary>
        /// Clears cached triggers for given object type.
        /// </summary>
        public static void ClearHashtable(string objectType)
        {
            HelperObject.ClearHashtableInternal(objectType);
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Clears cached triggers for given object type.
        /// </summary>
        protected virtual void ClearHashtableInternal(string objectType)
        {
            if (objectType == null)
            {
                cachedTriggers.Value.Clear();
            }
            else
            {
                if (cachedTriggers.Value.ContainsKey(objectType))
                {
                    cachedTriggers.Value.Remove(objectType);
                }
            }
        }


        /// <summary>
        /// Registers custom trigger.
        /// </summary>
        /// <param name="trigger">Trigger to register</param>
        protected virtual void RegisterCustomTriggerInternal(ITrigger trigger)
        {
            customTriggers.Value.Add(trigger);
        }


        /// <summary>
        /// Processes all triggers of given type on given options collection.
        /// </summary>
        protected virtual void ProcessTriggersInternal(IEnumerable<TriggerOptions> options)
        {
            options = options.ToList();

            if (options.Any(o => (o.ObjectType == null) || (o.Info == null)))
            {
                throw new InvalidOperationException("Field Info and ObjectType is null in some of the options passed.");
            }

            // Process custom triggers first
            foreach (var trigger in customTriggers.Value)
            {
                trigger.Process(options);
            }

            // Load triggers or get them from cache
            foreach (var group in options.GroupBy(o => o.ObjectType))
            {
                var triggers = GetTriggers(group.Key);

                // Process triggers
                foreach (var trigger in triggers)
                {
                    trigger.Process(group);
                }
            }
        }


        /// <summary>
        /// Processes all triggers of given type on given BaseInfo.
        /// </summary>
        protected virtual void ProcessTriggersInternal(TriggerOptions options)
        {
            ProcessTriggersInternal(new[] { options });
        }


        /// <summary>
        /// Gets all cached triggers.
        /// </summary>
        /// <param name="objectType">Object type</param>
        protected virtual TriggerList GetTriggersInternal(string objectType)
        {
            var triggers = cachedTriggers.Value;

            if (triggers.ContainsKey(objectType))
            {
                return triggers[objectType];
            }

            lock (syncLock)
            {
                if (triggers.ContainsKey(objectType))
                {
                    return triggers[objectType];
                }

                TriggerList triggerList = new TriggerList();

                ObjectWorkflowTriggerInfoProvider.GetObjectWorkflowTriggers()
                    .WhereEquals("TriggerObjectType", objectType)
                    .WhereIn("TriggerWorkflowID", new IDQuery<WorkflowInfo>()
                                                            .WhereEquals("WorkflowType", (int)WorkflowTypeEnum.Automation)
                                                            .WhereTrue("WorkflowEnabled"))
                    .ForEachObject(trigger =>
                    {
                        triggerList.Add(new Trigger(trigger));
                    });

                triggers.Add(objectType, triggerList);
            }

            return triggers[objectType];
        }

        #endregion
    }
}