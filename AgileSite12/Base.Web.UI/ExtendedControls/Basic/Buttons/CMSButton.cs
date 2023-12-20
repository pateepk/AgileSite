using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.Base.Web.UI
{
    #region "Enums"

    /// <summary>
    /// Set of button types.
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// Primary button, will have style btn-primary.
        /// </summary>
        Primary,

        /// <summary>
        /// Default button, will have style btn-default.
        /// </summary>
        Default,

        /// <summary>
        /// Button with specific styles except generic button styles.
        /// </summary>
        None
    }

    #endregion


    /// <summary>
    /// CMS Button with support for disabled items.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSButton : Button
    {
        #region "Constants"

        /// <summary>
        /// When this expression is used in Text property, it wil be replaced with icon markup.
        /// </summary>
        public const string ICON_PLACEMENT_MACRO = "##ICON##";

        #endregion


        #region "Variables"

        private bool? mIsLiveSite;
        private bool? mStopProcessing;
        private string mComponentName;
        private string mText;
        private bool? mRenderInputTag; 

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the style of the button.
        /// </summary>
        public virtual ButtonStyle ButtonStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class that serves as icon for the button.
        /// Icon is by default rendered before text. Position of the icon can be overriden by
        /// using expression <see cref="ICON_PLACEMENT_MACRO"/> inside the text property.
        /// </summary>
        public virtual string IconCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if java script methods for enabling/disabling button should be generated.
        /// </summary>
        public virtual bool RenderScript
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip text.
        /// </summary>
        public new virtual string ToolTip
        {
            get;
            set;
        }


        /// <summary>
        /// Text displayed in button control.
        /// </summary>
        public new virtual string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
                // Set base Text property. Value of this property will be rendred inside the value attribute of button tag. 
                base.Text = value == null ? null : value.Replace(ICON_PLACEMENT_MACRO, "");
            }
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        public virtual bool IsLiveSite
        {
            get
            {
                if (mIsLiveSite == null)
                {
                    // Try to get the property value from parent controls
                    mIsLiveSite = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.IsLiveSite, !(Page is IAdminPage));
                }

                return mIsLiveSite.Value;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Component name
        /// </summary>
        public virtual string ComponentName
        {
            get
            {
                if (mComponentName == null)
                {
                    mComponentName = "";

                    // Try to get the property value from parent controls
                    mComponentName = ControlsHelper.GetParentProperty<AbstractUserControl, string>(this, s => s.ComponentName, mComponentName);
                }

                return mComponentName;
            }
            set
            {
                mComponentName = value;
            }
        }


        /// <summary>
        /// Indicates if the control should perform the operations.
        /// </summary>
        public virtual bool StopProcessing
        {
            get
            {
                if (mStopProcessing == null)
                {
                    mStopProcessing = false;

                    // Try to get the property value from parent controls
                    mStopProcessing = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.StopProcessing, mStopProcessing.Value);
                }

                return mStopProcessing.Value;
            }
            set
            {
                mStopProcessing = value;
            }
        }


        /// <summary>
        /// Indicates whether button should disable its click handler upon first click.
        /// </summary>
        public virtual bool DisableAfterSubmit
        {
            get;
            set;
        }


        /// <summary>
        /// When true input tag is rendered instead of button tag.
        /// </summary>
        public virtual bool RenderInputTag
        {
            get
            {
                if (mRenderInputTag.HasValue)
                {
                    return mRenderInputTag.Value;                    
                }

                return IsLiveSite && ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSButtonRenderInputTag"], true);
            }
            set
            {
                mRenderInputTag = value;
            }
        }

        #endregion


        #region "Overridden properties"

        /// <summary>
        /// HTML tag for this control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return RenderInputTag ? HtmlTextWriterTag.Input : HtmlTextWriterTag.Button;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Render java script functions
            if (Visible)
            {
                if (RenderScript)
                {
                    ScriptHelper.RegisterJQuery(Page);

                    var script = string.Format(@"
function BTN_Enable(id)
{{
    var elem = $cmsj('#' + id);
    elem.removeAttr('disabled');
    elem.removeClass('{0} btn-disabled');
}}
function BTN_Disable(id)
{{
    var elem = $cmsj('#' + id);
    elem.attr('disabled', 'disabled');
    // First remove existing disabled classes to make sure that these classes are there only once (addClass does not replace, always appends the class)
    elem.removeClass('{0} btn-disabled').addClass('{0} btn-disabled');
}}
", DisabledCssClass);

                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "BTN_Scripts", ScriptHelper.GetScript(script));
                }

                if (DisableAfterSubmit)
                {
                    ScriptHelper.RegisterJQuery(Page);

                    // Disable click handler after first click.
                    StringBuilder clickScript = new StringBuilder();
                    clickScript.Append(@"
(function(){
    var button = $cmsj('#", ClientID, @"');
    button.bind('click', function() {
        button.bind('click', function() { return false; });
        button.attr('onclick', 'return false;');
        return true;
    });
})();");
                    ScriptHelper.RegisterStartupScript(this, typeof(string), "SubmitOnlyOnce_" + ClientID, ScriptHelper.GetScript(clickScript.ToString()));
                }
            }
        }


        /// <summary>
        /// Renders the button.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Add the generic button styles
            this.AddCssClass("btn");

            switch (ButtonStyle)
            {
                case ButtonStyle.Primary:
                    this.AddCssClass("btn-primary");
                    break;

                case ButtonStyle.Default:
                    this.AddCssClass("btn-default");
                    break;
            }
            
            if (!IsEnabled)
            {
                this.AddCssClass("btn-disabled");

                // If client click is defined, add it even if disabled
                if (!String.IsNullOrEmpty(OnClientClick) && RenderScript)
                {
                    Attributes.Add("onclick", OnClientClick);
                }
            }

            // Add tooltip
            if (ToolTip != null)
            {
                Attributes.Add("title", ToolTip);
            }

            base.Render(writer);
        }


        /// <summary>
        /// Renders content.
        /// </summary>
        /// <param name="writer">HTML text writer.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // Input tag doesn't use content. It sets value attribute instead. 
            if (!RenderInputTag)
            {
                var content = Text ?? "";
                var iconClass = IconCssClass;

                // When icon is missing use unknown icon
                if (content.Contains(ICON_PLACEMENT_MACRO))
                {
                    iconClass = iconClass ?? "icon-unknown";
                }
                    // When placement macro is missing place icon before text.
                else if (!string.IsNullOrEmpty(iconClass))
                {
                    content = ICON_PLACEMENT_MACRO + content;
                }

                // Encode the content before the icon HTML is replaced
                content = HTMLHelper.HTMLEncode(content);

                // Render icon markup
                if (!string.IsNullOrEmpty(iconClass))
                {
                    var icon = String.Format("<i aria-hidden=\"true\" class=\"{0}\" id=\"{1}\"></i>", iconClass, ClientID + "_icon");
                    content = content.Replace(ICON_PLACEMENT_MACRO, icon);
                }

                // For <button> tag it's necessary to render the text as the content
                writer.Write(content);
            }
        }

        #endregion
    }
}
