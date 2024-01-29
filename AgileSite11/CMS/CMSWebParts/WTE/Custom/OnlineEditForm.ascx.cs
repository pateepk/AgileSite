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
using CMS.FormEngine;
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
    public partial class CMSWebParts_BizForms_OnlineEditForm : CMSAbstractWebPart
    {
        #region "Constants"

        public const string CANCEL_BUTTON_ID = "btnCancel";
        public const string PREVIOUS_BUTTON_ID = "btnPrevious";
        public const string NEXT_BUTTON_ID = "btnNext"; // this we'll pull dynamically from the form submit button for now.

        #endregion "Constants"

        #region "Properties"

        #region "Form Properties"

        /// <summary>
        /// Gets or sets the form name of BizForm.
        /// </summary>
        public string BizFormName
        {
            get
            {
                return GetStringProperty("BizFormName", String.Empty);
            }
            set
            {
                SetValue("BizFormName", value);
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
                return ValidationHelper.GetString(GetValue("ItemKeyName"), "ItemID");
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

        /// <summary>
        /// Gets or sets the default form layout.
        /// </summary>
        public string DefaultFormLayout
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DefaultFormLayout"), FormLayoutEnum.SingleTable.ToString());
            }
            set
            {
                SetValue("DefaultFormLayout", value);
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

        #endregion "Form Properties"

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
        /// Has previous button
        /// </summary>
        public bool HasPreviousButton
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("HasPreviousButton"), false);
            }
            set
            {
                SetValue("HasPreviousButton", value);
            }
        }

        public string PreviousButtonText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviousButtonText"), "Previous");
            }
            set
            {
                SetValue("PreviousButtonText", value);
            }
        }

        public string PreviousButtonCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviousButtonCssClass"), String.Empty);
            }
            set
            {
                SetValue("PreviousButtonCssClass", value);
            }
        }

        /// <summary>
        /// Has next button
        /// </summary>
        public bool HasNextButton
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("HasNextButton"), false);
            }
            set
            {
                SetValue("HasNextButton", value);
            }
        }

        public string NextButtonText
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NextButtonText"), "Next");
            }
            set
            {
                SetValue("NextButtonText", value);
            }
        }

        public string NextButtonCssClass
        {
            get
            {
                return ValidationHelper.GetString(GetValue("NextButtonCssClass"), String.Empty);
            }
            set
            {
                SetValue("NextButtonCssClass", value);
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

        /// <summary>
        /// Redirect URL
        /// </summary>
        public string FormRedirectURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FormRedirectURL"), "");
            }
            set
            {
                SetValue("FormRedirectURL", value);
            }
        }

        #endregion "Redirection"

        #region "multipart form functionality"

        /// <summary>
        /// The current multipart step
        /// </summary>
        public int CurrentStep
        {
            get
            {
                int defStep = 0;
                int? sessionStep = GetInt(GetSession("CurrentStep"));
                int? qsStep = GetInt(QueryHelper.GetInteger("CurrentStep", 0));
                if (EnableMultiPartForm)
                {
                    defStep = sessionStep.GetValueOrDefault(1);
                }
                return ValidationHelper.GetInteger(GetValue("CurrentStep"), defStep);
                //return QueryHelper.GetInteger("CurrentStep", defStep);
            }
            set
            {
                SetValue("CurrentStep", value);
                SetSession("CurrentStep", value);
            }
        }

        /// <summary>
        /// The current form anme
        /// </summary>
        public string CurrentAlternateFormName
        {
            get
            {
                string ret = AlternativeFormName;
                if (EnableMultiPartForm)
                {
                    string currentStepString = CurrentStep.ToString();
                    if (!String.IsNullOrWhiteSpace(AlternativeFormNamePrefix))
                    {
                        ret = "BizForm." + BizFormName + "." + AlternativeFormNamePrefix + currentStepString;
                    }
                    else
                    {
                        ret = AlternativeFormName;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Emable multipart form functionality
        /// </summary>
        public bool EnableMultiPartForm
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("EnableMultiPartForm"), false);
            }
            set
            {
                SetValue("EnableMultiPartForm", value);
            }
        }

        /// <summary>
        /// Numbers of Parts
        /// TODO: if this is "0" or null try to detect parts from the list of alt form
        /// </summary>
        public int NumberOfMultiParts
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NumberOfMultiParts"), 0);
            }
            set
            {
                SetValue("NumberOfMultiParts", value);
            }
        }

        /// <summary>
        /// Multipart form name (without the number)
        /// </summary>
        public string AlternativeFormNamePrefix
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AlternativeFormNamePrefix"), AlternativeFormName);
            }
            set
            {
                SetValue("AlternativeFormNamePrefix", value);
            }
        }

        #endregion "multipart form functionality"

        #endregion "Properties"

        #region "Page Events"

        /// <summary>
        /// Load Event Handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            form.OnAfterSave += form_OnAfterSave;
            form.OnBeforeSave += form_OnBeforeSave;
            if (ItemID > 0)
            {
                form.ItemID = ItemID;
            }

            if (!String.IsNullOrWhiteSpace(FormRedirectURL))
            {
                form.FormRedirectToUrl = FormRedirectURL;
            }

            base.OnLoad(e);
            AddButtons();
        }

        /// <summary>
        /// Content loaded event handler.
        /// </summary>
        public override void OnContentLoaded()
        {
            base.OnContentLoaded();

            if (ItemID > 0)
            {
                form.ItemID = ItemID;
            }

            if (!String.IsNullOrWhiteSpace(FormRedirectURL))
            {
                form.FormRedirectToUrl = FormRedirectURL;
            }

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
            //if (!RequestHelper.IsPostBack() && !IsPostBack)
            //{
            //    AddActionMessage();
            //}

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

            //if (form.FieldEditingControls != null)
            //{
            //    //force site id to be current site id and disabled
            //    EditingFormControl ctrl = form.FieldEditingControls["siteid"] as EditingFormControl;
            //    if (ctrl != null)
            //    {
            //        // Replace value of NameOfField field with value of Column2
            //        ctrl.Value = SiteContext.CurrentSiteID;
            //        ctrl.Enabled = false;
            //    }
            //}
            //else
            //{
            //    lblMsg.Text = "You do not have permission to edit this item.";
            //}
        }

        #endregion "Page Events"

        #region "form binding"

        /// <summary>
        /// Initializes the control properties.
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
                //DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(BizFormName);

                //if (dataClassInfo == null)
                //{
                //    return;
                //}

                // Set BizForm properties
                form.FormName = BizFormName;
                form.SiteName = SiteName;
                form.UseColonBehindLabel = UseColonBehindLabel;
                //form.AlternativeFormFullName = AlternativeFormName;
                form.AlternativeFormFullName = CurrentAlternateFormName;
                form.ValidationErrorMessage = ValidationErrorMessage;
                form.DefaultFormLayout = DefaultFormLayout.ToEnum<FormLayoutEnum>();
                //form.OnAfterSave += form_OnAfterSave;

                if (ItemID > 0)
                {
                    form.ItemID = ItemID;
                }

                if (!String.IsNullOrWhiteSpace(FormRedirectURL))
                {
                    form.FormRedirectToUrl = FormRedirectURL;
                }

                AddButtons();

                // Set the live site context
                if (form != null)
                {
                    form.ControlContext.ContextName = CMS.Base.Web.UI.ControlContext.LIVE_SITE;
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

                if (ShowCancelButton)
                {
                    //Control container = null;

                    // find the submit button.
                    FormSubmitButton submitButton = form.FindControl(form.SubmitButton.ID) as FormSubmitButton;
                    //FormSubmitButton submitButton = form.SubmitButton;
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

                        //HtmlGenericControl test = new HtmlGenericControl("i");
                        //test.Attributes["class"] = "fas fa-check-circle";
                        //submitButton.Controls.AddAt(0, test);
                    }

                    //if (container != null)
                    //{
                    //    //((WebControl)container).CssClass = ButtonContainerCssClass;
                    //    container.Controls.Clear();
                    //    if (submitButton != null)
                    //    {
                    //        // Remove it from the original location.
                    //        int idx = -1;

                    //        if (submitButton.Parent != null)
                    //        {
                    //            idx = submitButton.Parent.Controls.IndexOf(submitButton);
                    //        }

                    //        if (idx >= 0)
                    //        {
                    //            submitButton.Parent.Controls.RemoveAt(submitButton.Parent.Controls.IndexOf(submitButton));
                    //        }

                    //        container.Controls.Add(submitButton);
                    //    }

                    //    if (hasCancelButton)
                    //    {
                    //        container.Controls.Add(new LiteralControl("&nbsp;"));
                    //        container.Controls.Add(cancelBtn);
                    //    }
                    //    else
                    //    {
                    //        if (cancelButton != null)
                    //        {
                    //            container.Controls.Add(new LiteralControl("&nbsp;"));
                    //            container.Controls.Add(cancelButton);
                    //        }
                    //    }
                    //}
                    //else
                    //{
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
                    //}
                }
            }
        }

        /// <summary>
        /// Add Previous, Next and Cancel buttons
        /// </summary>
        protected void AddButtons()
        {
            bool hasPreviousButton = false;
            bool hasCancelButton = false;

            LocalizedButton cancelButton = null;
            LocalizedButton prevButton = null;
            // get the form submit button
            FormSubmitButton submitButton = form.FindControl(form.SubmitButton.ID) as FormSubmitButton;

            // only proceed if we have the submit button (we'll only have this in the onload event)
            if (submitButton != null)
            {
                if (EnableMultiPartForm && HasPreviousButton && CurrentStep > 1)
                {
                    prevButton = form.FindControl(PREVIOUS_BUTTON_ID) as LocalizedButton;
                    hasPreviousButton = prevButton != null;
                    if (!hasPreviousButton)
                    {
                        int step = CurrentStep - 1;
                        string prevURL = URLHelper.AddParameterToUrl(RequestContext.CurrentURL.Replace("=Add", "=edit").Replace("=add", "=edit"), ItemKeyName, form.ItemID.ToString());
                        prevURL = URLHelper.AddParameterToUrl(prevURL, "CurrentStep", step.ToString());
                        prevButton = new LocalizedButton
                        {
                            ID = PREVIOUS_BUTTON_ID,
                            Text = !String.IsNullOrWhiteSpace(PreviousButtonText) ? PreviousButtonText : "Previous",
                            CssClass = !String.IsNullOrWhiteSpace(PreviousButtonCssClass) ? PreviousButtonCssClass : form.FormButtonCssClass,
                            CausesValidation = false,
                            EnableViewState = false,
                            OnClientClick = string.Format("document.location.href='{0}';return false;", prevURL)
                        };
                    }
                }

                if (ShowCancelButton)
                {
                    cancelButton = form.FindControl(CANCEL_BUTTON_ID) as LocalizedButton;
                    hasCancelButton = cancelButton != null;
                    // must have a cancel url to go back to
                    if (!hasCancelButton && !String.IsNullOrWhiteSpace(CancelURL))
                    {
                        cancelButton = new LocalizedButton
                        {
                            ID = CANCEL_BUTTON_ID,
                            Text = !String.IsNullOrWhiteSpace(CancelButtonText) ? CancelButtonText : ResHelper.GetString("general.cancel"),
                            CssClass = !String.IsNullOrWhiteSpace(CancelButtonCssClass) ? CancelButtonCssClass : form.FormButtonCssClass,
                            CausesValidation = false,
                            EnableViewState = false,
                            OnClientClick = string.Format("document.location.href='{0}';return false;", CancelURL)
                        };
                    }
                }

                if (EnableMultiPartForm && HasNextButton && CurrentStep < NumberOfMultiParts)
                {
                    // update the submit button the "next" button settings
                    if (!String.IsNullOrWhiteSpace(NextButtonText))
                    {
                        submitButton.ResourceString = String.Empty;
                        submitButton.Text = NextButtonText;
                    }

                    if (!String.IsNullOrWhiteSpace(NextButtonCssClass))
                    {
                        submitButton.CssClass = NextButtonCssClass;
                    }
                }
                else
                {
                    // update the submit button with the settings if applicable
                    if (!String.IsNullOrWhiteSpace(SubmitButtonText))
                    {
                        submitButton.ResourceString = String.Empty;
                        submitButton.Text = SubmitButtonText;
                    }

                    if (!String.IsNullOrWhiteSpace(SubmitButtonCssClass))
                    {
                        submitButton.CssClass = SubmitButtonCssClass;
                    }
                }

                Panel container = null;
                // rebuild the submit button container
                // Remove the submit button from orginal location.
                try
                {
                    container = form.FindControl(submitButton.Parent.ID) as Panel;
                }
                catch (Exception)
                {
                    if (container == null)
                    {
                        container = (Panel)submitButton.Parent;
                    }
                }

                if (container != null)
                {
                    container.Controls.Clear(); // clear the controls

                    if (cancelButton != null)
                    {
                        if (container.Controls.Count > 1)
                        {
                            container.Controls.Add(new LiteralControl("&nbsp;")); // add a space
                        }
                        container.Controls.Add(cancelButton);
                    }

                    if (prevButton != null)
                    {
                        if (container.Controls.Count > 1)
                        {
                            container.Controls.Add(new LiteralControl("&nbsp;")); // add a space
                        }
                        container.Controls.Add(prevButton);
                    }

                    if (submitButton != null)
                    {
                        if (container.Controls.Count > 1)
                        {
                            container.Controls.Add(new LiteralControl("&nbsp;")); // add a space
                        }
                        container.Controls.Add(submitButton);
                    }
                }
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
            //CheckPermissions();
        }

        /// <summary>
        /// OnAfterSave event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void form_OnAfterSave(object sender, EventArgs e)
        {
            var test = form.ItemID;
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
            }

            if (!String.IsNullOrWhiteSpace(FormRedirectURL) && !EnableMultiPartForm)
            {
                form.FormRedirectToUrl = form.ResolveMacros(FormRedirectURL);
            }

            if (EnableMultiPartForm && CurrentStep < NumberOfMultiParts)
            {
                CurrentStep = CurrentStep + 1;
                string redirectUrl = URLHelper.AddParameterToUrl(RequestContext.CurrentURL.Replace("=Add", "=edit").Replace("=add", "=edit"), ItemKeyName, form.ItemID.ToString());
                redirectUrl = URLHelper.AddParameterToUrl(redirectUrl, "CurrentStep", CurrentStep.ToString());
                URLHelper.Redirect(redirectUrl);
                // form.FormRedirectToUrl = "?ItemID=" + form.ItemID.ToString();
            }
            else if (!String.IsNullOrWhiteSpace(FormRedirectURL))
            {
                //redirect to the redirect location or stop.
                form.FormRedirectToUrl = form.ResolveMacros(FormRedirectURL);
            }
        }

        #endregion "Event Handlers"

        #region "Form Operations"

        /// <summary>
        /// Get the item id using keys
        /// </summary>
        /// <returns></returns>
        protected int GetItemKeyID()
        {
            return QueryHelper.GetInteger(ItemKeyName, QueryHelper.GetInteger("RecordKey", 0));
        }

        /// <summary>
        /// Get The Biz form Item
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        protected BizFormItem GetFormItem(out string p_message)
        {
            BizFormItem item = null;
            p_message = String.Empty;
            string formName = BizFormName; //Code name of custom table
            int key = GetItemKeyID();
            if (key > 0)
            {
                p_message = "&nbsp;KeyId=" + key.ToString();
                //Definition of custom table in this case
                DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(formName);
                if (dataClassInfo != null)
                {
                    //item = dataClassInfo.Get
                    item = BizFormItemProvider.GetItem(key, dataClassInfo.ClassName);
                }
                if (item == null)
                {
                    p_message += "(Item Not Found)";
                }
            }
            return item;
        }

        /// <summary>
        /// Get Value a biz form item column.
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected Object GetFormItemValue(BizFormItem p_item, string p_key)
        {
            Object ret = null;
            if (p_item != null)
            {
                ret = p_item.GetValue(p_key);
            }
            return ret;
        }

        /// <summary>
        /// Set value of a biz from item column
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_key"></param>
        /// <param name="p_value"></param>
        protected void SetFormItemValue(BizFormItem p_item, string p_key, Object p_value)
        {
            if (p_item != null)
            {
                p_item.SetValue(p_key, p_value);
            }
        }

        #endregion "Form Operations"

        #region "Helpers"

        #region "Session helper"

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
        /// Set Session data
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
        /// Clear session data
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

        #endregion "Session helper"

        #region "Properties Helper"

        /// <summary>
        /// Get Bool property values
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public bool GetBoolProperty(string p_key, bool p_default)
        {
            return GetSafeBool(ValidationHelper.GetBoolean(GetValue(p_key), p_default), p_default);
        }

        /// <summary>
        /// Get string properties
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public string GetStringProperty(string p_key, string p_default)
        {
            return GetSafeString(ValidationHelper.GetString(GetValue(p_key), p_default), p_default);
        }

        /// <summary>
        /// Get nullable boolean
        /// </summary>
        /// <param name="p_key"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public bool? GetNullableBoolProperty(string p_key, bool? p_default)
        {
            return GetBool(ValidationHelper.GetNullableBoolean(GetValue(p_key), p_default), p_default);
        }

        #endregion "Properties Helper"

        #region "Cache Helper"

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

        #endregion "Cache Helper"

        #region "wte data helper"

        #region "get values"

        /// <summary>
        /// Safe cast an object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetString(object p_obj)
        {
            return GetString(p_obj, null);
        }

        /// <summary>
        ///  Get String value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetString(object p_obj, string p_default)
        {
            string ret = null;

            if (p_obj != null)
            {
                if (p_obj is Enum)
                {
                    ret = ((int)(p_obj)).ToString();
                }
                else
                {
                    ret = p_obj.ToString();
                }
            }

            // clean it up
            if (String.IsNullOrWhiteSpace(ret))
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable bool
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj)
        {
            return GetBool(p_obj, null);
        }

        /// <summary>
        /// Get boolean value with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool? GetBool(object p_obj, bool? p_default)
        {
            bool? ret = null;
            string val = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(val))
            {
                // the setting could be bool (true/false) or (0/1)
                bool b = false;
                if (bool.TryParse(val, out b))
                {
                    ret = b;
                }
                else
                {
                    // failed, try parsing it as int
                    int num = 0;
                    if (int.TryParse(val, out num))
                    {
                        ret = (num != 0);
                    }
                }
            }
            if (ret == null)
            {
                ret = p_default;
            }
            return ret;
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj)
        {
            return GetInt(p_obj, null);
        }

        /// <summary>
        /// Safe cast an object to nullable int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int? GetInt(object p_obj, int? p_default)
        {
            int? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                int num = 0;
                if (int.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj)
        {
            return GetDecimal(p_obj, null);
        }

        /// <summary>
        /// Get decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(object p_obj, decimal? p_default)
        {
            decimal? ret = null;
            string svalue = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(svalue))
            {
                decimal num = 0;
                if (decimal.TryParse(svalue, out num))
                {
                    ret = num;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            return ret;
        }

        /// <summary>
        /// Get DateTime? value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj)
        {
            return GetDateTime(p_obj, null);
        }

        /// <summary>
        /// Get date time value
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object p_obj, DateTime? p_default)
        {
            DateTime? ret = null;
            string val = GetString(p_obj);

            if (!String.IsNullOrWhiteSpace(val))
            {
                // default date is 01/01/1776

                // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
                // however DATETIME2 class does not have this limitation.

                DateTime d = DateTime.Now;
                if (DateTime.TryParse(val, out d))
                {
                    ret = d;
                }
                else
                {
                    ret = null;
                }
            }

            if (ret == null)
            {
                ret = p_default;
            }

            //return null if no date
            return ret;
        }

        #endregion "get values"

        #region "safe values"

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj)
        {
            return GetSafeString(p_obj, String.Empty);
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static string GetSafeString(object p_obj, string p_default)
        {
            string output = p_default;
            string value = GetString(p_obj, p_default);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value.Trim();
            }
            return output;
        }

        /// <summary>
        /// Get string value or null
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static string GetStringValueOrNull(object p_obj)
        {
            string output = null;
            string value = GetString(p_obj);
            if (!String.IsNullOrWhiteSpace(value))
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to boolean
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj)
        {
            return GetSafeBool(p_obj, false);
        }

        /// <summary>
        /// Get safe bool with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static bool GetSafeBool(object p_obj, bool p_default)
        {
            bool ret = p_default;
            bool? value = GetBool(p_obj, p_default);
            if (value.HasValue)
            {
                ret = value.Value;
            }
            return ret;
        }

        /// <summary>
        /// Get decimal or null (if value is false)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static bool? GetBoolValueOrNull(object p_obj)
        {
            bool? ret = null;
            bool value = GetSafeBool(p_obj);
            if (value)
            {
                ret = value;
            }
            return ret;
        }

        /// <summary>
        /// Convert object to int
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj)
        {
            return GetSafeInt(p_obj, 0);
        }

        /// <summary>
        /// Get safe int with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static int GetSafeInt(object p_obj, int p_default)
        {
            int output = p_default;
            int? value = GetInt(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get int or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns>null if failed to parse or if value is 0</returns>
        public static int? GetIntValueOrNull(object p_obj)
        {
            int? output = null;
            int value = GetSafeInt(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to decimal
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj)
        {
            return GetSafeDecimal(p_obj, 0);
        }

        /// <summary>
        /// Get safe decimal with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static decimal GetSafeDecimal(object p_obj, decimal p_default)
        {
            decimal output = p_default;
            decimal? value = GetDecimal(p_obj, p_default);
            if (value.HasValue)
            {
                output = value.Value;
            }
            return output;
        }

        /// <summary>
        /// Get decimal or null (if value is 0)
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static decimal? GetDecimalValueOrNull(object p_obj)
        {
            decimal? output = null;
            decimal value = GetSafeDecimal(p_obj);
            if (value > 0)
            {
                output = value;
            }
            return output;
        }

        /// <summary>
        /// Convert object to date time
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj)
        {
            return GetSafeDateTime(p_obj, DateTime.Now);
        }

        /// <summary>
        /// Get safe date time with default
        /// </summary>
        /// <param name="p_obj"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static DateTime GetSafeDateTime(object p_obj, DateTime p_default)
        {
            DateTime ret = p_default;
            DateTime? value = GetDateTimeValueOrNull(p_obj);
            if (value.HasValue)
            {
                ret = value.Value;
            }

            // the minimum date for MS SQL server is January 1, 1753 for the DATETIME class
            // however DATETIME2 class does not have this limitation.

            // clean up the value so that it is ms sql safe.
            if (ret.Year < 1900)
            {
                ret.AddYears(1900 - ret.Year);
            }

            return ret;
        }

        /// <summary>
        /// Get date time value or null!
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static DateTime? GetDateTimeValueOrNull(object p_obj)
        {
            DateTime? ret = GetDateTime(p_obj, null);
            return ret;
        }

        #endregion "safe values"

        #region "math"

        /// <summary>
        /// Round a decimal
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static decimal Round(decimal p_number, int p_decimalPoints)
        {
            return Convert.ToDecimal(Round(Convert.ToDouble(p_number), p_decimalPoints));
        }

        /// <summary>
        /// Round a double
        /// </summary>
        /// <param name="p_number"></param>
        /// <param name="p_decimalPoints"></param>
        /// <returns></returns>
        public static double Round(double p_number, int p_decimalPoints)
        {
            double decimalPowerOfTen = Math.Pow(10, p_decimalPoints);
            return Math.Floor(p_number * decimalPowerOfTen + 0.5) / decimalPowerOfTen;
        }

        #endregion "math"

        #region "utilities"

        /// <summary>
        /// Get bitwise int from an object
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static int GetBitWiseInt(Object p_valueSet)
        {
            int val = 0;
            HashSet<int> valueSet = GetIntHashSet(p_valueSet);
            foreach (int v in valueSet)
            {
                val += v;
            }
            return val;
        }

        /// <summary>
        /// Get hashset from a comma delimited string
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static HashSet<int> GetIntHashSet(Object p_valueSet)
        {
            HashSet<int> ret = new HashSet<int>();
            if (p_valueSet.GetType() == typeof(HashSet<int>))
            {
                ret = (HashSet<int>)p_valueSet;
            }
            else
            {
                List<string> ids = GetKeywordList(p_valueSet);
                foreach (string id in ids)
                {
                    ret.Add(GetSafeInt(ids));
                }
            }
            return ret;
        }

        /// <summary>
        /// Get int list
        /// </summary>
        /// <param name="p_valueSet"></param>
        /// <returns></returns>
        public static List<int> GetIntList(Object p_valueSet)
        {
            List<int> ret = new List<int>();

            if (p_valueSet != null)
            {
                if (p_valueSet.GetType() == typeof(List<int>))
                {
                    ret = (List<int>)p_valueSet;
                }
                else
                {
                    List<string> ids = GetKeywordList(p_valueSet);
                    foreach (string id in ids)
                    {
                        ret.Add(GetSafeInt(id));
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Check ID to see if it's an int.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static bool ValidateIdString(Object p_keywordList)
        {
            bool valid = true;
            List<string> ids = GetKeywordList(p_keywordList);
            foreach (string id in ids)
            {
                int val = 0;
                if (!int.TryParse(id, out val))
                {
                    valid = false;
                    break;
                }
            }
            return valid;
        }

        /// <summary>
        /// Get display keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetDisplayKeywordString(Object p_keywordList)
        {
            return GetKeywordString(p_keywordList, ",", false, true);
        }

        /// <summary>
        /// Get keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList)
        {
            return GetKeywordString(p_keywordList, ",", false, false);
        }

        /// <summary>
        /// Get keyword string
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <param name="p_delimiter"></param>
        /// <param name="p_addQuotation"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation)
        {
            return GetKeywordString(p_keywordList, p_delimiter, p_addQuotation, false);
        }

        /// <summary>
        /// format keyword string to a proper string with delimiter
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <param name="p_delimiter"></param>
        /// <param name="p_addQuotation"></param>
        /// <param name="p_addSpace"></param>
        /// <returns></returns>
        public static string GetKeywordString(Object p_keywordList, string p_delimiter, bool p_addQuotation, bool p_addSpace)
        {
            string ret = String.Empty;
            string delimiter = ",";
            if (p_keywordList != null)
            {
                if (!String.IsNullOrWhiteSpace(p_delimiter))
                {
                    delimiter = p_delimiter.Trim();
                }

                List<string> keywords = GetKeywordList(p_keywordList);
                foreach (string keyword in keywords)
                {
                    if (!String.IsNullOrWhiteSpace(keyword))
                    {
                        if (!String.IsNullOrWhiteSpace(ret))
                        {
                            ret += delimiter;
                            if (p_addSpace)
                            {
                                ret += " ";
                            }
                        }
                        if (p_addQuotation)
                        {
                            ret += String.Format("'{0}'", keyword.Trim());
                        }
                        else
                        {
                            ret += keyword.Trim();
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Create keyword list from 2 objects
        /// </summary>
        /// <param name="p_list1"></param>
        /// <param name="p_list2"></param>
        /// <returns></returns>
        public static List<string> JoinKeyWordList(Object p_list1, Object p_list2)
        {
            return JoinKeyWordList(p_list1, p_list2, false);
        }

        /// <summary>
        /// Create keyword list from 2 objects
        /// </summary>
        /// <param name="p_list1"></param>
        /// <param name="p_list2"></param>
        /// <param name="p_filterZero"></param>
        /// <returns></returns>
        public static List<string> JoinKeyWordList(Object p_list1, Object p_list2, bool p_filterZero)
        {
            List<string> ret = new List<string>();
            List<string> list1 = GetKeywordList(p_list1);
            List<string> list2 = GetKeywordList(p_list2);

            if (list1.Count > 0)
            {
                foreach (string item in list1)
                {
                    if (!p_filterZero || item != "0")
                    {
                        if (!ret.Contains(item))
                        {
                            ret.Add(item);
                        }
                    }
                }
            }

            if (list2.Count > 0)
            {
                foreach (string item in list2)
                {
                    if (!p_filterZero || item != "0")
                    {
                        if (!ret.Contains(item))
                        {
                            ret.Add(item);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Get a list of string from a string value.
        /// </summary>
        /// <param name="p_keywordList"></param>
        /// <returns></returns>
        public static List<string> GetKeywordList(Object p_keywordList)
        {
            List<string> keywordList = new List<string>();
            if (p_keywordList != null)
            {
                if (p_keywordList.GetType() == typeof(List<string>))
                {
                    // return the keyword list
                    keywordList = (List<string>)p_keywordList;
                }
                else if (p_keywordList.GetType() == typeof(HashSet<int>))
                {
                    // add each item to the collection
                    foreach (int keyword in (HashSet<int>)p_keywordList)
                    {
                        string valueString = GetSafeString(keyword);
                        if (!keywordList.Contains(valueString))
                        {
                            keywordList.Add(valueString);
                        }
                    }
                }
                else
                {
                    #region any other type, convert to string parse and add each item to the list

                    string tempValue = GetSafeString(p_keywordList);
                    if (!String.IsNullOrWhiteSpace(tempValue))
                    {
                        tempValue = tempValue.Replace("\\", "|").Replace("/", "|").Replace(" ", "|").Replace(",", "|").Trim();
                        string[] tempKeywords = tempValue.Split('|');
                        foreach (string keyword in tempKeywords)
                        {
                            if (!String.IsNullOrWhiteSpace(keyword))
                            {
                                if (!keywordList.Contains(keyword.Trim()))
                                {
                                    keywordList.Add(keyword.Trim());
                                }
                            }
                        }
                    }

                    #endregion any other type, convert to string parse and add each item to the list
                }
            }
            return keywordList;
        }

        /// <summary>
        /// Check to see if the an object is nullable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_object"></param>
        /// <returns></returns>
        public static bool IsNullable<T>(T p_object)
        {
            if (!typeof(T).IsGenericType)
                return false;
            return typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Get value with default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_value"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public static T GetDefaultValue<T>(T p_value, T p_default)
        {
            T ret = p_value;

            if (p_value == null)
            {
                // null just set to default
                ret = p_default;
            }
            else
            {
                Type t = p_value.GetType();
                if ((t == typeof(String)) || (t == typeof(string)))
                {
                    if (String.IsNullOrWhiteSpace(p_value.ToString()))
                    {
                        ret = p_default;
                    }
                    else
                    {
                        ret = p_value;
                    }
                }
                else
                {
                    ret = p_value;
                }
            }

            return ret;
        }

        #endregion "utilities"

        #endregion "wte data helper"

        #region "Post data"

        /// <summary>
        /// Get JSON data from POST
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_default"></param>
        /// <returns></returns>
        protected T GetJSONPostedData<T>(T p_default)
        {
            T ret = p_default;
            Stream str = Request.InputStream;
            if (str != null)
            {
                int length = (int)str.Length;
                byte[] data = new byte[length];
                int count = str.Read(data, 0, length);
                string jsonString = ASCIIEncoding.ASCII.GetString(data);
                string cleanJSON = jsonString.Replace("\\\"", "\"");
                string decodedHtml = HttpUtility.HtmlDecode(cleanJSON);

                //var t1 = DeserializeJSON(jsonString, typeof(object));
                //var t2 = DeserializeJSON(cleanJSON, typeof(object));
                //var t3 = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString);
                //var t4 = JsonConvert.DeserializeObject<UserManageData>(jsonString);

                try
                {
                    ret = JsonConvert.DeserializeObject<T>(jsonString);
                }
                catch (Exception)
                {
                    ret = p_default;
                }
                System.Diagnostics.Debug.WriteLine(jsonString);
            }
            return ret;
        }

        /// <summary>
        /// Deserialized JSON into an Object
        /// </summary>
        /// <param name="p_obj"></param>
        /// <returns></returns>
        public static object DeserializeJSON(string p_jsonString, Type p_objectType)
        {
            Object obj = null;
            if (!String.IsNullOrWhiteSpace(p_jsonString))
            {
                try
                {
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(p_jsonString));
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(p_objectType);
                    obj = ser.ReadObject(ms);
                    ms.Close();
                }
                catch (Exception ex)
                {
                    // ignore it (the object can't be serialized?)
                    //boom!
                    //int x = 1;
                    p_jsonString = ex.Message;
                    throw (ex);
                }
            }
            return obj;
        }

        /// <summary>
        /// Post Data and redirects
        /// </summary>
        /// <param name="p_page"></param>
        /// <param name="p_token"></param>
        /// <param name="p_postUrl"></param>
        public static void RedirectAndPOSTData(Page p_page, string p_token, string p_postUrl)
        {
            try
            {
                NameValueCollection data = new NameValueCollection();
                data.Add("token", p_token);

                //Prepare the Posting form
                string strForm = PreparePOSTForm(p_postUrl, data);

                //Add a literal control the specified page holding the Post Form, this is to submit the Posting form with the request.
                p_page.Controls.Add(new LiteralControl(strForm));
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Get Form with data
        /// </summary>
        /// <param name="p_postUrl"></param>
        /// <param name="p_postData"></param>
        /// <returns></returns>
        private static String PreparePOSTForm(string p_postUrl, NameValueCollection p_postData)
        {
            //Set a name for the form
            string formID = "PostForm";

            //  string formTemplate = @"<form id ="formID" name="formID" action="url" method="post" target="target">
            //  <input type="hidden" name="key" value="data[key"] />
            //</form>"

            //Build the form using the specified data to be posted.
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + p_postUrl + "\" method=\"POST\" target=\"load_payment\">");
            foreach (string key in p_postData)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key + "\" value=\"" + p_postData[key] + "\">");
            }
            strForm.Append("</form>");

            //Build the JavaScript which will do the Posting operation.
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");

            //Return the form and the script concatenated. (The order is important, Form then JavaScript)
            return strForm.ToString() + strScript.ToString();
        }

        #endregion "Post data"

        #endregion "Helpers"
    }
}