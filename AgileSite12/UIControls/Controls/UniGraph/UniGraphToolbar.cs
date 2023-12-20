using System;
using System.Collections.Generic;

using CMS.UIControls.UniMenuConfig;

namespace CMS.UIControls
{
    /// <summary>
    /// UniGraph toolbar class
    /// </summary>
    public class UniGraphToolbar : CMSUserControl
    {
        #region "Public properties"

        /// <summary>
        /// Nodes user should be able to add to graph from toolbar.
        /// </summary>
        protected List<Item> NodeItems
        {
            get;
            set;
        }


        /// <summary>
        /// Name of JS object used for deleting items.
        /// </summary>
        public virtual string JsGraphObject
        {
            get;
            set;
        }


        /// <summary>
        /// Path to control containing nodes to be added.
        /// </summary>
        protected virtual string NodesControlPath
        {
            get;
            set;
        }


        /// <summary>
        /// List of menu groups.
        /// </summary>
        public virtual List<Group> ToolbarGroups
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string of group containing nodes.
        /// </summary>
        public virtual string NodesGroupResourceString
        {
            get;
            set;
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (NodeItems.Count > 0)
            {
                KeyValuePair<string, object> nodesValue = new KeyValuePair<string, object>("NodesMenuItems", NodeItems);
                CMSUserControl nodesControl = CreateControl(NodesControlPath, nodesValue);
                Group nodesGroup = CreateGroup(nodesControl, NodesGroupResourceString);
                ToolbarGroups.Insert(0, nodesGroup);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds group to list of menu groups.
        /// </summary>
        /// <param name="controlPath">Path of control representing group</param>
        /// <param name="controlValue">Object to be sent to control</param>
        /// <param name="groupCaption">Caption of group</param>
        protected void AppendGroup(string controlPath, KeyValuePair<string, object> controlValue, string groupCaption)
        {
            CMSUserControl control = CreateControl(controlPath, controlValue);
            Group group = CreateGroup(control, groupCaption);
            ToolbarGroups.Add(group);
        }


        /// <summary>
        /// Creates control and sends him given value.
        /// </summary>
        /// <param name="path">Path to control to be created</param>
        /// <param name="controlValue">Value to be sent</param>
        /// <returns>New control representing group in uniMenu</returns>
        private CMSUserControl CreateControl(string path, KeyValuePair<string, object> controlValue)
        {
            int id = ToolbarGroups.Count;
            CMSUserControl nodesControl = (CMSUserControl)LoadUserControl(path);
            nodesControl.SetValue(controlValue.Key, controlValue.Value);
            nodesControl.ID = "groupControl" + id;
            nodesControl.ShortID = "gc" + id;
            return nodesControl;
        }


        /// <summary>
        /// Created group based on given control and caption.
        /// </summary>
        /// <param name="control">Control representing group</param>
        /// <param name="caption">Caption of group</param>
        /// <returns>New group in uniMenu</returns>
        private Group CreateGroup(CMSUserControl control, string caption)
        {
            Group group = new Group();
            group.Caption = GetString(caption);
            group.Control = control;
            return group;
        }

        #endregion
    }
}
