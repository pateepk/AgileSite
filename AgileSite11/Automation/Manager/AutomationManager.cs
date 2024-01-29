using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Automation
{
    /// <summary>
    /// Class for managing the marketing automation.
    /// </summary>
    public class AutomationManager : AbstractAutomationManager<BaseInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. " +
                  "For inheritance, use constructor AutomationManager(user). " +
                  "If you need an instance of this object use static GetInstance(user) method.")]
        public AutomationManager()
        {
            Init(null);
        }


        /// <summary>
        /// Constructor - Creates automation manager.
        /// </summary>
        /// <param name="user">User</param>
        protected AutomationManager(UserInfo user)
        {
            Init(user);
        }


        /// <summary>
        /// Initializes the manager
        /// </summary>
        /// <param name="user">User to run with</param>
        private void Init(UserInfo user)
        {
            EventLogSource = "Automation";
            User = user;
            ApplicationUrl = URLHelper.GetAbsoluteUrl("~/");
            if (ApplicationUrl != null)
            {
                ApplicationUrl = ApplicationUrl.TrimEnd('/');
            }
        }


        /// <summary>
        /// Changes the manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public override void ChangeManagerTypeTo(Type newType)
        {
            ChangeManagerType<AutomationManager>(newType);
        }


        /// <summary>
        /// Gets the instance of the manager for the given user.
        /// </summary>
        /// <remarks>
        /// When calling other methods of <see cref="AutomationManager"/> (for example <see cref="AbstractAutomationManager{InfoType}.StartProcess(InfoType, int, ObjectWorkflowTriggerInfo)"/>) permissions are checked for the given user.
        /// If user is not granted required permissions automation process doesn't work correctly.
        /// </remarks>
        /// <param name="user">User whose context is used. See remarks for more information.</param>
        public static AutomationManager GetInstance(UserInfo user)
        {
            var man = LoadManager<AutomationManager>();
            man.User = user;

            return man;
        }
    }
}