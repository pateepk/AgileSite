using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Crate for passing variables needed by triggers.
    /// </summary>
    public class TriggerOptions
    {
        private ISet<int> mIgnoreProcesses;

        
        /// <summary>
        /// Info to add to process. Required field.
        /// </summary>
        public BaseInfo Info;


        /// <summary>
        /// Allowed object type of workflow process that trigger would start. Required field.
        /// </summary>
        public String ObjectType;


        /// <summary>
        /// List of processes to ignore.
        /// </summary>
        public ISet<int> IgnoreProcesses
        {
            get
            {
                return mIgnoreProcesses ?? (mIgnoreProcesses = new HashSet<int>());
            }
        }


        /// <summary>
        /// Resolver on which macros of triggers will be resolved.
        /// </summary>
        public MacroResolver Resolver;


        /// <summary>
        /// Allowed types of triggers to be processed. Leave empty to process all types of triggers.
        /// </summary>
        public IList<WorkflowTriggerTypeEnum> Types;


        /// <summary>
        /// Function that must be passed to process trigger.
        /// </summary>
        public Func<ObjectWorkflowTriggerInfo, bool> PassFunction;


        /// <summary>
        /// The additional data for trigger that will be available in trigger via TriggerParameters[TRIGGER_DATA]
        /// </summary>
        public StringSafeDictionary<object> AdditionalData = new StringSafeDictionary<object>();
    }
}