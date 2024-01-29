using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using AjaxControlToolkit;

namespace CMS.Base.Web.UI.DragAndDrop
{
    /// <summary>
    /// Drag and drop extender class.
    /// </summary>
    [Designer(typeof(DragAndDropDesigner))]
    [TargetControlType(typeof(WebControl))]
    [ClientScriptResource("CMSExtendedControls.DragAndDropBehavior", "CMS.Base.Web.UI.DragAndDrop.Empty")]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [RequiredScript(typeof(TimerScript))]
    [RequiredScript(typeof(DragDropScripts))]
    [RequiredScript(typeof(DragPanelExtender))]
    public class DragAndDropExtender : ExtenderControlBase
    {
        /// <summary>
        /// If true, the layout is a flow layout (absolutely positioned elements)
        /// </summary>
        [ExtenderControlProperty]
        public bool FlowLayout
        {
            get
            {
                return GetPropertyValue("FreeLayout", false);
            }
            set
            {
                SetPropertyValue("FreeLayout", value);
            }
        }


        /// <summary>
        /// Class defining the dragging item.
        /// </summary>
        [ExtenderControlProperty]
        public string DragItemClass
        {
            get
            {
                return GetPropertyValue("DragItemClass", string.Empty);
            }
            set
            {
                SetPropertyValue("DragItemClass", value);
            }
        }


        /// <summary>
        /// Class defining the item handle.
        /// </summary>
        [ExtenderControlProperty]
        public string DragItemHandleClass
        {
            get
            {
                return GetPropertyValue("DragItemHandleClass", string.Empty);
            }
            set
            {
                SetPropertyValue("DragItemHandleClass", value);
            }
        }


        /// <summary>
        /// ID of the drop area cue.
        /// </summary>
        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        public string DropCueID
        {
            get
            {
                return GetPropertyValue("DropCueID", string.Empty);
            }
            set
            {
                SetPropertyValue("DropCueID", value);
            }
        }


        /// <summary>
        /// If true webpart zones are hightligted.
        /// </summary>
        [ExtenderControlProperty]
        public bool HighlightDropableAreas
        {
            get
            {
                return GetPropertyValue("HighlightDropableAreas", false);
            }
            set
            {
                SetPropertyValue("HighlightDropableAreas", value);
            }
        }


        /// <summary>
        /// Item group.
        /// </summary>
        [ExtenderControlProperty]
        public string ItemGroup
        {
            get
            {
                return GetPropertyValue("ItemGroup", string.Empty);
            }
            set
            {
                SetPropertyValue("ItemGroup", value);
            }
        }


        /// <summary>
        /// Method name called on item drop.
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("onDrop")]
        public string OnClientDrop
        {
            get
            {
                return GetPropertyValue("OnClientDrop", string.Empty);
            }
            set
            {
                SetPropertyValue("OnClientDrop", value);
            }
        }


        /// <summary>
        /// Method name called before item drop.
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("onBeforeDrop")]
        public string OnClientBeforeDrop
        {
            get
            {
                return GetPropertyValue("OnClientBeforeDrop", string.Empty);
            }
            set
            {
                SetPropertyValue("OnClientBeforeDrop", value);
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.PreRenderComplete += Page_PreRenderComplete;
        }


        /// <summary>
        /// PreRenderComplete event handler
        /// </summary>
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // Register the scripts
            ScriptHelper.RegisterScriptFile(Page, "DragAndDrop/FloatingBehavior.js");
            ScriptHelper.RegisterScriptFile(Page, "DragAndDrop/DragAndDropBehavior.js");
        }
    }
}
