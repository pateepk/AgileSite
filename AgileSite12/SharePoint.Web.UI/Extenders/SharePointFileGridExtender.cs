using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using CMS.SharePoint.Web.UI;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("SharePointFileGridExtender", typeof(SharePointFileGridExtender))]

namespace CMS.SharePoint.Web.UI
{
    /// <summary>
    /// SharePoint file listing extender
    /// </summary>
    public class SharePointFileGridExtender : ControlExtender<UniGrid>
    {
        private string SHAREPOINT_FILE_HANDLER_URL = UrlResolver.ResolveUrl("~/CMSPages/GetLocalSharePointFile.ashx");
        private string CANNOT_VIEW_IN_BROWSER_MESSAGE = ResHelper.GetString("SharePoint.File.CannotViewInBrowser");


        private CMSUIPage mUIPage;
        private SharePointLibraryInfo mLibrary;
        private HeaderAction mSynchronizeAction;
        private bool mSyncroniztionPerformed = false;


        /// <summary>
        /// Determines whether current user has modify permissions.
        /// </summary>
        private bool mHasModifyPermission;


        /// <summary>
        /// Determines whether the current edited library is in Read-only mode or not.
        /// </summary>
        private bool mLibraryIsInReadOnlyMode;


        /// <summary>
        /// Holds prepared formatting string for individual delete action scripts.
        /// </summary>
        private string mDeleteActionScript;


        /// <summary>
        /// Gets extended UI page.
        /// </summary>
        private CMSUIPage UIPage
        {
            get
            {
                return mUIPage ?? (mUIPage = (CMSUIPage)Control.Page);
            }
        }


        /// <summary>
        /// Gets edited library.
        /// </summary>
        private SharePointLibraryInfo Library
        {
            get
            {
                return mLibrary ?? (mLibrary = UIPage.EditedObject as SharePointLibraryInfo ?? UIPage.EditedObjectParent as SharePointLibraryInfo);
            }
        }


