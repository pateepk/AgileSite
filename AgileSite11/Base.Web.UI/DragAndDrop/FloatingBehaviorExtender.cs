using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using AjaxControlToolkit;

namespace CMS.Base.Web.UI.DragAndDrop
{
    /// <summary>
    /// Custom floating behavior definition.
    /// </summary>
    [Designer(typeof(FloatingBehaviorDesigner))]
    [TargetControlType(typeof(WebControl))]
    [ClientScriptResource("CMSExtendedControls.DragAndDropBehavior", "CMS.Base.Web.UI.DragAndDrop.Empty")]
    [RequiredScript(typeof(DragDropScripts))]
    internal class FloatingBehaviorExtender : ExtenderControlBase
    {
        /// <summary>
        /// Handle ID.
        /// </summary>
        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        public string DragHandleID
        {
            get
            {
                return GetPropertyValue<String>("DragHandleID", string.Empty);
            }
            set
            {
                SetPropertyValue<String>("DragHandleID", value);
            }
        }
    }
}