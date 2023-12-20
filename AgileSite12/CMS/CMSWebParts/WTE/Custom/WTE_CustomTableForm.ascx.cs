using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.CustomTables;
using CMS.OnlineForms;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.WebAnalytics;

using CMS.Base.Web.UI.ActionsConfig;
using CMS.EventLog;
using CMS.Globalization;
using CMS.HealthMonitoring;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.UIControls;
using CMS.URLRewritingEngine;
using CMS.WebFarmSync;
using CMS.WinServiceEngine;

using Newtonsoft.Json;
using System.Runtime.Serialization.Json;

namespace CMSApp.CMSWebParts.WTE.Custom
{
    public partial class WTE_CustomTableForm : CMSAbstractWebPart
    {
        #region "Properties"

        #region "Table Properties"

        /// <summary>
        /// Custom table used for edit item.
        /// </summary>
        public string CustomTable
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CustomTable"), String.Empty);
            }
            set
            {
                SetValue("CustomTable", value);
            }
        }

        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public int ItemID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue(ItemKeyName), 0);
            }
            set
            {
                SetValue(ItemKeyName, value);
            }
        }

        /// <summary>
        /// Key name used to identify edited object.
        /// </summary>
        public string ItemKeyName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ItemKeyName"), "edititemid");
            }
            set
            {
                SetValue("ItemKeyName", value);
            }
        }

        /// <summary>
        /// Gets or sets the alternative form full name (ClassName.AlternativeFormName).
        /// </summary>
        public string AlternativeFormName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AlternativeFormName"), "");
            }
            set
            {
                SetValue("AlternativeFormName", value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether the WebPart use colon behind label.
        /// </summary>
        public bool UseColonBehindLabel
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UseColonBehindLabel"), true);
            }
            set
            {
                SetValue("UseColonBehindLabel", value);
            }
        }

        /// <summary>
        /// Gets or sets the conversion track name used after successful registration.
        /// </summary>
        public string TrackConversionName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TrackConversionName"), "");
            }
            set
            {
                if (value.Length > 400)
                {
                    value = value.Substring(0, 400);
                }
                SetValue("TrackConversionName", value);
            }
        }

        /// <summary>
        /// Gets or sets the conversion value used after successful registration.
        /// </summary>
        public double ConversionValue
        {
            get
            {
                return ValidationHelper.GetDoubleSystem(GetValue("ConversionValue"), 0);
            }
            set
            {
                SetValue("ConversionValue", value);
            }
        }

        // <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string SiteName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SiteName"), "");
            }
            set
            {
                SetValue("SiteName", value);
            }
        }

        #endregion "Table Properties"

        #region "Messaging"

        /// <summary>
        /// Gets or sets the message which is displayed after validation failed.
        /// </summary>
        public string ValidationErrorMessage
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ValidationErrorMessage"), "");
            }
            set
            {
                SetValue("ValidationErrorMessage", value);
            }
        }

        /// <summary>
        /// Instruction for add
        /// </summary>
        public string InsertConfirmationText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("InsertConfirmationText"), "<font color=\"green\">Record added.</font>");
            }
            set
            {
                SetValue("InsertConfirmationText", value);
            }
        }

        /// <summary>
        /// Edit Instruction Text
        /// </summary>
        public string UpdateConfirmationText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UpdateConfirmationText"), "<font color=\"green\">Record updated.</font>");
            }
            set
            {
                SetValue("UpdateConfirmationText", value);
            }
        }

        /// <summary>
        /// Confirmation Text
        /// </summary>
        public string ConfirmationText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ConfirmationText"), String.Empty);
            }
            set
            {
                SetValue("ConfirmationText", value);
            }
        }

        /// <summary>
        /// Show instruction messages
        /// </summary>
        public bool ShowInstruction
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowInstruction"), true);
            }
            set
            {
                SetValue("ShowInstruction", value);
            }
        }

        /// <summary>
        /// Instruction for add
        /// </summary>
        public string InsertInstructionText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("InsertInstructionText"), "<font color=\"blue\">Fill in fields and click OK to add.</font>");
            }
            set
            {
                SetValue("InsertInstructionText", value);
            }
        }

        /// <summary>
        /// Edit Instruction Text
        /// </summary>
        public string UpdateInstructionText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("UpdateInstructionText"), "<font color=\"blue\">Update information and click OK to update.</font>");
            }
            set
            {
                SetValue("UpdateInstructionText", value);
            }
        }

        /// <summary>
        /// Copy instruction text
        /// </summary>
        public string CopyInstructionText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("EditInstructionText"), "<font color=\"blue\">Item Copied, please update information and click OK to update.</font>");
            }
            set
            {
                SetValue("EditInstructionText", value);
            }
        }

        #endregion "Messaging"

        #region "Buttons"

        /// <summary>
        /// Add Cancel button
        /// </summary>
        public bool ShowCancelButton
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ShowCancelButton"), true);
            }
            set
            {
                SetValue("ShowCancelButton", value);
            }
        }

        /// <summary>
        /// The text for the cancel button
        /// </summary>
        public string CancelButtonText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CancelButtonText"), String.Empty);
            }
            set
            {
                SetValue("CancelButtonText", value);
            }
        }

        /// <summary>
        /// The css for the cancel button
        /// </summary>
        public string CancelButtonCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CancelButtonCssClass"), String.Empty);
            }
            set
            {
                SetValue("CancelButtonCssClass", value);
            }
        }

        /// <summary>
        /// Submit button text
        /// </summary>
        public string SubmitButtonText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubmitButtonText"), "Submit");
            }
            set
            {
                SetValue("SubmitButtonText", value);
            }
        }

        /// <summary>
        /// The css for the submit
        /// </summary>
        public string SubmitButtonCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SubmitButtonCssClass"), String.Empty);
            }
            set
            {
                SetValue("SubmitButtonCssClass", value);
            }
        }

        /// <summary>
        /// Move button location
        /// </summary>
        public bool MoveButtons
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("MoveButtons"), true);
            }
            set
            {
                SetValue("MoveButtons", value);
            }
        }

        /// <summary>
        /// Button container name
        /// </summary>
        public string ButtonContainerID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ButtonContainerID"), "ncpbuttonplaceholder");
            }
            set
            {
                SetValue("ButtonContainerID", value);
            }
        }

        /// <summary>
        /// Button container css class
        /// </summary>
        public string ButtonContainerCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ButtonContainerCssClass"), "action-btns");
            }
            set
            {
                SetValue("ButtonContainerCssClass", value);
            }
        }

        #endregion "Buttons"

        #region "Redirection"

        /// <summary>
        /// Gets or sets the URL that the cancel button should redirect to.
        /// </summary>
        public string CancelURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CancelURL"), String.Empty);
            }
            set
            {
                SetValue("CancelURL", value);
            }
        }

        /// <summary>
        /// The URL to redirect to after submit or insert
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("RedirectURL"), String.Empty);
            }
            set
            {
                SetValue("RedirectURL", value);
            }
        }

        #endregion "Redirection"

        #region "Work flow"

        /// <summary>
        /// Process table workflow
        /// </summary>
        /// <returns></returns>
        public bool EnableWorkFlow
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EnableWorkFlow"), false);
            }
            set
            {
                SetValue("EnableWorkFlow", value);
            }
        }

        /// <summary>
        /// name of the store procedure that process the workflow.
        /// </summary>
        public string WorkFlowSprocName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WorkFlowSprocName"), String.Empty);
            }
            set
            {
                SetValue("WorkFlowSprocName", value);
            }
        }

        /// <summary>
        /// The name of the parameter to pass to pass the Item Key to the stored procedure
        /// </summary>
        public string WorkFlowItemIDParamName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WorkFlowItemIDParamName"), String.Empty);
            }
            set
            {
                SetValue("WorkFlowItemIDParamName", value);
            }
        }

        /// <summary>
        /// Name of The field to pass as the param to the sproc
        /// </summary>
        public string WorkFlowItemIDKey
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WorkFlowItemIDKey"), String.Empty);
            }
            set
            {
                SetValue("WorkFlowItemIDKey", value);
            }
        }

        /// <summary>
        /// The name of the parameter to pass to pass the WorkFlowID to the stored procedure
        /// </summary>
        public string WorkFlowIDParamName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WorkFlowIDParamName"), String.Empty);
            }
            set
            {
                SetValue("WorkFlowIDParamName", value);
            }
        }

        /// <summary>
        /// The work flow ID (for specific work flow procesing)
        /// </summary>
        public string WorkFlowID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("WorkFlowID"), String.Empty);
            }
            set
            {
                SetValue("WorkFlowID", value);
            }
        }

        #endregion "Work flow"

        #endregion "Properties"

        #region "Page Events"

        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            form.OnAfterSave += form_OnAfterSave;
            form.OnBeforeSave += form_OnBeforeSave;
            base.OnLoad(e);
            AddCancelButton();
        }

        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            SetupControl();
        }

        /// <summary>
        /// Reloads data for partial caching.
        /// </summary>
        public override void ReloadData()
        {
            base.ReloadData();
            SetupControl();
        }

        /// <summary>
        /// The page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            SetupControl();
            if (!RequestHelper.IsPostBack() && !IsPostBack)
            {
                AddActionMessage();
            }

            // Everything here should have already been done in the "onload" set up control

            /*
            if (!Page.IsPostBack)
            {
                //Item ID
                int key = 0;
                string customTable = CustomTable; //Code name of custom table
                if (QueryHelper.GetString("action", "").ToLower() == "edit")
                {
                    key = QueryHelper.GetInteger("RecordKey", 0); //Get query parameter from URL
                }

                //Definition of custom table in this case
                DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);

                form.CustomTableId = dataClassInfo.ClassID;
                //if we want an alternative form then add this in
                //form.AlternativeFormFullName = "customtable.SampleTable.test";
                form.SiteName = SiteContext.CurrentSite.SiteName;
                form.ItemID = key;
                form.ShowPrivateFields = true;

                if (key == 0)
                {
                    form.Mode = FormModeEnum.Insert;
                    //form.AddInformation("Fill in fields and click OK to add.", "<br/>");
                }
                else
                {
                    //form.AddInformation("Update information and click OK to update.", "<br/>");
                    form.Mode = FormModeEnum.Update;

                    // Custom code for modification of field   
                    form.OnAfterDataLoad += new EventHandler(form_OnAfterDataLoad);
                }
            }
            */
        }

        /// <summary>
        /// On prerender event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            /*
            foreach (EditingFormControl ctrl1 in form.FieldEditingControls.Values)
            {
                lblMsg.Text += ctrl1.ID + ", ";
            }
            */

            if (form.FieldEditingControls != null)
            {
                //force site id to be current site id and disabled
                EditingFormControl ctrl = form.FieldEditingControls["siteid"] as EditingFormControl;
                if (ctrl != null)
                {
                    // Replace value of NameOfField field with value of Column2
                    ctrl.Value = SiteContext.CurrentSiteID;
                    ctrl.Enabled = false;
                }
            }
            else
            {
                lblMsg.Text = "You do not have permission to edit this item.";
            }
        }

        #endregion "Page Events"

        #region "form binding"

        /// <summary>
        /// Setup control.
        /// </summary>
        protected void SetupControl()
        {
            if (StopProcessing)
            {
                // Do nothing
                form.StopProcessing = true;
            }
            else
            {
                DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(CustomTable);

                if (customTable == null)
                {
                    return;
                }

                form.CustomTableId = customTable.ClassID;
                form.UseColonBehindLabel = UseColonBehindLabel;
                form.ValidationErrorMessage = ValidationErrorMessage;
                form.AlternativeFormFullName = AlternativeFormName;

                form.ItemID = GetItemKeyID();

                form.SiteName = SiteContext.CurrentSite.SiteName;

                AddCancelButton();

                if (form.ItemID > 0)
                {
                    CheckReadPermissions(customTable);
                }
            }
        }

        /// <summary>
        /// Add the cancel button
        /// </summary>
        protected void AddCancelButton()
        {
            bool hasCancelButton = false;

            // add the cancel button
            // if (!String.IsNullOrWhiteSpace(CancelURL))
            {
                // find/create the cancel button
                LocalizedButton cancelBtn = form.FindControl("btnCancel") as LocalizedButton;
                hasCancelButton = cancelBtn != null;
                LocalizedButton cancelButton = null;
                if (!hasCancelButton && ShowCancelButton && !String.IsNullOrWhiteSpace(CancelURL))
                {
                    cancelButton = new LocalizedButton
                    {
                        ID = "btnCancel",
                        Text = !String.IsNullOrWhiteSpace(CancelButtonText) ? CancelButtonText : ResHelper.GetString("general.cancel"),
                        CssClass = !String.IsNullOrWhiteSpace(CancelButtonCssClass) ? CancelButtonCssClass : form.FormButtonCssClass,
                        CausesValidation = false,
                        EnableViewState = false,
                        OnClientClick = string.Format("document.location.href='{0}';return false;", CancelURL)
                    };
                }

                if (ShowCancelButton || MoveButtons)
                {
                    Control container = null;
                    if (MoveButtons)
                    {
                        container = form.FindControl(ButtonContainerID);
                    }

                    // find the submit button.
                    FormSubmitButton submitButton = form.FindControl(form.SubmitButton.ID) as FormSubmitButton;
                    if (submitButton != null)
                    {
                        // rebuild the button.
                        if (!String.IsNullOrWhiteSpace(SubmitButtonText))
                        {
                            submitButton.ResourceString = String.Empty;
                            submitButton.Text = SubmitButtonText;
                        }

                        if (!String.IsNullOrWhiteSpace(SubmitButtonCssClass))
                        {
                            submitButton.CssClass = SubmitButtonCssClass;
                        }

                        HtmlGenericControl test = new HtmlGenericControl("i");
                        test.Attributes["class"] = "fas fa-check-circle";
                        submitButton.Controls.AddAt(0, test);
                    }

                    if (container != null)
                    {
                        ((WebControl)container).CssClass = ButtonContainerCssClass;
                        container.Controls.Clear();
                        if (submitButton != null)
                        {
                            // Remove it from the original location.
                            int idx = -1;

                            if (submitButton.Parent != null)
                            {
                                idx = submitButton.Parent.Controls.IndexOf(submitButton);
                            }

                            if (idx >= 0)
                            {
                                submitButton.Parent.Controls.RemoveAt(submitButton.Parent.Controls.IndexOf(submitButton));
                            }

                            container.Controls.Add(submitButton);
                        }

                        if (hasCancelButton)
                        {
                            container.Controls.Add(new LiteralControl("&nbsp;"));
                            container.Controls.Add(cancelBtn);
                        }
                        else
                        {
                            if (cancelButton != null)
                            {
                                container.Controls.Add(new LiteralControl("&nbsp;"));
                                container.Controls.Add(cancelButton);
                            }
                        }
                    }
                    else
                    {
                        if (submitButton != null)
                        {
                            if (hasCancelButton)
                            {
                                submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 1, new LiteralControl("&nbsp;"));
                                submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 2, cancelBtn);
                            }
                            else
                            {
                                if (cancelButton != null)
                                {
                                    submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 1, new LiteralControl("&nbsp;"));
                                    submitButton.Parent.Controls.AddAt(submitButton.Parent.Controls.IndexOf(submitButton) + 2, cancelButton);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add action message
        /// </summary>
        protected void AddActionMessage()
        {
            string action = QueryHelper.GetString("action", "").ToLower();
            switch (action)
            {
                case "edit":
                case "update":
                    if (ShowInstruction)
                    {
                        form.AddInformation(UpdateInstructionText, "<br/>");
                    }
                    form.Mode = FormModeEnum.Update;
                    form.OnAfterDataLoad += new EventHandler(form_OnAfterDataLoad);
                    break;

                case "copy":
                    if (ShowInstruction)
                    {
                        form.AddInformation(CopyInstructionText, "<br/>");
                    }
                    form.Mode = FormModeEnum.Update;
                    form.OnAfterDataLoad += new EventHandler(form_OnAfterDataLoad);
                    break;

                case "add":
                    if (ShowInstruction)
                    {
                        form.AddInformation(InsertInstructionText, "<br/>");
                    }
                    form.Mode = FormModeEnum.Insert;
                    break;

                default:
                    // do nothing.
                    break;
            }
        }

        #endregion "form binding"

        #region "Event Handlers"

        /// <summary>
        /// OnBeforeSave event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void form_OnBeforeSave(object sender, EventArgs e)
        {
            CheckPermissions();
        }

        /// <summary>
        /// OnAfterSave event handler
        /// </summary>
        protected void form_OnAfterSave(object sender, EventArgs e)
        {
            if (form.IsInsertMode || form.Mode == FormModeEnum.Insert)
            {
                if (TrackConversionName != String.Empty)
                {
                    string siteName = SiteContext.CurrentSiteName;

                    if (AnalyticsHelper.AnalyticsEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && !AnalyticsHelper.IsIPExcluded(siteName, RequestContext.UserHostAddress))
                    {
                        HitLogProvider.LogConversions(SiteContext.CurrentSiteName, LocalizationContext.PreferredCultureCode, TrackConversionName, 0, ConversionValue);
                    }
                }
                form.AddInformation(InsertConfirmationText, "<br/>");
            }
            else
            {
                form.AddInformation(UpdateConfirmationText, "<br/>");
            }

            if (EnableWorkFlow)
            {
                string message = String.Empty;
                string companyID = GetStringOrNull(form.GetDataValue("ItemID"));
                Object data2 = form.Data;
                bool success = PerformWorkFlow(companyID, out message);
                form.AddInformation(message, "<br/>");

                if (success)
                {
                    // dump the site's cache
                    ClearCache();
                }
            }

            string redirectUrl = RedirectURL;
            if (String.IsNullOrWhiteSpace(RedirectURL))
            {
                // Redirect to edit mode after new item submit
                redirectUrl = URLHelper.AddParameterToUrl(RequestContext.CurrentURL.Replace("=Add", "=edit").Replace("=add", "=edit"), ItemKeyName, form.ItemID.ToString());
            }
            URLHelper.Redirect(redirectUrl);
            //URLHelper.Redirect(URLHelper.AddParameterToUrl(RequestContext.CurrentURL, ItemKeyName, form.ItemID.ToString()));
        }

        /// <summary>
        /// OnAfterLoad event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void form_OnAfterDataLoad(object sender, EventArgs e)
        {
            if (form.FieldControls != null)
            {
                // Get form control information
                EditingFormControl ctrl = form.FieldEditingControls["to"] as EditingFormControl;
                if (ctrl != null)
                {
                    // Replace value of NameOfField field with value of Column2
                    ctrl.Value = ValidationHelper.GetString(form.GetFieldValue("from"), "");
                }
            }
        }

        #endregion "Event Handlers"

        #region "Permission checking"

        /// <summary>
        /// Checks create or modify permission.
        /// </summary>
        private void CheckPermissions()
        {
            CustomTableItem item = form.EditedObject;
            // If editing item
            if (item.ItemID > 0)
            {
                // Check 'Modify' permission
                if (!item.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
                {
                    // Show error message
                    form.MessagesPlaceHolder.ClearLabels();
                    form.ShowError(String.Format(GetString("customtable.permissiondenied.modify"), item.ClassName));
                    form.StopProcessing = true;
                }
            }
            else
            {
                // Check 'Create' permission
                if (!item.CheckPermissions(PermissionsEnum.Create, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
                {
                    // Show error message
                    form.MessagesPlaceHolder.ClearLabels();
                    form.ShowError(String.Format(GetString("customtable.permissiondenied.create"), item.ClassName));
                    form.StopProcessing = true;
                }
            }
        }

        /// <summary>
        /// Check Read permission?
        /// </summary>
        /// <param name="customTable"></param>
        private void CheckReadPermissions(DataClassInfo customTable)
        {
            // Check 'Read' permission
            if (customTable.CheckPermissions(PermissionsEnum.Read, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
            {
                return;
            }

            // Show error message
            form.MessagesPlaceHolder.ClearLabels();
            form.ShowError(String.Format(GetString("customtable.permissiondenied.read"), customTable.ClassName));
            form.StopProcessing = true;
        }

        #endregion "Permission checking"

        #region "work flow"

        /// <summary>
        /// Process work flow
        /// </summary>
        /// <param name="p_companyID"></param>
        /// <returns></returns>
        public bool PerformWorkFlow(string p_companyID, out string p_message)
        {
            bool success = true;
            p_message = String.Empty;
            DataSet ds = null;

            try
            {
                bool processWorkFlow = EnableWorkFlow;
                string workFlowSprocName = WorkFlowSprocName.Replace("dbo.", String.Empty).Trim();
                string workFlowItemIDParamName = WorkFlowItemIDParamName.Replace("@", String.Empty).Trim();
                string workFlowItemIDKey = WorkFlowItemIDKey;
                string workFlowIDParamName = WorkFlowIDParamName;
                string workFlowID = WorkFlowID;

                if (processWorkFlow && !String.IsNullOrWhiteSpace(workFlowItemIDParamName))
                {
                    GeneralConnection conn = ConnectionHelper.GetConnection();
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@" + workFlowItemIDParamName, p_companyID);

                    QueryParameters qp = new QueryParameters(workFlowSprocName, parameters, QueryTypeEnum.StoredProcedure, false);
                    ds = conn.ExecuteQuery(qp);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        if (row != null)
                        {
                            p_message += GetStringOrNull(row["Message"]);
                            success = GetBoolOrNull(row["Success"]).GetValueOrDefault(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return success;
        }

        #endregion "work flow"

        #region "Helpers

        /// <summary>
        /// Clear all caching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void ClearCache(object sender = null, EventArgs args = null)
        {
            if (StopProcessing)
            {
                return;
            }

            // Clear the cache
            CacheHelper.ClearCache(null, true);
            Functions.ClearHashtables();

            // Drop the routes
            CMSDocumentRouteHelper.DropAllRoutes();

            // Disposes all zip files
            ZipStorageProvider.DisposeAll();

            // Collect the memory
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // no need to log
            // Log event
            //EventLogProvider.LogEvent(EventType.INFORMATION, "System", "CLEARCACHE", GetString("Administration-System.ClearCacheSuccess"));

            //ShowConfirmation(GetString("Administration-System.ClearCacheSuccess"));
        }

        /// <summary>
        /// Get the item id using keys
        /// </summary>
        /// <returns></returns>
        protected int GetItemKeyID()
        {
            return QueryHelper.GetInteger(ItemKeyName, QueryHelper.GetInteger("RecordKey", 0));
        }

        /// <summary>
        /// Get the Custom Table Item
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        protected CustomTableItem GetCustomTableItem(out string p_message)
        {
            CustomTableItem item = null;
            p_message = String.Empty;
            string customTable = CustomTable; //Code name of custom table
            int key = GetItemKeyID();
            if (key > 0)
            {
                p_message = "&nbsp;KeyId=" + key.ToString();
                //Definition of custom table in this case
                DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);
                if (dataClassInfo != null)
                {
                    //item = dataClassInfo.Get
                    item = CustomTableItemProvider.GetItem(key, customTable);
                    ObjectQuery<CustomTableItem> item2 = CustomTableItemProvider.GetItems(customTable).WhereID("ItemID", key);
                }
                if (item == null)
                {
                    p_message += "(Item Not Found)";
                }
            }
            return item;
        }

        /// <summary>
        /// Get Value from a custom table item column
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected Object GetCustomTableItemValue(CustomTableItem p_item, string p_key)
        {
            Object ret = null;
            if (p_item != null)
            {
                ret = p_item.GetValue(p_key);
            }
            return ret;
        }

        /// <summary>
        /// Set value of a custom table item column
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        protected void SetCustomTableItemValue(CustomTableItem p_item, string p_key, Object p_value)
        {
            if (p_item != null)
            {
                p_item.SetValue(p_key, p_value);
            }
        }

        /// <summary>
        /// Get Session data
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSession(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        ret = (string)HttpContext.Current.Session[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void SetSession(string p_key, object p_value)
        {
            try
            {
                if (p_value == null)
                {
                    ClearSession(p_key);
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Session != null)
                        {
                            HttpContext.Current.Session[p_key] = p_value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void ClearSession(string p_key)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session.Remove(p_key);
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get Int value or null
        /// </summary>
        /// <param name="p_object"></param>
        /// <returns></returns>
        protected int? GetIntOrNull(object p_object)
        {
            int? ret = null;
            int val = 0;
            if (int.TryParse(GetStringOrNull(p_object), out val))
            {
                ret = val;
            }
            return ret;
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_object"></param>
        /// <returns></returns>
        protected string GetStringOrNull(object p_object)
        {
            string ret = String.Empty;
            try
            {
                if (p_object != null)
                {
                    ret = p_object.ToString();
                }
            }
            catch (Exception)
            {
                // ignore it
            }
            return ret;
        }

        /// <summary>
        /// Get boolean value of Null
        /// </summary>
        /// <param name="p_object"></param>
        /// <returns></returns>
        protected bool? GetBoolOrNull(object p_object)
        {
            bool? ret = null;
            bool val = false;
            if (bool.TryParse(GetStringOrNull(p_object), out val))
            {
                ret = val;
            }
            return ret;
        }

        #endregion "Helpers
    }
}