        /// <summary>
        /// Initializes the page
        /// </summary>
        public override void OnInit()
        {
            if (Library == null || !Library.CheckPermissions(PermissionsEnum.Read, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
            {
                CMSPage.RedirectToInformation("editedobject.notexists");

                return;
            }

            UIPage.Load += PageOnLoad;
            UIPage.PreRender += Page_OnPreRender;

            Control.OnExternalDataBound += Control_OnExternalDataBound;
            Control.OnAction += Control_OnAction;

            mHasModifyPermission = Library.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
            mLibraryIsInReadOnlyMode = SharePointConnectionInfoProvider.GetSharePointConnectionInfo(Library.SharePointLibrarySharePointConnectionID) == null;

            mDeleteActionScript = String.Format("if (window.confirm({0})) {{{{ return window.CMS.UG_{1}.command('deletesharepointfile', '{{0}}'); }}}} return false;", ScriptHelper.GetString(ResHelper.GetString("SharePoint.File.Delete.Confirm")), Control.ClientID);
        }


        /// <summary>
        /// Load event
        /// </summary>
        private void PageOnLoad(object sender, EventArgs eventArgs)
        {
            AddHeaderActions();
            UIPage.HeaderActions.ActionControlCreated.Before += ActionControlCreatedBefore;
        }


        /// <summary>
        /// Page pre render event
        /// </summary>
        private void Page_OnPreRender(object sender, EventArgs eventArgs)
        {
            var synchronizationState = SharePointLibraryInfoProvider.GetSharePointLibrarySynchronizationState(Library);
            ShowLibraryState(synchronizationState);
            var now = DateTime.Now;

            // Disable synchronization button if the synchronization is in progress or user cannot modify
            if (mSynchronizeAction != null)
            {
                mSynchronizeAction.Enabled &= !(synchronizationState.IsRunning || (synchronizationState.NextRunTime.Value <= now));
                mSynchronizeAction.Enabled &= !mLibraryIsInReadOnlyMode;
            }

            // Reload grid data if relevant 
            if (synchronizationState.IsRunning || ((synchronizationState.NextRunTime.Value > now) && mSyncroniztionPerformed))
            {
                Control.ReloadData();
            }
        }


        /// <summary>
        /// Handles the event invoked by the header action.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="eventArgs">Ignored</param>
        private void SharePointLibrarySynchronize_Handler(object sender, EventArgs eventArgs)
        {
            try
            {
                SynchronizeLibrary();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SharePoint", "SYNCHRONIZELIBRARY", ex);
                UIPage.ShowError(ResHelper.GetString("SharePoint.Library.SynchronizationError"));
            }
        }


        /// <summary>
        /// Handles the external data binding event of the grid.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="sourceName">Name of the external source</param>
        /// <param name="parameter">Parameter provided by the grid</param>
        /// <returns>Object to be displayed by the grid</returns>
        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName)
            {
                case "sharepointfiledownload":
                    CMSGridActionButton downloadButton = sender as CMSGridActionButton;
                    if (downloadButton != null)
                    {
                        int fileId = ValidationHelper.GetInteger(downloadButton.CommandArgument, -1);
                        var fileInfo = SharePointFileInfoProvider.GetSharePointFileInfo(fileId);
                        if (fileInfo != null)
                        {
                            downloadButton.OnClientClick = String.Format("window.open('{0}'); return false;", ScriptHelper.GetString(GetLocalSharePointFileHandler.GetSharePointFileUrl(SHAREPOINT_FILE_HANDLER_URL, fileId, true), false));
                        }

                        return downloadButton;
                    }

                    break;

                case "sharepointfileview":
                    CMSGridActionButton viewButton = sender as CMSGridActionButton;
                    if (viewButton != null)
                    {
                        int fileId = ValidationHelper.GetInteger(viewButton.CommandArgument, -1);
                        var fileInfo = SharePointFileInfoProvider.GetSharePointFileInfo(fileId);
                        if (fileInfo != null)
                        {
                            if (ImageHelper.IsMimeImage(fileInfo.SharePointFileMimeType))
                            {
                                viewButton.OnClientClick = String.Format("window.open('{0}'); return false;", ScriptHelper.GetString(GetLocalSharePointFileHandler.GetSharePointFileUrl(SHAREPOINT_FILE_HANDLER_URL, fileId), false));
                            }
                            else
                            {
                                viewButton.Enabled = false;
                                viewButton.ScreenReaderDescription = String.Empty;
                                viewButton.ToolTip = CANNOT_VIEW_IN_BROWSER_MESSAGE;
                            }
                        }

                        return viewButton;
                    }

                    break;

                case "sharepointfiledelete":
                    CMSGridActionButton deleteButton = sender as CMSGridActionButton;
                    if (deleteButton != null)
                    {
                        int fileId = ValidationHelper.GetInteger(deleteButton.CommandArgument, -1);
                        deleteButton.Enabled = mHasModifyPermission && !mLibraryIsInReadOnlyMode;
                        deleteButton.OnClientClick = String.Format(mDeleteActionScript, fileId);

                        return deleteButton;
                    }

                    break;

                case "sharepointfileupdate":
                    int sharePointFileId = ValidationHelper.GetInteger(parameter, -1);
                    var uploader = CreateIconUploader(sharePointFileId);

                    return uploader;
            }

            return null;
        }


        /// <summary>
        /// Action event handler. Handles the file deletion action.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="actionArgument"></param>
        private void Control_OnAction(string actionName, object actionArgument)
        {
            switch (actionName)
            {
                case "deletesharepointfile":
                    SharePointFileInfo fileInfo = SharePointFileInfoProvider.GetSharePointFileInfo(ValidationHelper.GetInteger(actionArgument, -1));
                    if (fileInfo != null)
                    {
                        try
                        {
                            SharePointFileInfoProvider.RecycleFile(fileInfo);
                        }
                        catch (SharePointConnectionNotFoundException)
                        {
                            UIPage.ShowError(ResHelper.GetString("SharePoint.File.Delete.NoConnectionError"));
                        }
                        catch (ArgumentException)
                        {
                            CMSPage.RedirectToInformation("editedobject.notexists");
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("SharePoint", "DELETEFILE", ex);
                            UIPage.ShowError(ResHelper.GetString("SharePoint.File.Delete.UnexpectedError"));
                        }
                    }

                    break;
            }
        }


