using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.UIControls.UniMenuConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// Basic class for UI Toolbar menu.
    /// </summary>
    public class UIToolbar : CMSUserControl
    {
        #region "Custom events"

        /// <summary>
        /// Button filtered delegate.
        /// </summary>
        public delegate bool ButtonFilterEventHandler(object sender, UniMenuArgs eventArgs);


        /// <summary>
        /// Button created delegate.
        /// </summary>
        public delegate void ButtonCreatedEventHandler(object sender, UniMenuArgs eventArgs);


        /// <summary>
        /// Groups created delegate.
        /// </summary>
        public delegate void GroupsCreatedEventHandler(object sender, List<Group> groups);


        /// <summary>
        /// Group filtered delegate.
        /// </summary>
        public delegate bool GroupFilterEventHandler(object sender, UniMenuArgs eventArgs);


        /// <summary>
        /// Button filtered event handler.
        /// </summary>
        public event ButtonFilterEventHandler OnButtonFiltered;


        /// <summary>
        /// Group filtered event handler
        /// </summary>
        public event GroupFilterEventHandler OnGroupFiltered;


        /// <summary>
        /// Button creating event handler.
        /// </summary>
        public event ButtonCreatedEventHandler OnButtonCreating;


        /// <summary>
        /// Button created event handler.
        /// </summary>
        public event ButtonCreatedEventHandler OnButtonCreated;


        /// <summary>
        /// Groups created event handler.
        /// </summary>
        public event GroupsCreatedEventHandler OnGroupsCreated;

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Group created handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="groups">List of groups</param>
        protected void RaiseOnGroupsCreated(object sender, List<Group> groups)
        {
            if (OnGroupsCreated != null)
            {
                OnGroupsCreated(sender, groups);
            }
        }


        /// <summary>
        /// Button created handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="eventArgs">Event arguments</param>
        protected void RaiseOnButtonCreated(object sender, UniMenuArgs eventArgs)
        {
            if (OnButtonCreated != null)
            {
                OnButtonCreated(sender, eventArgs);
            }
        }


        /// <summary>
        /// Group filtered handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="eventArgs">Event arguments</param>
        protected bool RaiseOnGroupFiltered(object sender, UniMenuArgs eventArgs)
        {
            if (OnGroupFiltered != null)
            {
                return OnGroupFiltered(sender, eventArgs);
            }

            return true;
        }


        /// <summary>
        /// Button creating handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="eventArgs">Event arguments</param>
        protected void RaiseOnButtonCreating(object sender, UniMenuArgs eventArgs)
        {
            if (OnButtonCreating != null)
            {
                OnButtonCreating(sender, eventArgs);
            }
        }


        /// <summary>
        /// Button filtered handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="eventArgs">Event arguments</param>
        protected bool RaiseOnButtonFiltered(object sender, UniMenuArgs eventArgs)
        {
            if (OnButtonFiltered != null)
            {
                return OnButtonFiltered(sender, eventArgs);
            }

            return true;
        }

        #endregion
    }
}
