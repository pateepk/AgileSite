using System;
using System.Web;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Copy to clipboard button control with localized text string.
    /// </summary>
    [ToolboxData(@"<{0}:LocalizedCopyToClipboardButton runat=""server"" />"), Serializable()]
    public class LocalizedCopyToClipboardButton : LocalizedButton
    {
        /// <summary>
        /// Gets or sets ID of control which represents source to copy the content from.
        /// </summary>
        public string CopySourceControlID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets resource string for tool tip of button, which is used when 'copy' command is not supported by client's browser.
        /// </summary>
        public string ToolTipNotSupportedResourceString
        {
            get;
            set;
        }

        
        /// <summary>
        /// Gets or sets resource string for alert message, when copying to clipboard fails on client.
        /// </summary>
        public string CopyFailedAlertMessageResourceString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the client-side script that executes when a button's click event is raised.
        /// If not initialized, null or empty string is set, gets <c>return false;</c> to prevent page submit.
        /// When custom client-side script is set, it must return <c>false</c> if page submit is not desired.
        /// </summary>
        public override string OnClientClick
        {
            get
            {
                return String.IsNullOrEmpty(base.OnClientClick) ? "return false;" : base.OnClientClick;
            }
            set
            {
                base.OnClientClick = value;
            }
        }


        /// <summary>
        /// Creates instance of Localized Copy to clipboard button with default text and messages.
        /// </summary>
        public LocalizedCopyToClipboardButton()
        {
            ResourceString = "copytoclipboard.button";
            CopyFailedAlertMessageResourceString = "copytoclipboard.failed";
            ToolTipNotSupportedResourceString = "copytoclipboard.notsupported";
        }


        /// <summary>
        /// Generates client script for the button.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible)
            {
                var copySourceControl = FindControl(CopySourceControlID);

                if ((copySourceControl == null) && !DesignMode)
                {
                    throw new HttpException($"Unable to find control with id '{CopySourceControlID}' that is a copy source for {nameof(LocalizedCopyToClipboardButton)} '{ID}'.");
                }

                ScriptHelper.RegisterJQuery(Page);

                var script = $@"
                    $cmsj(document).ready(function () {{
                        if (!document.queryCommandSupported('copy')) {{
                            var copyButton = $cmsj('#{ClientID}');
                            copyButton.prop('disabled', true);
                            copyButton.removeClass('btn-primary btn-default').addClass('btn-disabled');
                            copyButton.prop('title', '{ControlsLocalization.GetString(this, ToolTipNotSupportedResourceString)}');
                        }}
                        else {{
                            $cmsj('#{ClientID}').click(function(event) {{
                                $cmsj('#{copySourceControl?.ClientID}').select();
                                
                                try {{
                                    document.execCommand('copy');
                                }} catch (err) {{
                                    alert('{ControlsLocalization.GetString(this, CopyFailedAlertMessageResourceString)}');
                                }}
                            }});
                        }}
                    }});
                ";
                
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), $"CopyToClipboardButton_{ClientID}", ScriptHelper.GetScript(script));
            }
        }
    }
}