        /// <summary>
        /// Adds necessary header actions to the page.
        /// </summary>
        private void AddHeaderActions()
        {
            var uploader = CreateUploader();

            var mUploadAction = new HeaderAction()
            {
                Text = "SharePointFileUploadHeaderAction",
                CommandName = "SharePointFileUploadHeaderAction",
            };
            UIPage.AddHeaderAction(mUploadAction);
            mSynchronizeAction = new HeaderAction()
            {
                Text = ResHelper.GetString("sharepoint.library.synchronize"),
                ButtonStyle = ButtonStyle.Default,
                CommandName = "SharePointLibrarySynchronize",
            };
            UIPage.AddHeaderAction(mSynchronizeAction);
            ComponentEvents.RequestEvents.RegisterForEvent("SharePointLibrarySynchronize", SharePointLibrarySynchronize_Handler);
        }


        /// <summary>
        /// Inserts uploader to header actions.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Event arguments</param>
        private void ActionControlCreatedBefore(object sender, HeaderActionControlCreatedEventArgs e)
        {
            string commandName = e.Action.CommandName;
            if (commandName == "SharePointFileUploadHeaderAction")
            {
                e.ActionControl = CreateUploader();

                e.Cancel();
            }
        }


        /// <summary>
        /// Creates the uploader control with all the specifications
        /// </summary>
        /// <returns>The uploader control</returns>
        private DirectUploaderWebControl CreateUploader()
        {
            var uploader = new DirectUploaderWebControl(UIPage);
            uploader.Uploader.Text = ResHelper.GetString("SharePoint.File.New");
            uploader.Uploader.UploadHandler.AttachAdditionalParameters += UploadHandlerAttachAdditionalParameters;
            uploader.Uploader.UploadHandler.UploadHandlerUrl = "~/CMSModules/SharePoint/CMSPages/MultiFileUploader.ashx";
            uploader.Uploader.Multiselect = true;
            uploader.Uploader.ButtonStyle = ButtonStyle.Primary;
            uploader.Uploader.AllowedExtensions = null;
            uploader.Uploader.ShowProgress = true;
            uploader.Uploader.UploadMode = MultifileUploaderModeEnum.DirectSingle;

            // Set up javascript module
            uploader.JavaScriptModuleName = SharePointMultiFileUploader.DEFAULT_JS_MODULE_NAME;
            uploader.JavascriptModuleParameters = null;

            // Disable uploader if necessary
            uploader.Enabled = mHasModifyPermission && !mLibraryIsInReadOnlyMode;

            // Make sure uploader works even after postback
            uploader.Uploader.ForceLoad = true;

            return uploader;
        }


        /// <summary>
        /// Creates the small Grid uploader control with all the specifications
        /// </summary>
        /// <param name="fileId">Id of the file that will be update through this uploader</param>
        /// <returns></returns>
        private DirectUploaderWebControl CreateIconUploader(int fileId)
        {
            var uploader = CreateUploader();

            uploader.Uploader.ShowIconMode = true;
            uploader.Uploader.Multiselect = false;
            uploader.Uploader.ObjectID = fileId;
            uploader.Uploader.UploadHandler.AttachAdditionalParameters += IconUploadHandlerAttachAdditionalParameters;

            return uploader;
        }


