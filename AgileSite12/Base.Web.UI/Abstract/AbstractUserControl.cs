using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base.Web.UI.ActionsConfig;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// User control base.
    /// </summary>
    public abstract class AbstractUserControl : UserControl, IEnsureControls, IShortID, IResourcePrefixManager
    {
        #region "Variables"

        private bool mStopProcessing;
        private bool? mIsLiveSite;
        private string mComponentName;
        private ICMSDocumentManager mDocumentManager;
        private MessagesPlaceHolder mMessagesPlaceHolder;
        private HeaderActions mHeaderActions;
        private bool mUsesLocalMessagesPlaceHolder = true;
        private ICollection<String> mResourcePrefixes;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if the control should perform the operations.
        /// </summary>
        public virtual bool StopProcessing
        {
            get
            {
                return mStopProcessing;
            }
            set
            {
                mStopProcessing = value;
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
                    IAdminPage page = Page as IAdminPage;
                    mIsLiveSite = (page == null);

                    // Try to get the property value from parent controls
                    mIsLiveSite = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.IsLiveSite, mIsLiveSite.Value);
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
        /// Short ID of the control.
        /// </summary>
        public string ShortID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control uses local messages placeholder
        /// </summary>
        public bool UsesLocalMessagesPlaceHolder
        {
            get
            {
                if (MessagesPlaceHolder != null)
                {
                    return mUsesLocalMessagesPlaceHolder;
                }

                return false;
            }
        }


        /// <summary>
        /// Placeholder for messages
        /// </summary>
        public virtual MessagesPlaceHolder MessagesPlaceHolder
        {
            get
            {
                if (mMessagesPlaceHolder == null)
                {
                    // Try to get placeholder from siblings
                    if (Parent != null)
                    {
                        ControlCollection siblings = Parent.Controls;
                        foreach (var sib in siblings)
                        {
                            MessagesPlaceHolder mess = sib as MessagesPlaceHolder;
                            if (mess != null)
                            {
                                mMessagesPlaceHolder = mess;
                                mUsesLocalMessagesPlaceHolder = false;
                                return mMessagesPlaceHolder;
                            }
                        }
                    }

                    // Get page placeholder as default one
                    ICMSPage page = Page as ICMSPage;
                    if (page != null)
                    {
                        mMessagesPlaceHolder = page.MessagesPlaceHolder;
                    }

                    // Try to get placeholder from parent controls
                    mMessagesPlaceHolder = ControlsHelper.GetParentProperty<AbstractUserControl, MessagesPlaceHolder>(this, s => s.MessagesPlaceHolder, mMessagesPlaceHolder);
                    mUsesLocalMessagesPlaceHolder = false;
                }

                return mMessagesPlaceHolder;
            }
        }


        /// <summary>
        /// Header actions control
        /// </summary>
        public virtual HeaderActions HeaderActions
        {
            get
            {
                if (mHeaderActions == null)
                {
                    // Get page
                    ICMSPage page = Page as ICMSPage;
                    mHeaderActions = (page != null) ? page.HeaderActions : null;

                    // Try to get header actions from parent controls
                    mHeaderActions = ControlsHelper.GetParentProperty<AbstractUserControl, HeaderActions>(this, s => s.HeaderActions, mHeaderActions);
                }

                return mHeaderActions;
            }
            set
            {
                ICMSPage page = Page as ICMSPage;
                if (page != null)
                {
                    page.HeaderActions = value;
                }

                mHeaderActions = value;
            }
        }


        /// <summary>
        /// Document manager control
        /// </summary>
        public virtual ICMSDocumentManager DocumentManager
        {
            get
            {
                if (mDocumentManager == null)
                {
                    // Get page
                    ICMSPage page = Page as ICMSPage;
                    mDocumentManager = (page != null) ? page.DocumentManager : null;

                    // Try to get manager from parent controls
                    mDocumentManager = ControlsHelper.GetParentProperty<AbstractUserControl, ICMSDocumentManager>(this, s => s.DocumentManager, mDocumentManager);

                    if (mDocumentManager == null)
                    {
                        throw new Exception("[AbstractUserControl.DocumentManager]: Page does not implement the 'ICMSPage' interface or missing document manager.");
                    }
                }

                return mDocumentManager;
            }
        }


        /// <summary>
        /// Prefix for the resource strings which are used for the localization by the control and its child controls. 
        /// </summary>
        /// <remarks>If custom translation is not defined, the default resource string translation is used. Localization is called through <see cref="GetString"/> method.</remarks>
        /// <example>The following code is internally represented as "customprefix.ok|general.ok" resource string key if the property is set to the "customprefix" value.
        /// <code>
        /// ResourcePrefix = "customprefix";
        /// var localizedString = GetString("general.ok");
        /// </code>
        /// </example>
        /// <seealso cref="GetString"/>
        public virtual string ResourcePrefix
        {
            get;
            set;
        }


        /// <summary>
        /// List of cached resource prefixes for the parent hierarchy
        /// </summary>
        public ICollection<string> ResourcePrefixes
        {
            get
            {
                return mResourcePrefixes ?? (mResourcePrefixes = ControlsLocalization.GetPrefixFromControlHierarchy(this));
            }
        }

        #endregion


        #region "Context properties"

        /// <summary>
        /// Current page info
        /// </summary>
        public virtual PageInfo CurrentPageInfo
        {
            get
            {
                return DocumentContext.CurrentPageInfo;
            }
        }


        /// <summary>
        /// Current document
        /// </summary>
        public virtual TreeNode CurrentDocument
        {
            get
            {
                return DocumentContext.CurrentDocument;
            }
        }


        /// <summary>
        /// Current site
        /// </summary>
        public virtual SiteInfo CurrentSite
        {
            get
            {
                return SiteContext.CurrentSite;
            }
        }


        /// <summary>
        /// Current user
        /// </summary>
        public virtual CurrentUserInfo CurrentUser
        {
            get
            {
                return MembershipContext.AuthenticatedUser;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Fires before UserControl Init.
        /// </summary>
        public static event BeforeEventHandler OnBeforeUserControlInit;


        /// <summary>
        /// Fires after UserControl Init.
        /// </summary>
        public static event EventHandler OnAfterUserControlInit;


        /// <summary>
        /// Fires before UserControl Load.
        /// </summary>
        public static event BeforeEventHandler OnBeforeUserControlLoad;


        /// <summary>
        /// Fires after UserControl Load.
        /// </summary>
        public static event EventHandler OnAfterUserControlLoad;


        /// <summary>
        /// Fires before UserControl PreRender.
        /// </summary>
        public static event BeforeEventHandler OnBeforeUserControlPreRender;


        /// <summary>
        /// Fires after UserControl PreRender.
        /// </summary>
        public static event EventHandler OnAfterUserControlPreRender;


        /// <summary>
        /// Fires before UserControl Render.
        /// </summary>
        public static event BeforeRenderEventHandler OnBeforeUserControlRender;


        /// <summary>
        /// Fires after UserControl Render.
        /// </summary>
        public static event RenderEventHandler OnAfterUserControlRender;

        #endregion


        #region "Image URLs methods"

        /// <summary>
        /// Resolves the given URL
        /// </summary>
        /// <param name="url">URL to resolve</param>
        public new string ResolveUrl(string url)
        {
            return UrlResolver.ResolveUrl(url);
        }


        /// <summary>
        /// Gets UI image relative path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        public string GetImagePath(string imagePath)
        {
            return UIHelper.GetImagePath(Page, imagePath);
        }


        /// <summary>
        /// Gets UI image relative path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/Membership/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be ensured</param>
        public string GetImageUrl(string imagePath, bool isLiveSite = false, bool ensureDefaultTheme = false)
        {
            return UIHelper.GetImageUrl(Page, imagePath, isLiveSite, ensureDefaultTheme);
        }


        /// <summary>
        /// Returns resolved path to the flag image for the specified culture.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="iconSet">Name of the subfolder where icon images are located</param>
        public virtual string GetFlagIconUrl(string cultureCode, string iconSet)
        {
            return UIHelper.GetFlagIconUrl(Page, cultureCode, iconSet);
        }

        #endregion


        #region "UserControl cycle events"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            // Before event
            if ((OnBeforeUserControlInit == null) || OnBeforeUserControlInit(this, e))
            {
                base.OnInit(e);
            }

            // After event
            if (OnAfterUserControlInit != null)
            {
                OnAfterUserControlInit(this, e);
            }
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Before event
            if ((OnBeforeUserControlLoad == null) || OnBeforeUserControlLoad(this, e))
            {
                base.OnLoad(e);

                CMSHttpContext.Current.Response.Cache.SetNoStore();
            }

            // After event
            if (OnAfterUserControlLoad != null)
            {
                OnAfterUserControlLoad(this, e);
            }
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Before event
            if ((OnBeforeUserControlPreRender == null) || OnBeforeUserControlPreRender(this, e))
            {
                base.OnPreRender(e);
            }

            // After event
            if (OnAfterUserControlPreRender != null)
            {
                OnAfterUserControlPreRender(this, e);
            }
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                return;
            }

            string path = null;
            if (SystemContext.DevelopmentMode)
            {
                path = AppRelativeVirtualPath;
                if (path != null)
                {
                    path = path.Replace("-", "=");

                    writer.Write("<!-- {0} -->", path);
                }
            }

            // Before event
            if ((OnBeforeUserControlRender == null) || OnBeforeUserControlRender(this, writer))
            {
                base.Render(writer);
            }

            // After event
            if (OnAfterUserControlRender != null)
            {
                OnAfterUserControlRender(this, writer);
            }

            if (path != null)
            {
                writer.Write("<!-- END {0} -->", path);
            }
        }


        /// <summary>
        /// Loads the user control based on the given path
        /// </summary>
        /// <param name="controlPath">Control path</param>
        public Control LoadUserControl(string controlPath)
        {
            VirtualPathLog.Log(controlPath);
            return LoadControl(controlPath);
        }


        /// <summary>
        /// Interface for control that is able to explicitly ensure its child controls
        /// </summary>
        public void EnsureControls()
        {
            EnsureChildControls();
        }

        #endregion


        #region "Eval methods"

        /// <summary>
        /// Evaluates the given value
        /// </summary>
        /// <param name="column">Column name to evaluate</param>
        public virtual new object Eval(string column)
        {
            return base.Eval(column);
        }


        /// <summary>
        /// Templated Eval, returns the value converted to specific type.
        /// </summary>
        /// <typeparam name="ReturnType">Result type</typeparam>
        /// <param name="columnName">Column name</param>
        public virtual ReturnType Eval<ReturnType>(string columnName)
        {
            return ValidationHelper.GetValue<ReturnType>(Eval(columnName));
        }


        /// <summary>
        /// Evaluates the item data (safe version), with html encoding.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="encode">If true the text is encoded</param>
        public virtual object Eval(string columnName, bool encode)
        {
            object value = Eval(columnName);
            if (encode)
            {
                value = HTMLHelper.HTMLEncode(ValidationHelper.GetString(value, string.Empty));
            }

            return value;
        }


        /// <summary>
        /// Evaluates the item data and doesn't encode it. Method should be used for columns with html content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual string EvalHTML(string columnName)
        {
            return ValidationHelper.GetString(Eval(columnName, false), string.Empty);
        }


        /// <summary>
        /// Evaluates the item data, encodes it to be used in HTML attribute.
        /// </summary>
        /// <param name="column">Column name</param>
        public virtual string EvalHtmlAttribute(string column)
        {
            object value = Eval(column);
            return HTMLHelper.EncodeForHtmlAttribute(ValidationHelper.GetString(value, string.Empty));
        }


        /// <summary>
        /// Evaluates the item data, encodes it to be used in javascript code and encapsulates it with "'".
        /// </summary>
        /// <param name="columnName">Column name</param>        
        public virtual string EvalJSString(string columnName)
        {
            object value = Eval(columnName);
            return ScriptHelper.GetString(ValidationHelper.GetString(value, string.Empty));
        }


        /// <summary>
        /// Evaluates the item data and encodes it. Method should be used for columns with string nonhtml content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual string EvalText(string columnName)
        {
            return EvalText(columnName, false);
        }


        /// <summary>
        /// Evaluates the item data and encodes it. Method should be used for columns with string nonhtml content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="localize">Indicates if text should be localized</param>
        public virtual string EvalText(string columnName, bool localize)
        {
            // Encode text
            string value = ValidationHelper.GetString(Eval(columnName, true), string.Empty);

            if (localize)
            {
                // Localize text
                return ResHelper.LocalizeString(value);
            }

            return value;
        }


        /// <summary>
        /// Evaluates the item data and converts it to the integer.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual int EvalInteger(string columnName)
        {
            return Eval<int>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the double.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual double EvalDouble(string columnName)
        {
            return Eval<double>(columnName);
        }
        
        
        /// <summary>
        /// Evaluates the item data and converts it to the decimal.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual decimal EvalDecimal(string columnName)
        {
            return Eval<decimal>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the date time.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual DateTime EvalDateTime(string columnName)
        {
            return Eval<DateTime>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the guid.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual Guid EvalGuid(string columnName)
        {
            return Eval<Guid>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the bool.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual bool EvalBool(string columnName)
        {
            return Eval<bool>(columnName);
        }

        #endregion


        #region "Methods for info messages"

        /// <summary>
        /// Returns the localized string of the control's hierarchically highest parent that has the <see cref="ResourcePrefix"/> property and its translation defined. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// If no parent but the control has the property defined, returns the control's localized string.
        /// </para>
        /// <para>
        /// If none of them have the <see cref="ResourcePrefix"/> property defined, returns the default resource string translation.
        /// </para>
        /// </remarks>
        /// <param name="stringName">String to localize.</param>
        /// <param name="culture">Culture to be used for localization.</param>
        /// <seealso cref="ResourcePrefix"/>
        public virtual string GetString(string stringName, string culture = null)
        {
            return ControlsLocalization.GetString(this, stringName, culture);
        }


        /// <summary>
        /// Shows the general changes saved message.
        /// </summary>
        public virtual void ShowChangesSaved()
        {
            ShowConfirmation(null);
        }


        /// <summary>
        /// Shows the general confirmation message.
        /// </summary>
        /// <param name="text">Custom message</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowConfirmation(string text, bool persistent = false)
        {
            ShowMessage(MessageTypeEnum.Confirmation, text, null, null, persistent);
        }


        /// <summary>
        /// Shows the given information on the page, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowInformation(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Information, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Logs the exception and 
        /// </summary>
        /// <param name="source">Error source for the event log</param>
        /// <param name="eventCode">Event code for the event log</param>
        /// <param name="ex">Exception to log</param>
        public void LogAndShowError(string source, string eventCode, Exception ex)
        {
            ShowError(String.Format(ResHelper.GetString("General.ErrorOccuredLog"), source, eventCode, ex.Message));

            EventLogProvider.LogException(source, eventCode, ex);
        }


        /// <summary>
        /// Shows the specified error message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Error message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowError(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Error, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Shows the specified warning message, optionally with a tooltip text.
        /// </summary>
        /// <param name="text">Warning message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowWarning(string text, string description = null, string tooltipText = null, bool persistent = true)
        {
            ShowMessage(MessageTypeEnum.Warning, text, description, tooltipText, persistent);
        }


        /// <summary>
        /// Shows the specified message, optionally with a tooltip text.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        public virtual void ShowMessage(MessageTypeEnum type, string text, string description, string tooltipText, bool persistent)
        {
            // Use basic styles when used on live site
            ShowMessage(type, text, description, tooltipText, persistent, true);
        }


        /// <summary>
        /// Shows the specified message, optionally with a tooltip text.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="description">Additional description</param>
        /// <param name="tooltipText">Tooltip text</param>
        /// <param name="persistent">Indicates if the message is persistent</param>
        /// <param name="useBasicStylesOnLiveSite">Indicates whether the message should use the basic styles when used on live site</param>
        protected virtual void ShowMessage(MessageTypeEnum type, string text, string description, string tooltipText, bool persistent, bool useBasicStylesOnLiveSite)
        {
            bool displayed = false;
            ICMSPage page = Page as ICMSPage;

            // Keep messages placeholder object
            MessagesPlaceHolder plc = MessagesPlaceHolder;

            // Use control placeholder if basic styles should be used or on live site and placeholder is for live site or page doesn't have placeholder
            if ((plc != null) && (page != null) && (plc.BasicStyles || IsLiveSite || !plc.LiveSiteOnly || (page.MessagesPlaceHolder == null)))
            {
                if (IsLiveSite && useBasicStylesOnLiveSite)
                {
                    plc.BasicStyles = true;
                }

                plc.ShowMessage(type, text, description, tooltipText, persistent);
                displayed = true;
            }
            // Use placeholder from master page
            else if (page != null)
            {
                page.ShowMessage(type, text, description, tooltipText, persistent);
                displayed = true;
            }

            if (!displayed)
            {
                throw new Exception("[AbstractUserControl.ShowMessage]: Page is not 'CMSPage' or missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds message text to existing message on the page.
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="text">Message text</param>
        /// <param name="separator">Separator</param>
        public virtual void AddMessage(MessageTypeEnum type, string text, string separator = null)
        {
            bool added = false;
            ICMSPage page = Page as ICMSPage;

            // Use control placeholder if basic styles should be used or on live site and placeholder is for live site or page doesn't have placeholder
            if ((MessagesPlaceHolder != null) && (MessagesPlaceHolder.BasicStyles || IsLiveSite || !MessagesPlaceHolder.LiveSiteOnly || (page.MessagesPlaceHolder == null)))
            {
                if (IsLiveSite)
                {
                    MessagesPlaceHolder.BasicStyles = true;
                }

                MessagesPlaceHolder.AddMessage(type, text, separator);
                added = true;
            }
            // Use placeholder from master page
            else if (page != null)
            {
                page.AddMessage(type, text, separator);
                added = true;
            }

            if (!added)
            {
                throw new Exception("[AbstractUserControl.AddMessage]: Page is not 'CMSPage' or missing messages placeholder.");
            }
        }


        /// <summary>
        /// Adds information text to existing message on the page.
        /// </summary>
        /// <param name="text">Information message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddInformation(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Information, text, separator);
        }


        /// <summary>
        /// Adds error text to existing message on the page.
        /// </summary>
        /// <param name="text">Error message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddError(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Error, text, separator);
        }


        /// <summary>
        /// Adds warning text to existing message on the page.
        /// </summary>
        /// <param name="text">Warning message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddWarning(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Warning, text, separator);
        }


        /// <summary>
        /// Adds confirmation text to existing message on the page.
        /// </summary>
        /// <param name="text">Confirmation message</param>
        /// <param name="separator">Separator</param>
        public virtual void AddConfirmation(string text, string separator = null)
        {
            AddMessage(MessageTypeEnum.Confirmation, text, separator);
        }

        #endregion


        #region "Header actions methods"

        /// <summary>
        /// Adds specified action to the page header actions.
        /// </summary>
        /// <param name="action">Header action</param>
        public virtual void AddHeaderAction(HeaderAction action)
        {
            if (HeaderActions != null)
            {
                HeaderActions.AddAction(action);
            }
            else
            {
                throw new Exception("[AbstractUserControl.AddHeaderAction]: The control's page does not contain HeaderActions control where the action can be added.");
            }
        }

        #endregion
    }
}
