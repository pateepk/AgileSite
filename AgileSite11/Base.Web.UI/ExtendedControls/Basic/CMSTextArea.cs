using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Basic multiple lines text editor.
    /// </summary>
    [Description("Multiple lines text editor")]
    [ToolboxData(@"<{0}:CMSTextArea ID=""CMSTextArea1"" runat=""server""></{0}:CMSTextArea>")]
    [DefaultProperty("Text"), ControlValueProperty("Text"), ValidationProperty("Text"), DefaultEvent("TextChanged"), SupportsEventValidation]
    public class CMSTextArea : CMSTextBox
    {
        #region "Variables"

        private int mScrollPosition = 0;
        private bool mMaintainScrollPositionOnPostback = true;

        #endregion


        #region "Designer properties"

        /// <summary>
        /// Gets or sets a value indicating whether the text content wraps.
        /// </summary>
        /// <value>True, if text wrapping is allowed, otherwise false. Default is false.</value>
        [Browsable(true)]
        [Description("Determines if the text content wraps")]
        [Category("Layout")]
        [DefaultValue(false)]
        public override bool Wrap
        {
            get
            {
                return base.Wrap;
            }
            set
            {
                base.Wrap = value;
            }
        }


        /// <summary>
        /// Gets or sets whether the scroll position is maintained between postbacks.
        /// </summary>
        /// <value>True if the scroll position is maintained, otherwise false. Default is true.</value>
        [Browsable(true)]
        [Description("Determines if the scroll position is maintained between postbacks")]
        [Category("Basic")]
        [DefaultValue(true)]
        public bool MaintainScrollPositionOnPostback
        {
            get
            {
                return mMaintainScrollPositionOnPostback;
            }
            set
            {
                mMaintainScrollPositionOnPostback = value;
            }
        }


        /// <summary>
        /// Gets or sets whether the focus is enabled.
        /// </summary>
        [Browsable(true)]
        [Description("Determines whether the focus is enabled")]
        [Category("Basic")]
        [DefaultValue(false)]
        public bool EnableFocus
        {
            get;
            set;
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the name of the hidden field that carries a scroll position.
        /// </summary>
        [Browsable(false)]
        public string PositionMarker
        {
            get
            {
                return string.Format(@"{0}_POSITION", ClientID);
            }
        }


        /// <summary>
        /// Gets the scroll position to use on postback.
        /// </summary>
        [Browsable(false)]
        protected int ScrollPosition
        {
            get
            {
                return mMaintainScrollPositionOnPostback ? mScrollPosition : 0;
            }
            set
            {
                mScrollPosition = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Initializes a new instance of CMSTextArea class.
        /// </summary>
        public CMSTextArea()
        {
            // Set default values for overridden properties            
            base.TextMode = TextBoxMode.MultiLine;
        }

        #endregion


        #region "Control methods"

        /// <summary>
        /// Retrieves scroll position markers from hidden fields.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        protected override void OnLoad(EventArgs e)
        {
            if (RequestHelper.IsPostBack() && MaintainScrollPositionOnPostback)
            {
                mScrollPosition = ValidationHelper.GetInteger(Page.Request.Form[PositionMarker], 0);
            }
        }


        /// <summary>
        /// Renders scripts and elements required for correct functionality.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterPosition();
            RegisterScrollScripts();
        }


        /// <summary>
        /// Registers the position marker.
        /// </summary>
        protected virtual void RegisterPosition()
        {
            if ((Context != null) && MaintainScrollPositionOnPostback)
            {
                ScriptHelper.RegisterHiddenField(this, PositionMarker, GetScrollPositionValue());
            }
        }


        /// <summary>
        /// Gets the scroll position value
        /// </summary>
        protected virtual string GetScrollPositionValue()
        {
            return ScrollPosition.ToString();
        }


        /// <summary>
        /// Register required scroll scripts
        /// </summary>
        private void RegisterScrollScripts()
        {
            // Position remember
            if (MaintainScrollPositionOnPostback && Enabled && Visible)
            {
                // Save position scripts
                string scripts = string.Format(
                    @"
function Get(id) {{
    return document.getElementById(id);
}}

function ETA_SavePosition(id) {{                                   
    var ed = Get(id);
    if(ed) {{
        var sp = ed.scrollTop; 
        var pos = Get(id + '_POSITION');
        if(pos) {{
            pos.value = sp; 
        }}
    }}
}}

function ETA_GetPosition(id)
{{
    var hid = Get(id + '_POSITION');
    var ed = Get(id);
    try {{ 
            {0}
            ed.scrollTop = hid.value; 
    }} catch (ex) {{}} 
}}", (EnableFocus) ? "ed.focus();ed.blur();" : "");

                // Register scripts
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ETA_Positioning", ScriptHelper.GetScript(scripts));
                ScriptManager.RegisterStartupScript(this, typeof(string), "ETA_GetPosition_" + ClientID, String.Format("ETA_GetPosition('{0}');", ClientID), true);
                ScriptManager.RegisterOnSubmitStatement(this, typeof(string), "ETA_SavePosition_" + ClientID, String.Format("ETA_SavePosition('{0}');", ClientID));
            }
        }

        #endregion
    }
}
