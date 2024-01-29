using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.Synchronization;


namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Object manager control.
    /// </summary>
    public class CMSObjectManager : CMSAbstractManager<SimpleObjectManagerEventArgs, SimpleObjectManagerEventArgs>, ICMSObjectManager, IPostBackEventHandler
    {
        #region "Variables"

        /// <summary>
        /// Object version manager instance.
        /// </summary>
        protected ObjectVersionManager mVersionManager = null;

        /// <summary>
        /// Object with which the manager is working.
        /// </summary>
        protected BaseInfo mInfoObject = null;

        /// <summary>
        /// Hidden field for comment argument.
        /// </summary>
        protected HiddenField hdnComment = new HiddenField { ID = "hdnComment", EnableViewState = false };

        private bool? mRenderScript;

        /// <summary>
        /// Button for launching partial postback
        /// </summary>
        protected Button btnCommand = new Button { ID = "btnCommand", EnableViewState = false, CssClass = "HiddenButton" };

        #endregion


        #region "Static properties"

        /// <summary>
        /// Returns true if CMSUseObjectCheckinCheckout and CMSKeepNewCheckedOut are true.
        /// </summary>
        public static bool KeepNewObjectsCheckedOut
        {
            get
            {
                // Automatically checkout the object if required
                return SynchronizationHelper.UseCheckinCheckout && SettingsKeyInfoProvider.GetBoolValue("CMSKeepNewCheckedOut");
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Hidden comment field.
        /// </summary>
        protected HiddenField HiddenComment
        {
            get
            {
                EnsureChildControls();
                return hdnComment;
            }
        }


        /// <summary>
        /// Action comment (for CheckIn operation).
        /// </summary>
        public string ActionComment
        {
            get
            {
                return HTMLHelper.HTMLEncode(HiddenComment.Value);
            }
        }


        /// <summary>
        /// Indicates if manager should register for common events (save etc.).
        /// </summary>
        public bool RegisterEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if java script functions for object management should be rendered.
        /// </summary>
        public bool RenderScript
        {
            get
            {
                if (mRenderScript == null)
                {
                    mRenderScript = (InfoObject != null);
                }
                return mRenderScript.Value;
            }
            set
            {
                mRenderScript = value;
            }
        }


        /// <summary>
        /// Object version manager instance.
        /// </summary>
        public virtual ObjectVersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = new ObjectVersionManager());
            }
        }


        /// <summary>
        /// Indicates if the object locking panel should be showed.
        /// </summary>
        public bool ShowPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Returns object with which the manager is working. It return the object based on ObjectType nd ObjectID properties, if these properties are not present, checks if the EditedObject is present.
        /// </summary>
        public BaseInfo InfoObject
        {
            get
            {
                if (!string.IsNullOrEmpty(ObjectType) && (mInfoObject == null))
                {
                    mInfoObject = ModuleManager.GetObject(ObjectType);
                    if (mInfoObject != null)
                    {
                        if (ObjectID > 0)
                        {
                            mInfoObject = mInfoObject.Generalized.GetObject(ObjectID);
                        }
                    }
                }
                if (mInfoObject != null)
                {
                    return mInfoObject;
                }

                if (UIContext.EditedObject != null)
                {
                    return UIContext.EditedObject as BaseInfo;
                }

                return null;
            }
        }


        /// <summary>
        /// Object type of the object. Taken into account only if EditedObject is null.
        /// </summary>
        public string ObjectType
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ObjectType"], null);
            }
            set
            {
                ViewState["ObjectType"] = value;
                mInfoObject = null;
            }
        }


        /// <summary>
        /// ID of the object. Taken into account only if EditedObject is null.
        /// </summary>
        public int ObjectID
        {
            get
            {
                return ValidationHelper.GetInteger(ViewState["ObjectID"], 0);
            }
            set
            {
                ViewState["ObjectID"] = value;
                mInfoObject = null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the post back event.
        /// </summary>
        /// <param name="eventArgument">Event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            var eventArgs = new SimpleObjectManagerEventArgs(InfoObject, Mode, eventArgument);

            // Check consistency
            if (!RaiseCheckConsistency(eventArgs))
            {
                return;
            }

            try
            {
                RaiseBeforeAction(eventArgs);

                if (eventArgs.IsValid)
                {
                    if (IsActionAllowed(eventArgument))
                    {
                        switch (eventArgument)
                        {
                            case ComponentEvents.SAVE:
                                SaveObject(eventArgs);
                                break;

                            case DocumentComponentEvents.CHECKIN:
                                CheckInAndSaveObject(ActionComment);
                                break;

                            case DocumentComponentEvents.CHECKOUT:
                                CheckOutObject();
                                break;

                            case DocumentComponentEvents.UNDO_CHECKOUT:
                                UndoCheckOutObject();
                                break;
                        }
                    }

                    ClearComment();

                    RaiseAfterAction(eventArgs);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowError(ResHelper.GetStringFormat("general.sourcecontrolerror", ex.Message));
                EventLogProvider.LogException(ObjectType, eventArgument, ex);
            }
        }


        /// <summary>
        /// Renders scripts
        /// </summary>
        public void RenderScripts()
        {
            if (RenderScript)
            {
                StringBuilder sb = new StringBuilder();

                // Submit script for next, previous and save action
                bool addComment = false;

                sb.Append("function CheckConsistency_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(this, String.Empty, false, false), "; } \n");
                if (IsActionAllowed(ComponentEvents.SAVE))
                {
                    sb.Append("function SaveObject_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(btnCommand, ComponentEvents.SAVE), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.CHECKOUT))
                {
                    sb.Append("function CheckOut_", ClientID, "() { ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.CHECKOUT, false, false), "; } \n");
                }
                if (IsActionAllowed(DocumentComponentEvents.CHECKIN))
                {
                    sb.Append("function CheckIn_", ClientID, "(comment) { SetComment_", ClientID, "(comment); ", GetSubmitScript(), ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.CHECKIN, false, false), "; } \n");
                    addComment = true;
                }
                if (IsActionAllowed(DocumentComponentEvents.UNDO_CHECKOUT))
                {
                    sb.Append("function UndoCheckOut_", ClientID, "() { if (!confirm('", GetUndoCheckOutConfirmation(InfoObject, ResourceCulture), "')) return false; ", ControlsHelper.GetPostBackEventReference(this, DocumentComponentEvents.UNDO_CHECKOUT, false, false), "; } \n");
                }

                if (addComment)
                {
                    // Register the dialog script
                    ScriptHelper.RegisterDialogScript(Page);

                    string url = ApplicationUrlHelper.ResolveDialogUrl("~/CMSModules/Objects/Dialogs/Comment.aspx");
                    sb.Append("function AddComment_", ClientID, "(name, objType, objId, menuId) { if((menuId == null) || (menuId == '')){ menuId = '", ClientID, "'; } modalDialog('", url, "?acname=' + name + '&objectType=' + objType + '&objectId=' + objId + '&menuId=' + menuId, 'CheckinComment', 770, 400, null, null, true); }\n");
                    sb.Append("function SetComment_", ClientID, "(comment) { document.getElementById('", HiddenComment.ClientID, "').value = comment; } \n");
                }

                // Register the script
                if (sb.Length > 0)
                {
                    ScriptHelper.RegisterStartupScript(Page, typeof(string), "ObjectManagement_" + ClientID, ScriptHelper.GetScript(sb.ToString()));
                }
            }
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="argument">Action argument</param>
        /// <param name="comment">Action comment</param>
        public string GetJSFunction(string action, string argument, string comment)
        {
            return GetJSFunctionInternal(action, argument, comment);
        }


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="arg">Argument (optional)</param>
        /// <param name="comment">Action comment (optional)</param>
        private string GetJSFunctionInternal(string action, string arg, string comment)
        {
            string name = null;

            if (string.IsNullOrEmpty(comment))
            {
                comment = "''";
            }

            switch (action)
            {
                case ComponentEvents.SAVE:
                    name = "SaveObject_{0}()";
                    break;

                case "CONS":
                    name = "CheckConsistency_{0}()";
                    break;

                case DocumentComponentEvents.CHECKOUT:
                    name = "CheckOut_{0}()";
                    break;

                case DocumentComponentEvents.CHECKIN:
                    name = "CheckIn_{0}({1})";
                    break;

                case DocumentComponentEvents.UNDO_CHECKOUT:
                    name = "UndoCheckOut_{0}()";
                    break;

                // Special case
                case ComponentEvents.COMMENT:
                    // actionName|objectType|objectId|menuId
                    string[] args = arg.Split('|');
                    return string.Format("AddComment_{0}({1},{2},{3},'{4}')", ClientID, args[0], args[1], args[2], ClientID);
            }

            return string.Format(name, ClientID, comment, arg, action);
        }


        /// <summary>
        /// Gets submit script for save changes support
        /// </summary>
        /// <returns>Submit script for save changes support</returns>
        public string GetSubmitScript()
        {
            return "if(window.SubmitAction != null) { window.SubmitAction(); } ";
        }


        /// <summary>
        /// Saves the object.
        /// </summary>
        public virtual void SaveObject()
        {
            SaveObject(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.SAVE));
        }


        /// <summary>
        /// Saves the object.
        /// </summary>
        public virtual void SaveObject(SimpleObjectManagerEventArgs args)
        {
            if (CanSaveObject())
            {
                SaveObjectInternal(args);
            }
            else
            {
                ShowCheckInFailureInfo();
            }
        }


        private void SaveObjectInternal(SimpleObjectManagerEventArgs args = null)
        {
            RaiseSaveData(args ?? new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.SAVE));
        }


        private bool CanSaveObject()
        {
            if (!SynchronizationHelper.UseCheckinCheckout || !InfoObject.TypeInfo.SupportsLocking)
            {
                return true;
            }

            GeneralizedInfo genInfo = InfoObject;

            if (genInfo.ObjectID == 0)
            {
                return true;
            }

            if (!genInfo.IsCheckedOut)
            {
                return false;
            }
            
            return InfoObject.ObjectSettings.ObjectCheckedOutByUserID == CMSActionContext.CurrentUser.UserID;
        }


        /// <summary>
        /// Checks the object in.
        /// </summary>
        /// <param name="comment">Comment of the check-in</param>
        public void CheckInObject(string comment)
        {
            CheckInObject(comment, InfoObject);
        }


        /// <summary>
        /// Checks the object in.
        /// </summary>
        /// <param name="comment">Comment of the check-in</param>
        /// <param name="infoObject">Info to checkin</param>
        public virtual void CheckInObject(string comment, BaseInfo infoObject)
        {
            try
            {
                VersionManager.CheckIn(infoObject, null, comment);
            }
            catch (ObjectVersioningException)
            {
                ShowInformation(GetString("objecteditmenu.checkinerror"));
            }
        }


        /// <summary>
        /// Checks the object in and saves it.
        /// </summary>
        /// <param name="comment">Comment of the check-in</param>
        public void CheckInAndSaveObject(string comment)
        {
            CheckInAndSaveObject(comment, InfoObject);
        }


        /// <summary>
        /// Checks the object in and saves it.
        /// </summary>
        /// <param name="comment">Comment of the check-in</param>
        /// <param name="infoObject">Info to checkin</param>
        public virtual void CheckInAndSaveObject(string comment, BaseInfo infoObject)
        {
            try
            {
                VersionManager.CheckIn(infoObject, null, comment);
                SaveObjectInternal();
            }
            catch (ObjectVersioningException)
            {
                ShowCheckInFailureInfo();
            }
        }


        /// <summary>
        /// Checks the object out.
        /// </summary>
        public void CheckOutObject()
        {
            CheckOutObject(InfoObject);
        }


        /// <summary>
        /// Checks the object out.
        /// </summary>
        /// <param name="infoObject">Info to checkout</param>
        public virtual void CheckOutObject(BaseInfo infoObject)
        {
            try
            {
                VersionManager.CheckOut(infoObject, MembershipContext.AuthenticatedUser);
            }
            catch (ObjectVersioningException ex)
            {
                ShowInformation(string.Format(GetString("objecteditmenu.checkouterror"), UserInfoProvider.GetUserNameById(ex.Settings.ObjectCheckedOutByUserID)));
            }
        }


        private void ShowCheckInFailureInfo()
        {
            if (InfoObject.ObjectSettings.ObjectCheckedOutByUserID > 0)
            {
                var userName = UserInfoProvider.GetUserNameById(InfoObject.ObjectSettings.ObjectCheckedOutByUserID);
                userName = HTMLHelper.HTMLEncode(userName);

                var objectType = HTMLHelper.HTMLEncode(InfoObject.TypeInfo.ObjectType);
                var objectDisplayName = HTMLHelper.HTMLEncode(InfoObject.Generalized.ObjectDisplayName);

                var infoMessage = string.Format(GetString("objecteditmenu.checkedoutbyanotheruser"), objectType, objectDisplayName, userName);
                ShowInformation(infoMessage);
            }
            else
            {
                ShowInformation(GetString("objecteditmenu.checkinerror"));
            }
        }


        /// <summary>
        /// Cancels the object checkout.
        /// </summary>
        public virtual void UndoCheckOutObject()
        {
            UndoCheckOutObject(InfoObject);
        }


        /// <summary>
        /// Cancels the object checkout.
        /// </summary>
        /// <param name="infoObject">Info to undo checkout</param>
        public virtual void UndoCheckOutObject(BaseInfo infoObject)
        {
            try
            {
                VersionManager.UndoCheckOut(infoObject);
            }
            catch (ObjectVersioningException)
            {
                if (infoObject.ObjectSettings.ObjectCheckedOutByUserID > 0)
                {
                    ShowInformation(GetString("objecteditmenu.undocheckoutunauthorized"));
                }
                else
                {
                    ShowInformation(GetString("objecteditmenu.undocheckouterror"));
                }
            }
        }


        /// <summary>
        /// Clears current comment.
        /// </summary>
        private void ClearComment()
        {
            hdnComment.Value = "";
        }


        /// <summary>
        /// Clears security result.
        /// </summary>
        public override void ClearProperties()
        {
            if (InfoObject != null)
            {
                RequestStockHelper.DropStorage(GetStorageKey(), false);
            }
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Gets the undo check out confirmation message for the specified info.
        /// Takes info versioning support into account.
        /// </summary>
        /// <param name="info">Info for which to display the message</param>
        /// <param name="resourceCulture">Resource string culture, optional</param>
        public static string GetUndoCheckOutConfirmation(BaseInfo info, string resourceCulture)
        {
            string resource = (info != null) && info.TypeInfo.SupportsVersioning ? "ObjectEditMenu.UndoCheckOutConfirmation" : "ObjectEditMenu.UndoCheckOutNoVersioningConfirmation";
            return resourceCulture == null ? ResHelper.GetString(resource) : ResHelper.GetString(resource, resourceCulture);
        }


        /// <summary>
        /// Finds current object manager and uses it to checkout current object.
        /// </summary>
        /// <param name="page">Page object</param>
        public static void CheckOutNewObject(Page page)
        {
            if (KeepNewObjectsCheckedOut)
            {
                CMSObjectManager manager = CMSObjectManager.GetCurrent(page);
                if (manager != null)
                {
                    manager.CheckOutObject();
                }
            }
        }

        #endregion


        #region "Consistency methods"

        /// <summary>
        /// Checks the default consistency for the editing context.
        /// </summary>
        /// <param name="args">Event arguments</param>
        protected override void CheckDefaultConsistency(SimpleObjectManagerEventArgs args)
        {
            if (InfoObject == null)
            {
                // Object is not initialized, do not continue
                args.IsValid = false;
            }
        }

        #endregion


        #region "Page methods"

        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Register();
        }


        /// <summary>
        /// Gets the current instance of the object manager
        /// </summary>
        /// <param name="control">Control from which to get the manager</param>
        public static CMSObjectManager GetCurrent(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            // Try to get from UI Context
            var ctx = UIContextHelper.GetUIContext(control);
            if (ctx != null)
            {
                return ctx["CMSObjectManager"] as CMSObjectManager;
            }

            return null;
        }


        private void Register()
        {
            // Ensure in current UI context
            var ctx = UIContextHelper.GetUIContext(this);
            if ((ctx != null) && (ctx["CMSObjectManager"] == null))
            {
                ctx["CMSObjectManager"] = this;
            }
        }


        /// <summary>
        /// CreateChildControls event handler.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (StopProcessing)
            {
                return;
            }

            btnCommand.Click += btnCommand_Click;

            Controls.Add(hdnComment);
            Controls.Add(btnCommand);
        }

        /// <summary>
        /// Button click event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        protected void btnCommand_Click(object sender, EventArgs e)
        {
            RaisePostBackEvent(ComponentEvents.SAVE);
        }


        /// <summary>
        /// Returns true if object is checked out or use checkin/out is not used
        /// </summary>
        public bool IsObjectChecked()
        {
            return (!SynchronizationHelper.UseCheckinCheckout || ((InfoObject != null) && (InfoObject.Generalized.IsCheckedOutByUserID > 0)));
        }

        /// <summary>
        /// OnLoad event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (StopProcessing)
            {
                return;
            }

            if (RegisterEvents)
            {
                // Register events
                if (SynchronizationHelper.UseCheckinCheckout && (Mode == FormModeEnum.Update))
                {
                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.CHECKIN, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.CHECKIN)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.CHECKIN))
                        {
                            CheckInObject(ActionComment);
                        }
                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.CHECKOUT, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.CHECKOUT)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.CHECKOUT))
                        {
                            CheckOutObject();
                        }
                        ClearComment();
                    });

                    ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.UNDO_CHECKOUT, (s, args) =>
                    {
                        // Check consistency
                        if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.UNDO_CHECKOUT)))
                        {
                            return;
                        }

                        if (IsActionAllowed(ComponentEvents.UNDO_CHECKOUT))
                        {
                            UndoCheckOutObject();
                        }
                        ClearComment();
                    });
                }

                // Register events
                ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.SAVE, (s, args) =>
                {
                    // Check consistency
                    if (!RaiseCheckConsistency(new SimpleObjectManagerEventArgs(InfoObject, Mode, ComponentEvents.SAVE)))
                    {
                        return;
                    }

                    if (IsActionAllowed(ComponentEvents.SAVE))
                    {
                        SaveObject();
                    }
                    ClearComment();
                });
            }
        }


        /// <summary>
        /// Render action.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (StopProcessing)
            {
                return;
            }

            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[CMSObjectManager Control : " + ClientID + " ]");
                return;
            }

            RenderScripts();
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Initializes security properties
        /// </summary>
        /// <param name="storageKey">Storage key for request</param>
        private void InitializeSecurityProperties(string storageKey)
        {
            if (InfoObject == null)
            {
                return;
            }

            // Properties already initialized
            if (RequestStockHelper.GetStorage(storageKey, false, false) != null)
            {
                return;
            }

            string siteName = InfoObject.Generalized.ObjectSiteName;
            if (string.IsNullOrEmpty(siteName))
            {
                siteName = SiteContext.CurrentSiteName;
            }

            bool allowModify;

            switch (Mode)
            {
                case FormModeEnum.Insert:
                    allowModify = MembershipContext.AuthenticatedUser.IsAuthorizedPerObject(PermissionsEnum.Create, InfoObject, siteName);
                    break;

                default:
                    allowModify = MembershipContext.AuthenticatedUser.IsAuthorizedPerObject(PermissionsEnum.Modify, InfoObject, siteName);
                    break;
            }

            bool useCheckinCheckout = SynchronizationHelper.UseCheckinCheckout;

            // Add results to the request
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.SAVE), allowModify, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.CHECKIN), allowModify && useCheckinCheckout, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.CHECKOUT), allowModify && useCheckinCheckout, false);
            RequestStockHelper.AddToStorage(storageKey, GetActionKey(ComponentEvents.UNDO_CHECKOUT), allowModify && useCheckinCheckout, false);
        }


        /// <summary>
        /// Checks if given action is allowed.
        /// </summary>
        public bool IsActionAllowed(string actionName)
        {
            // Object is not initialized
            if (InfoObject == null)
            {
                return true;
            }

            // Initialize security properties
            string storageKey = GetStorageKey();
            InitializeSecurityProperties(storageKey);

            // Check request stock helper
            string key = GetActionKey(actionName);
            if (RequestStockHelper.Contains(storageKey, key, false))
            {
                return ValidationHelper.GetBoolean(RequestStockHelper.GetItem(storageKey, key, false), false);
            }

            return false;
        }


        /// <summary>
        /// Returns action request key.
        /// </summary>
        private string GetActionKey(string actionName)
        {
            return actionName;
        }


        /// <summary>
        /// Returns storage key for current object.
        /// </summary>
        private string GetStorageKey()
        {
            var obj = InfoObject;

            return string.Format("ObjectManager_{0}_{1}_{2}", obj.TypeInfo.ObjectType, obj.Generalized.ObjectID, Mode);
        }

        #endregion
    }
}