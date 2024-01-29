using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Placeholder for messages.
    /// </summary>
    [ToolboxItem(false)]
    public class MessagesPlaceHolder : CMSPlaceHolder, INamingContainer
    {
        #region "Variables"

        private Label mInfoLabel = null;
        private Label mErrorLabel = null;
        private Label mWarningLabel = null;
        private Label mConfirmationLabel = null;
        private CMSUpdatePanel pnlInfo = null;

        private bool? mBasicStyles;

        private bool clientIdSet;
        private string mWrapperControlClientID = String.Empty;
        private string mWrapperControlID = String.Empty;
        private int? mMessageDescriptionLimit;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the control id of the wrapping control. Control offset and width is used to placeholder positioning
        /// </summary>
        public string WrapperControlID
        {
            get
            {
                return mWrapperControlID;
            }
            set
            {
                mWrapperControlID = value;
            }
        }


        /// <summary>
        /// Gets or sets the client control id of the wrapping control. Control offset and width is used to placeholder positioning
        /// </summary>
        public string WrapperControlClientID
        {
            get
            {
                if (!clientIdSet && !String.IsNullOrEmpty(WrapperControlID) && (Parent != null))
                {
                    Control ctrl = ControlsHelper.FindParentControl(WrapperControlID, this);
                    if (ctrl != null)
                    {
                        return ctrl.ClientID;
                    }
                }
                return mWrapperControlClientID;
            }
            set
            {
                mWrapperControlClientID = value;
                clientIdSet = true;
            }
        }

        /// <summary>
        /// Indicates that the placeholder should be used only for live site in context of control
        /// </summary>
        public bool LiveSiteOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Offset of Y coordinates
        /// </summary>
        public int OffsetY
        {
            get;
            set;
        }


        /// <summary>
        /// Opacity (0 - 100) of the messages panel (Applies only for dynamic messages. Default value is 87.)
        /// </summary>
        public int Opacity
        {
            get;
            set;
        }


        /// <summary>
        /// Offset of X coordinates
        /// </summary>
        public int OffsetX
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether relative placeholder should be used to move original content
        /// </summary>
        public bool UseRelativePlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if confirmation message should be displayed.
        /// </summary>
        public bool Confirmation
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout of the information or changes saved message
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout of the information message
        /// </summary>
        public int InfoTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout of the confirmation message
        /// </summary>
        public int ConfirmationTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout of the warning message
        /// </summary>
        public int WarningTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Timeout of the error message
        /// </summary>
        public int ErrorTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Information message text
        /// </summary>
        public string InfoText
        {
            get
            {
                return InfoLabel.Text;
            }
            set
            {
                InfoLabel.Text = value;
            }
        }


        /// <summary>
        /// Warning message text
        /// </summary>
        public string WarningText
        {
            get
            {
                return WarningLabel.Text;
            }
            set
            {
                WarningLabel.Text = value;
            }
        }


        /// <summary>
        /// Error message text
        /// </summary>
        public string ErrorText
        {
            get
            {
                return ErrorLabel.Text;
            }
            set
            {
                ErrorLabel.Text = value;
            }
        }


        /// <summary>
        /// Confirmation message text
        /// </summary>
        public string ConfirmationText
        {
            get
            {
                return ConfirmationLabel.Text;
            }
            set
            {
                ConfirmationLabel.Text = value;
            }
        }


        /// <summary>
        /// Information message basic CSS class
        /// </summary>
        public string InfoBasicCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Warning message basic CSS class
        /// </summary>
        public string WarningBasicCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Error message basic CSS class
        /// </summary>
        public string ErrorBasicCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Confirmation message basic CSS class
        /// </summary>
        public string ConfirmationBasicCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Information message description
        /// </summary>
        public string InfoDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Warning message description
        /// </summary>
        public string WarningDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Error message description
        /// </summary>
        public string ErrorDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Confirmation message description
        /// </summary>
        public string ConfirmationDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Information label tooltip
        /// </summary>
        public string InfoToolTip
        {
            get
            {
                return InfoLabel.ToolTip;
            }
            set
            {
                InfoLabel.ToolTip = value;
            }
        }


        /// <summary>
        /// Warning message tooltip
        /// </summary>
        public string WarningToolTip
        {
            get
            {
                return WarningLabel.ToolTip;
            }
            set
            {
                WarningLabel.ToolTip = value;
            }
        }


        /// <summary>
        /// Error message tooltip
        /// </summary>
        public string ErrorToolTip
        {
            get
            {
                return ErrorLabel.ToolTip;
            }
            set
            {
                ErrorLabel.ToolTip = value;
            }
        }


        /// <summary>
        /// Confirmation message tooltip
        /// </summary>
        public string ConfirmationToolTip
        {
            get
            {
                return ConfirmationLabel.ToolTip;
            }
            set
            {
                ConfirmationLabel.ToolTip = value;
            }
        }


        /// <summary>
        /// Information label.
        /// </summary>
        public Label InfoLabel
        {
            get
            {
                return mInfoLabel ?? (mInfoLabel = new AlertLabel { AlertType = MessageTypeEnum.Information, ID = "lI" });
            }
            set
            {
                mInfoLabel = value;
            }
        }


        /// <summary>
        /// Warning label.
        /// </summary>
        public Label WarningLabel
        {
            get
            {
                return mWarningLabel ?? (mWarningLabel = new AlertLabel { AlertType = MessageTypeEnum.Warning, ID = "lW" });
            }
            set
            {
                mWarningLabel = value;
            }
        }


        /// <summary>
        /// Error label.
        /// </summary>
        public Label ErrorLabel
        {
            get
            {
                return mErrorLabel ?? (mErrorLabel = new AlertLabel { AlertType = MessageTypeEnum.Error, ID = "lE" });
            }
            set
            {
                mErrorLabel = value;
            }
        }


        /// <summary>
        /// Confirmation label.
        /// </summary>
        public Label ConfirmationLabel
        {
            get
            {
                return mConfirmationLabel ?? (mConfirmationLabel = new AlertLabel { AlertType = MessageTypeEnum.Confirmation, ID = "lS" });
            }
            set
            {
                mConfirmationLabel = value;
            }
        }


        /// <summary>
        /// Indicates if basic styles should be used
        /// </summary>
        public bool BasicStyles
        {
            get
            {
                if (mBasicStyles == null)
                {
                    // Use basic styles for live site and dashboards
                    mBasicStyles = IsLiveSite || (PortalContext.ViewMode == ViewModeEnum.DashboardWidgets);
                }

                return mBasicStyles.Value;
            }
            set
            {
                mBasicStyles = value;
            }
        }


        /// <summary>
        /// CSS class for messages container
        /// </summary>
        public string ContainerCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Maximal length of message description.
        /// </summary>
        protected virtual int MessageDescriptionLimit
        {
            get
            {
                if (mMessageDescriptionLimit == null)
                {
                    mMessageDescriptionLimit = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSUIMessageDescriptionLimit"], 1000);
                }

                return mMessageDescriptionLimit.Value;
            }
        }


        /// <summary>
        /// Indicates whether any of the message labels contains text.
        /// </summary>
        public bool HasText
        {
            get
            {
                return !String.IsNullOrEmpty(ErrorText) || !String.IsNullOrEmpty(WarningText) || !String.IsNullOrEmpty(InfoText) || !String.IsNullOrEmpty(ConfirmationText);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public MessagesPlaceHolder()
        {
            EnableViewState = false;
            UseRelativePlaceHolder = true;
            Timeout = 2000;
            InfoTimeout = Timeout;
            ConfirmationTimeout = Timeout;
            Opacity = 100;

            // CSS classes
            InfoBasicCssClass = "InfoLabel";
            WarningBasicCssClass = InfoBasicCssClass;
            ConfirmationBasicCssClass = InfoBasicCssClass;
            ErrorBasicCssClass = "ErrorLabel";
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Encapsulate in update panel if script manager is registered
            Control container = this;
            if (!ControlsHelper.IsInUpdatePanel(this))
            {
                ScriptManager sManager = ScriptManager.GetCurrent(Page);
                if (sManager != null)
                {
                    // Update panel
                    pnlInfo = new CMSUpdatePanel { ID = "pMP", UpdateMode = UpdatePanelUpdateMode.Conditional };
                    container.Controls.Add(pnlInfo);
                    container = pnlInfo.ContentTemplateContainer;
                }
            }

            container.Controls.Add(InfoLabel);
            container.Controls.Add(WarningLabel);
            container.Controls.Add(ErrorLabel);
            container.Controls.Add(ConfirmationLabel);
        }


        /// <summary>
        /// Render event
        /// </summary>
        /// <param name="writer">HTML writer</param>        
        protected override void Render(HtmlTextWriter writer)
        {
            if (HandleVisibility())
            {
                // Initialize labels on Render to cover settings in PreRender
                InitLabels();

                // Render container only in the UI
                if (!BasicStyles)
                {
                    string containerClass = CssHelper.EnsureClass(ContainerCssClass, "cms-bootstrap");

                    writer.Write("<div{0}>", CssHelper.GetCssClassAttribute(containerClass));
                }

                base.Render(writer);

                if (!BasicStyles)
                {
                    writer.Write("</div>");
                }
            }
            else
            {
                base.Render(writer);
            }
        }


        /// <summary>
        /// Register required scripts
        /// </summary>
        protected void RegisterScripts()
        {
            if (!BasicStyles)
            {
                ScriptHelper.RegisterJQuery(Page);
                ScriptHelper.RegisterApplicationConstants(Page);
                ScriptHelper.RegisterScriptFile(Page, "Controls/MessagesPlaceHolder.js");

                if (RequestHelper.IsAsyncPostback())
                {
                    // Initialize labels on PreRender for partial post-back
                    InitLabels();
                }
            }

            // Update panel if used
            if ((pnlInfo != null) && RequestHelper.IsPostBack())
            {
                pnlInfo.Update();
            }
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Try register scripts
            if (HandleVisibility())
            {
                RegisterScripts();
            }
            // Try handle visibility after pre render of all controls
            else
            {
                Page.PreRenderComplete += Page_PreRenderComplete;
            }
        }


        /// <summary>
        /// Page.PreRender complete handler
        /// </summary>
        void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // Register scripts
            if (HandleVisibility())
            {
                RegisterScripts();
            }
        }


        private bool HandleVisibility()
        {
            // Handle visibility
            InfoLabel.Visible = !String.IsNullOrEmpty(InfoText);
            ErrorLabel.Visible = !String.IsNullOrEmpty(ErrorText);
            WarningLabel.Visible = !String.IsNullOrEmpty(WarningText);
            ConfirmationLabel.Visible = Confirmation;

            return (InfoLabel.Visible || ErrorLabel.Visible || WarningLabel.Visible || ConfirmationLabel.Visible);
        }


        /// <summary>
        /// Initializes labels and scripts
        /// </summary>
        private void InitLabels()
        {
            // Handle visibility
            if (HandleVisibility())
            {
                // Register scripts
                if (!BasicStyles)
                {
                    string script = String.Format(@"
var wrpCtrlid = {0};
var lblDetails = {1};
var lblLess = {2};
var posOffsetX = {3};
var posOffsetY = {4};
var lblsOpacity = {5};
var useRelPlc = {6};",
                        ScriptHelper.GetString(WrapperControlClientID),
                        ScriptHelper.GetString(ResHelper.GetString("advancedlabel.details")),
                        ScriptHelper.GetString(ResHelper.GetString("advancedlabel.detailsless")),
                        OffsetX,
                        OffsetY,
                        Opacity,
                        (UseRelativePlaceHolder ? "true" : "false"));

                    // Register general script
                    ScriptHelper.RegisterStartupScript(Page, typeof(string), "handlelabelinit", script, true);
                }

                // Initialize labels
                InitLabel(InfoLabel, InfoDescription, InfoTimeout, (InfoTimeout <= 0), InfoBasicCssClass);
                InitLabel(ErrorLabel, ErrorDescription, ErrorTimeout, (ErrorTimeout <= 0), ErrorBasicCssClass);
                InitLabel(WarningLabel, WarningDescription, WarningTimeout, (WarningTimeout <= 0), WarningBasicCssClass);
                InitLabel(ConfirmationLabel, ConfirmationDescription, ConfirmationTimeout, false, ConfirmationBasicCssClass);
            }
        }


        /// <summary>
        /// Initialize label properties
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="description">Description</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="allowClose">Indicates if the message can be closed</param>
        /// <param name="basicClass">Basic CSS class</param>
        private void InitLabel(Label label, string description, int timeout, bool allowClose, string basicClass)
        {
            if (label.Visible)
            {
                if (allowClose)
                {
                    label.CssClass += " alert-dismissable";
                }

                if (BasicStyles)
                {
                    AlertLabel alertLabel = label as AlertLabel;
                    if (alertLabel != null)
                    {
                        alertLabel.BasicStyles = true;
                    }

                    label.CssClass = basicClass;
                    if (!string.IsNullOrEmpty(description))
                    {
                        label.ToolTip = string.Join("&nbsp;", new[] { label.ToolTip, description });
                    }
                }
                else
                {
                    // Make the label invisible by default
                    label.Attributes.Add("style", "opacity: 0;position:fixed;");
                    label.Enabled = true;
                    // Register script for label
                    string script = string.Format(@"
$cmsj(document).ready(function() {{
    if (window.CMSHandleLabel) {{ 
        CMSHandleLabel('{0}', {1}, {2}, {3}, {4}); 
    }} 
}});",
                        label.ClientID,
                        ScriptHelper.GetString(description, true),
                        timeout,
                        allowClose ? "1" : "0",
                        GetLabelOptions());
                    ScriptHelper.RegisterStartupScript(Page, typeof(string), "handlelabel_" + label.ClientID, script, true);
                }
            }
        }


        /// <summary>
        /// Returns javascript object to setup placeholder. 
        /// </summary>
        private string GetLabelOptions()
        {
            if (BasicStyles)
            {
                return "null";
            }

            return String.Format("{{wrpCtrlid : {0}, lblDetails : {1}, lblLess : {2}, posOffsetX : {3}, posOffsetY : {4}, lblsOpacity : {5}, useRelPlc : {6}, }}",
                                 ScriptHelper.GetString(WrapperControlClientID),
                                 ScriptHelper.GetString(ResHelper.GetString("advancedlabel.details")),
                                 ScriptHelper.GetString(ResHelper.GetString("advancedlabel.detailsless")),
                                 OffsetX,
                                 OffsetY,
                                 Opacity,
                                 (UseRelativePlaceHolder ? "true" : "false"));
        }


        /// <summary>
        /// Gets script for anchor
        /// </summary>
        /// <param name="controlId">Control representing the anchor</param>
        /// <param name="fieldName">Name of the field</param>
        public string GetAnchorScript(string controlId, string fieldName)
        {
            return string.Format("CMSHndlLblAnchor('{0}', '{1}')", controlId, fieldName);
        }


        /// <summary>
        /// Adds text message.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public void AddMessage(MessageTypeEnum type, string text, string separator = null)
        {
            Label lbl = null;
            switch (type)
            {
                case MessageTypeEnum.Information:
                    lbl = InfoLabel;
                    break;

                case MessageTypeEnum.Confirmation:
                    Confirmation = true;
                    lbl = ConfirmationLabel;
                    break;

                case MessageTypeEnum.Warning:
                    lbl = WarningLabel;
                    break;

                case MessageTypeEnum.Error:
                    lbl = ErrorLabel;
                    break;
            }

            if (lbl != null)
            {
                AddMessage(lbl, text, separator);
            }
        }


        /// <summary>
        /// Adds text message to the label.
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        private void AddMessage(Label label, string text, string separator = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Default separator
            if (separator == null)
            {
                separator = "<br />";
            }

            if (string.IsNullOrEmpty(label.Text))
            {
                label.Text = text;
            }
            else
            {
                label.Text = string.Join(separator, new[] { label.Text, text });
            }
        }


        /// <summary>
        /// Shows the given message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="type">Type of the message</param>
        /// <param name="text">Information message</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public void ShowMessage(MessageTypeEnum type, string text, string description = null, string tooltipText = null, bool persistent = false)
        {
            if ((description != null) && (description.Length > MessageDescriptionLimit))
            {
                string eventType = GetEventLogType(type);
                string padString = string.Format("... {0}", ResHelper.GetString("general.seeeventlog"));
                LogMessageToEventLog(eventType, "UIMESSAGE", text, description);
                description = HTMLHelper.LimitLength(description, MessageDescriptionLimit, padString, true);
            }
            switch (type)
            {
                case MessageTypeEnum.Information:
                    if (persistent)
                    {
                        InfoTimeout = 0;
                    }
                    else
                    {
                        InfoTimeout = Timeout;
                    }
                    InfoText = text;
                    InfoDescription = description;
                    InfoToolTip = tooltipText;
                    break;

                case MessageTypeEnum.Confirmation:
                    if (persistent)
                    {
                        ConfirmationTimeout = 0;
                    }
                    else
                    {
                        ConfirmationTimeout = Timeout;
                    }
                    Confirmation = true;
                    if (string.IsNullOrEmpty(text))
                    {
                        text = ResHelper.GetString("General.ChangesSaved");
                    }
                    ConfirmationText = text;
                    ConfirmationDescription = description;
                    ConfirmationToolTip = tooltipText;
                    break;

                case MessageTypeEnum.Warning:
                    WarningTimeout = persistent ? 0 : Timeout;
                    WarningText = text;
                    WarningDescription = description;
                    WarningToolTip = tooltipText;
                    break;

                case MessageTypeEnum.Error:
                    if (persistent)
                    {
                        ErrorTimeout = 0;
                    }
                    else
                    {
                        ErrorTimeout = Timeout;
                    }
                    ErrorText = text;
                    ErrorDescription = description;
                    ErrorToolTip = tooltipText;
                    break;
            }
        }


        /// <summary>
        /// Shows the given information on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public void ShowInformation(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Information, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Shows the specified error message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Error message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        public void ShowError(string text, string description = null, string tooltipText = null)
        {
            ShowMessage(MessageTypeEnum.Error, text, description, tooltipText, true);
        }


        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Warning message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        public void ShowWarning(string text, string description = null, string tooltipText = null)
        {
            ShowMessage(MessageTypeEnum.Warning, text, description, tooltipText, true);
        }


        /// <summary>
        /// Shows the general changes saved message.
        /// </summary>
        public void ShowChangesSaved()
        {
            ShowConfirmation(null);
        }


        /// <summary>
        /// Shows the general confirmation message.
        /// </summary>
        /// <param name="text">Custom message</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public void ShowConfirmation(string text, bool persistent = false)
        {
            ShowMessage(MessageTypeEnum.Confirmation, text, null, null, persistent);
        }


        /// <summary>
        /// Adds information text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="separator">Separator</param>
        public void AddInformation(string text, string separator = null)
        {
            AddMessage(InfoLabel, text, separator);
        }


        /// <summary>
        /// Adds error text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="separator">Separator</param>
        public void AddError(string text, string separator = null)
        {
            AddMessage(ErrorLabel, text, separator);
        }


        /// <summary>
        /// Prepends error text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="separator">Separator</param>
        public void PrependError(string text, string separator = null)
        {
            string sep = string.IsNullOrEmpty(ErrorText) ? "" : separator;
            ErrorText = string.Join(sep, new[] { text, ErrorText });
        }


        /// <summary>
        /// Adds warning text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="separator">Separator</param>
        public void AddWarning(string text, string separator = null)
        {
            AddMessage(WarningLabel, text, separator);
        }


        /// <summary>
        /// Adds confirmation text to existing message on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Message</param>
        /// <param name="separator">Separator</param>
        public void AddConfirmation(string text, string separator = null)
        {
            Confirmation = true;
            AddMessage(ConfirmationLabel, text, separator);
        }


        /// <summary>
        /// Clears labels text
        /// </summary>
        public void ClearLabels()
        {
            InfoText = string.Empty;
            ErrorText = string.Empty;
            WarningText = string.Empty;
            ConfirmationText = string.Empty;
        }


        /// <summary>
        /// Logs message to event log.
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <param name="eventCode">Event code</param>
        /// <param name="text">Short text</param>
        /// <param name="description">Full description</param>
        protected virtual void LogMessageToEventLog(string type, string eventCode, string text, string description)
        {
            EventLogProvider.LogEvent(type, DocumentContext.CurrentTitle, eventCode, text + "\r\n\r\n " + description, RequestContext.RawURL, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName);
        }


        /// <summary>
        /// Returns type of event log record based on given message type.
        /// </summary>
        /// <param name="type">Message type</param>
        protected virtual string GetEventLogType(MessageTypeEnum type)
        {
            switch (type)
            {
                case MessageTypeEnum.Error:
                    return EventType.ERROR;
                case MessageTypeEnum.Warning:
                    return EventType.WARNING;
                default:
                    return EventType.INFORMATION;
            }
        }

        #endregion
    }
}