        /// <summary>
        /// Makes sure that the file's ID is included in additional parameters
        /// </summary>
        /// <param name="sender">MultiFileUploader indirectly included in a DirecFileUploader</param>
        /// <param name="e">Event arguments containing the collection of additional arguments</param>
        private void IconUploadHandlerAttachAdditionalParameters(object sender, MfuAdditionalParameterEventArgs e)
        {
            DirectFileUploader uploader = ((CMSUserControl)sender).Parent.Parent as DirectFileUploader;
            if (uploader != null)
            {
                e.AddParameter(SharePointMultiFileUploader.SHAREPOINT_FILE_ID_PARAMETER_NAME, uploader.ObjectID.ToString());
            }
        }


        /// <summary>
        /// Adds necessary parameters to the additional parameter collection for the uploader.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Event arguments containing the collection of additional arguments</param>
        private void UploadHandlerAttachAdditionalParameters(object sender, MfuAdditionalParameterEventArgs e)
        {
            e.AddParameter(SharePointMultiFileUploader.SHAREPOINT_LIBRARY_ID_PARAMETER_NAME, Library.SharePointLibraryID.ToString());
        }


        /// <summary>
        /// Synchronizes SharePoint library.
        /// </summary>
        private void SynchronizeLibrary()
        {
            if (Library == null || mLibraryIsInReadOnlyMode)
            {
                return;
            }

            SharePointLibraryInfoProvider.SynchronizeSharePointLibrary(Library);
            mSyncroniztionPerformed = true;
        }


        /// <summary>
        /// Ensures correct and sufficient information is displayed to user about the SharePoint library
        /// </summary>
        /// <param name="synchronizationState">Current state of the synchronization process.</param>
        private void ShowLibraryState(SharePointLibrarySynchronizationState synchronizationState)
        {
            if (mLibraryIsInReadOnlyMode)
            {
                UIPage.ShowWarning(ResHelper.GetString("SharePoint.Library.ReadOnlyWarning"));
                mSynchronizeAction.Enabled = false;

                return;
            }
            else if (!mHasModifyPermission)
            {
                UIPage.ShowWarning(ResHelper.GetString("SharePoint.Library.CannotModify"));
                mSynchronizeAction.Enabled = false;
            }
            ShowSynchronizationState(synchronizationState);
        }


        /// <summary>
        /// Shows information about current synchronization state.
        /// </summary>
        /// <param name="synchronizationState">Current state of the synchronization process.</param>
        private void ShowSynchronizationState(SharePointLibrarySynchronizationState synchronizationState)
        {
            if (synchronizationState.IsRunning)
            {
                // Synchronization is running
                UIPage.ShowInformation(ResHelper.GetString("sharepoint.library.synchronizationrunning"));
            }
            else if (synchronizationState.NextRunTime.Value <= DateTime.Now)
            {
                // Synchronization is queued in the scheduler to be run ASAP
                UIPage.ShowInformation(ResHelper.GetString("SharePoint.Library.SynchronizationASAP"));
            }
            else
            {
                // Synchronization is scheduled for future
                ShowLastRunResult(synchronizationState);
            }
        }


        /// <summary>
        /// Shows the result of the latest synchronization
        /// </summary>
        /// <param name="synchronizationState">Synchronization state of the library</param>
        private void ShowLastRunResult(SharePointLibrarySynchronizationState synchronizationState)
        {
            if (synchronizationState.LastRunTime.HasValue)
            {
                // Synchronization has been run at least once
                string lastRunString = TimeZoneHelper.ConvertToUserTimeZone(synchronizationState.LastRunTime.Value, false, MembershipContext.AuthenticatedUser, SiteContext.CurrentSite);
                if (String.IsNullOrEmpty(synchronizationState.LastResult))
                {
                    // Last synchronization was successful
                    UIPage.ShowInformation(String.Format(ResHelper.GetString("sharepoint.library.lastsynchronization"), lastRunString));
                }
                else
                {
                    // Last synchronization finished with an error
                    UIPage.ShowError(String.Format(ResHelper.GetString("sharepoint.library.lastsynchronization.error"), lastRunString, HTMLHelper.HTMLEncode(synchronizationState.LastResult)));
                }
            }
        }
    }
}
