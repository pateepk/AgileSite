using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Manager for registered synchronization actions.
    /// </summary>
    public static class SynchronizationActionManager
    {
        // List of registered synchronization actions
        private static readonly List<SynchronizationAction> mActions = new List<SynchronizationAction>();


        /// <summary>
        /// Registers the given synchronization action
        /// </summary>
        /// <param name="execute">Method executed to perform the synchronization</param>
        /// <param name="condition">Condition under which the action is executed</param>
        public static void RegisterAction(Func<LogObjectChangeSettings, IEnumerable<ISynchronizationTask>> execute, Func<LogObjectChangeSettings, bool> condition = null)
        {
            RegisterAction(new SynchronizationAction(execute, condition));
        }


        /// <summary>
        /// Registers the given synchronization action
        /// </summary>
        /// <param name="action">Action to register</param>
        public static void RegisterAction(SynchronizationAction action)
        {
            mActions.Add(action);
        }


        /// <summary>
        /// Executes all registered synchronization actions
        /// </summary>
        /// <param name="settings">Action settings</param>
        internal static List<ISynchronizationTask> ExecuteActions(LogObjectChangeSettings settings)
        {
            var tasks = new List<ISynchronizationTask>();

            foreach (var action in mActions)
            {
                // Execute action
                var actionTasks = action.ExecuteAction(settings);

                // Collect synchronization tasks
                if (actionTasks != null)
                {
                    tasks.AddRange(actionTasks);
                }
            }

            return tasks;
        }


        /// <summary>
        /// Indicates if at least one of the synchronization actions can be executed
        /// </summary>
        /// <param name="settings">Action settings</param>
        internal static bool CanExecuteActions(LogObjectChangeSettings settings)
        {
            bool exec = false;
            foreach (var action in mActions)
            {
                exec |= action.CanBeExecuted(settings);
            }

            return exec;
        }
    }
}