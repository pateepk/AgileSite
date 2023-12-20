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

namespace CMSApp.CMSWebParts.WTE.TrainingNetwork
{
    /// <summary>
    /// Web part for managing a TN Catalog and products (multiple row selection support)
    /// </summary>
    public partial class TN_CatalogManage : CMSAbstractWebPart
    {
        #region "Custom Classes"

        /// <summary>
        /// Data for assigning catalog to clients
        /// </summary>
        public class CompanyAssignmentData
        {
            public string AssignmentID { get; set; }

            public string CompanyID { get; set; }

            public string CatalogID { get; set; }

            public string CategoryID { get; set; }

            public string VideoID { get; set; }
        }

        /// <summary>
        /// Data for assigning products or category to catalog
        /// </summary>
        public class ProductAssignmentData
        {
            public string AssignmentID { get; set; }

            public string CatalogID { get; set; }

            public string CategoryID { get; set; }

            public string VideoID { get; set; }
        }

        /// <summary>
        /// Catalog management data
        /// </summary>
        public class CatalogManageData
        {
            /// <summary>
            /// The action.
            /// </summary>
            public string Action { get; set; }

            public List<string> CatalogID { get; set; }

            public List<ProductAssignmentData> ProductAssignmentData { get; set; }

            public List<CompanyAssignmentData> CompanyAssignmentData { get; set; }
        }

        #endregion "Custom Classes"

        #region "Properties"

        #region "Webpart Specific Options"

        /// <summary>
        /// Is debug mode enabled
        /// </summary>
        public bool IsDebugMode
        {
            get
            {
                return GetBoolProperty("IsDebugMode", false);
            }
            set
            {
                this.SetValue("IsDebugMode", value);
            }
        }

        /// <summary>
        /// Show the built in GUI
        /// </summary>
        public bool ShowGUI
        {
            get
            {
                return GetBoolProperty("ShowGUI", false);
            }
            set
            {
                this.SetValue("ShowGUI", value);
            }
        }

        /// <summary>
        /// Mode of operation
        /// "manage" = enable/disable or delete
        /// "assignproduct" = assign product or category
        /// "assigncompany" = assign company
        /// </summary>
        public string ManageMode
        {
            get
            {
                return GetStringProperty("ManageMode", "");
            }
            set
            {
                this.SetValue("ManageMode", value);
            }
        }

        #endregion "Webpart Specific Options"

        #region "Redirection"

        /// <summary>
        /// Redirect URL after successful deleting
        /// </summary>
        public string DeleteRedirectURL
        {
            get
            {
                return GetStringProperty("DeleteRedirectURL", "");
            }
            set
            {
                this.SetValue("DeleteRedirectURL", value);
            }
        }

        /// <summary>
        /// Redirect URL after successful copy
        /// </summary>
        public string CopyRedirectURL
        {
            get
            {
                return GetStringProperty("CopyRedirectURL", "");
            }
            set
            {
                this.SetValue("CopyRedirectURL", value);
            }
        }

        /// <summary>
        /// Get/Set base redirect url
        /// </summary>
        public string RedirectURL
        {
            get
            {
                return GetStringProperty("RedirectURL", "");
            }
            set
            {
                this.SetValue("RedirectURL", value);
            }
        }

        #endregion "Redirection"

        #region "Message"

        /// <summary>
        /// Copy Success Message
        /// </summary>
        public string CopySuccessMessage
        {
            get
            {
                return GetStringProperty("CopySuccessMessage", String.Empty);
            }
            set
            {
                this.SetValue("CopySuccessMessage", value);
            }
        }

        /// <summary>
        /// Copy Failed Message
        /// </summary>
        public string CopyFailedMessage
        {
            get
            {
                return GetStringProperty("CopyFailedMessage", String.Empty);
            }
            set
            {
                this.SetValue("CopyFailedMessage", value);
            }
        }

        /// <summary>
        /// Delete success message
        /// </summary>
        public string DeleteSuccessMessage
        {
            get
            {
                return GetStringProperty("DeleteSuccessMessage", String.Empty);
            }
            set
            {
                this.SetValue("DeleteSuccessMessage", value);
            }
        }

        /// <summary>
        /// Delete fail message
        /// </summary>
        public string DeleteFailedMessage
        {
            get
            {
                return GetStringProperty("DeleteFailedMessage", String.Empty);
            }
            set
            {
                this.SetValue("DeleteFailedMessage", value);
            }
        }

        #endregion "Message"

        #region "Session Data"

        /// <summary>
        /// Get/Set the message to show when page load.
        /// </summary>
        public string SessionMessage
        {
            get
            {
                string ret = GetSession("TNCustomTableManagerSessionMessage");

                if (String.IsNullOrWhiteSpace(ret))
                {
                    if (String.IsNullOrWhiteSpace(ret))
                    {
                        ret = GetStringProperty("TNCustomTableManagerSessionMessage", "");
                    }
                }

                return ret;
            }
            set
            {
                this.SetValue("TNCustomTableManagerSessionMessage", value);
                SetSession("TNCustomTableManagerSessionMessage", value);
            }
        }

        /// <summary>
        /// do we have an error
        /// </summary>
        public bool? IsSessionError
        {
            get
            {
                bool? ret = null;
                string retString = GetSession("TNCatalogManagerIsSessionError");

                if (String.IsNullOrWhiteSpace(retString))
                {
                    if (String.IsNullOrWhiteSpace(retString))
                    {
                        ret = GetNullableBoolProperty("TNCatalogManagerIsSessionError", null);
                    }
                }
                else
                {
                    ret = GetNullableBoolProperty(retString, null);
                }

                return ret;
            }
            set
            {
                this.SetValue("TNCatalogManagerIsSessionError", value);
                SetSession("TNCatalogManagerIsSessionError", value);
            }
        }

        #endregion "Session Data"

        #region "Custom Table Information"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string CustomTable
        {
            get
            {
                return GetStringProperty("CustomTable", "");
            }
            set
            {
                this.SetValue("CustomTable", value);
            }
        }

        /// <summary>
        /// Key name used to identify edited object.
        /// </summary>
        public string ItemKeyName
        {
            get
            {
                return GetStringProperty("ItemKeyName", "edititemid");
            }
            set
            {
                SetValue("ItemKeyName", value);
            }
        }

        /// <summary>
        /// Column name for the "active column"
        /// </summary>
        public string ActiveColumnKey
        {
            get
            {
                return GetStringProperty("ActiveColumnKey", "IsActive");
            }
            set
            {
                this.SetValue("ActiveColumnKey", value);
            }
        }

        /// <summary>
        /// Column name of the ID field
        /// </summary>
        public string IDColumnKey
        {
            get
            {
                return GetStringProperty("IDColumnKey", "ItemID");
            }
            set
            {
                this.SetValue("IDColumnKey", value);
            }
        }

        #endregion "Custom Table Information"

        #region "Catalog Specific Information"

        /// <summary>
        /// Catalog ID column key
        /// </summary>
        public string CatalogIDColumnKey
        {
            get
            {
                return GetStringProperty("CatalogIDColumnKey", "CatalogID");
            }
            set
            {
                this.SetValue("CatalogIDColumnKey", value);
            }
        }

        /// <summary>
        /// Category ID column key
        /// </summary>
        public string CategoryIDColumnKey
        {
            get
            {
                return GetStringProperty("CategoryIDColumnKey", "NodeID");
            }
            set
            {
                this.SetValue("CategoryIDColumnKey", value);
            }
        }

        /// <summary>
        /// Video ID column key
        /// </summary>
        public string VideoIDColumnKey
        {
            get
            {
                return GetStringProperty("VideoIDColumnKey", "VideoID");
            }
            set
            {
                this.SetValue("VideoIDColumnKey", value);
            }
        }

        /// <summary>
        /// Company ID column key
        /// </summary>
        public string CompanyIDColumnKey
        {
            get
            {
                return GetStringProperty("CompanyIDColumnKey", "CompanyID");
            }
            set
            {
                this.SetValue("CompanyIDColumnKey", value);
            }
        }

        #endregion "Catalog Specific Information"

        #endregion "Properties"

        #region "Page Events"

        /// <summary>
        /// The page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsVisible)
            {
                // set everything to hide
                divManageAction.Visible = false;
                divCompanyAssignmentAction.Visible = false;
                divProductAssigmentAction.Visible = false;
                divtest.Visible = false;

                if (ShowGUI)
                {
                    switch (ManageMode)
                    {
                        case "manage":
                            {
                                divManageAction.Visible = true;
                            }
                            break;

                        case "assignproduct":
                            {
                                divProductAssigmentAction.Visible = true;
                            }
                            break;

                        case "assigncompany":
                            {
                                divCompanyAssignmentAction.Visible = true;
                            }
                            break;

                        default:
                            {
                                if (IsDebugMode)
                                {
                                    //divManageAction.Visible = true;
                                    //divProductAssigmentAction.Visible = true;
                                    //divCompanyAssignmentAction.Visible = true;
                                }
                            }
                            break;
                    }

                    divtest.Visible = IsDebugMode;
                }

                if (!RequestHelper.IsPostBack() && !Page.IsPostBack)
                {
                    // do not show messaging for now. (does this work?)
                    ShowSessionMessage(true);
                }
                else
                {
                    // process the action through "POST".
                    // This is actually not the right way to do it, we should build this out as a API.
                    ProcessAction();
                }
            }
        }

        #endregion "Page Events"

        #region "Methods"

        #region "Processing"

        /// <summary>
        /// Process action
        /// </summary>
        protected bool ProcessAction()
        {
            bool success = true;
            switch (ManageMode)
            {
                case "assignproduct":
                    {
                        success = ProcessAssignProduct();
                    }
                    break;

                case "assigncompany":
                    {
                        success = ProcessAssignCompany();
                    }
                    break;

                case "manage":
                case "managecatalog":
                case "manageuser":
                    {
                        success = ProcessManageCatalog();
                    }
                    break;

                default:
                    {
                        // no operation
                    }
                    break;
            }
            return success;
        }

        /// <summary>
        /// Process Catalog Actions (disable, enable, delete, copy)
        /// </summary>
        /// <returns></returns>
        protected bool ProcessManageCatalog()
        {
            bool success = false;
            string redirectURL = String.Empty;
            string message = String.Empty;
            string action = QueryHelper.GetString("action", "").ToLower();
            string debugMessage = "URL:" + RedirectURL;

            CatalogManageData data = GetJSONPostedData<CatalogManageData>(null);
            if (data != null)
            {
                action = data.Action;
                try
                {
                    // get the list here.
                    string tempMessage = String.Empty;
                    List<string> itemids = data.CatalogID;
                    foreach (string id in itemids)
                    {
                        CustomTableItem item = GetCustomTableItem(id, out tempMessage);
                        debugMessage += tempMessage;

                        if (CanManageCustomItem(item, out tempMessage))
                        {
                            switch (action)
                            {
                                case "delete":
                                    item = DeleteItem(item, out success, out tempMessage);
                                    if (success && item != null)
                                    {
                                        if (!String.IsNullOrWhiteSpace(DeleteRedirectURL))
                                        {
                                            redirectURL = DeleteRedirectURL;
                                        }
                                    }
                                    break;

                                case "copy":
                                    item = CopyItem(item, out success, out tempMessage);
                                    if (success && item != null)
                                    {
                                        if (!String.IsNullOrWhiteSpace(CopyRedirectURL))
                                        {
                                            redirectURL = CopyRedirectURL + "?ItemID=" + item.ItemID.ToString() + "&Action=Copy";
                                        }
                                    }
                                    break;

                                case "enable":
                                    item = EnableItem(item, true, out success, out tempMessage);
                                    if (success && item != null)
                                    {
                                        if (!String.IsNullOrWhiteSpace(RedirectURL))
                                        {
                                            redirectURL = RedirectURL;
                                        }
                                    }
                                    break;

                                case "disable":
                                    item = EnableItem(item, false, out success, out tempMessage);
                                    if (success && item != null)
                                    {
                                        if (!String.IsNullOrWhiteSpace(RedirectURL))
                                        {
                                            redirectURL = RedirectURL;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }

                        if (!String.IsNullOrWhiteSpace(tempMessage))
                        {
                            message += tempMessage;
                        }
                        else
                        {
                            debugMessage += tempMessage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message += "Error: " + ex.Message;
                    success = false;
                }

                // show any message or do redirection here?
                SetSessionMessage(message, success);
                ShowSessionMessage(false);

                if (success)
                {
                    if (String.IsNullOrWhiteSpace(redirectURL))
                    {
                        redirectURL = RequestContext.RawURL;
                    }
                    string test = RequestContext.URL.ToString();
                    string param = RequestContext.CurrentQueryString;
                    //URLHelper.Redirect(URLHelper.AddParameterToUrl(RequestContext.CurrentURL, ItemKeyName, form.ItemID.ToString()));
                    //URLHelper.Redirect(redirectURL);
                    //Response.Redirect(test, true);
                }
            }

            return success;
        }

        /// <summary>
        /// Process Product/Category assignment
        /// </summary>
        protected bool ProcessAssignProduct()
        {
            bool success = false;
            string redirectURL = String.Empty;
            string message = String.Empty;
            string action = QueryHelper.GetString("action", "").ToLower();
            string debugMessage = "URL:" + RedirectURL;

            CatalogManageData data = GetJSONPostedData<CatalogManageData>(null);
            if (data != null)
            {
                action = data.Action;
                try
                {
                    UserInfo authUser = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);

                    // get the list here.
                    string tempMessage = String.Empty;
                    List<ProductAssignmentData> assignmentData = data.ProductAssignmentData;

                    if (assignmentData != null && assignmentData.Count > 0)
                    {
                        foreach (ProductAssignmentData adata in assignmentData)
                        {
                            debugMessage += tempMessage;

                            switch (action)
                            {
                                case "assign":
                                    success = AssignedProduct(adata, out tempMessage);
                                    break;

                                case "unassign":
                                    success = UnAssignedProduct(adata, out tempMessage);
                                    break;

                                default:
                                    // do nothing not supported
                                    break;
                            }

                            if (!String.IsNullOrWhiteSpace(tempMessage))
                            {
                                message += tempMessage;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message += "Error: " + ex.Message;
                    success = false;
                }

                // show any message or do redirection here?
                SetSessionMessage(message, success);
                ShowSessionMessage(false);

                if (success)
                {
                    if (String.IsNullOrWhiteSpace(redirectURL))
                    {
                        redirectURL = RequestContext.RawURL;
                    }
                    string test = RequestContext.URL.ToString();
                    string param = RequestContext.CurrentQueryString;
                    //URLHelper.Redirect(URLHelper.AddParameterToUrl(RequestContext.CurrentURL, ItemKeyName, form.ItemID.ToString()));
                    //URLHelper.Redirect(redirectURL);
                    //Response.Redirect(test, true);
                }
            }
            return success;
        }

        /// <summary>
        /// Process company assignment
        /// </summary>
        protected bool ProcessAssignCompany()
        {
            bool success = false;
            string redirectURL = String.Empty;
            string message = String.Empty;
            string action = QueryHelper.GetString("action", "").ToLower();
            string debugMessage = "URL:" + RedirectURL;
            CatalogManageData data = GetJSONPostedData<CatalogManageData>(null);
            if (data != null)
            {
                action = data.Action;
                try
                {
                    UserInfo authUser = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);

                    // get the list here.
                    string tempMessage = String.Empty;
                    List<CompanyAssignmentData> assignmentData = data.CompanyAssignmentData;

                    if (assignmentData != null && assignmentData.Count > 0)
                    {
                        foreach (CompanyAssignmentData adata in assignmentData)
                        {
                            debugMessage += tempMessage;

                            switch (action)
                            {
                                case "assign":
                                    success = AssignedCompany(adata, out tempMessage);
                                    break;

                                case "unassign":
                                    success = UnAssignedCompany(adata, out tempMessage);
                                    break;

                                default:
                                    // do nothing not supported
                                    break;
                            }

                            if (!String.IsNullOrWhiteSpace(tempMessage))
                            {
                                message += tempMessage;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    message += "Error: " + ex.Message;
                    success = false;
                }

                // show any message or do redirection here?
                SetSessionMessage(message, success);
                ShowSessionMessage(false);

                if (success)
                {
                    if (String.IsNullOrWhiteSpace(redirectURL))
                    {
                        redirectURL = RequestContext.RawURL;
                    }
                    string test = RequestContext.URL.ToString();
                    string param = RequestContext.CurrentQueryString;
                    //URLHelper.Redirect(URLHelper.AddParameterToUrl(RequestContext.CurrentURL, ItemKeyName, form.ItemID.ToString()));
                    //URLHelper.Redirect(redirectURL);
                    //Response.Redirect(test, true);
                }
            }
            return success;
        }

        #region "Catalog Operations"

        /// <summary>
        /// Assign product or category to a catalog
        /// ** this create a new record on the table
        /// </summary>
        /// <param name="p_data"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public bool AssignedProduct(ProductAssignmentData p_data, out string p_message)
        {
            bool ret = false;

            CustomTableItem item = GetCustomTableItem(p_data.AssignmentID, out p_message);
            if (item != null)
            {
                SetCustomTableItemValue(item, CatalogIDColumnKey, GetInt(p_data.CatalogID));
                SetCustomTableItemValue(item, CategoryIDColumnKey, GetInt(p_data.CategoryID));
                SetCustomTableItemValue(item, VideoIDColumnKey, GetInt(p_data.VideoID));
                SetCustomTableItemValue(item, ActiveColumnKey, true);
                item.Update();
            }
            else
            {
                item = CreateCustomTableItem(out p_message);
                SetCustomTableItemValue(item, CatalogIDColumnKey, GetInt(p_data.CatalogID));
                SetCustomTableItemValue(item, CategoryIDColumnKey, GetInt(p_data.CategoryID));
                SetCustomTableItemValue(item, VideoIDColumnKey, GetInt(p_data.VideoID));
                SetCustomTableItemValue(item, ActiveColumnKey, true);
                item.Insert();
            }

            return ret;
        }

        /// <summary>
        /// Remove product and category assignment from a catalog
        /// </summary>
        /// <param name="p_data"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public bool UnAssignedProduct(ProductAssignmentData p_data, out string p_message)
        {
            bool ret = false;
            p_message = String.Empty;
            CustomTableItem item = GetCustomTableItem(p_data.AssignmentID, out p_message);
            if (item != null)
            {
                // go ahead and delete for now
                item = DeleteItem(item, out ret, out p_message);
            }
            return ret;
        }

        /// <summary>
        /// Assign catalog, products, or category to a company
        /// ** this create a new record on the table
        /// </summary>
        /// <param name="p_data"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public bool AssignedCompany(CompanyAssignmentData p_data, out string p_message)
        {
            bool ret = false;
            CustomTableItem item = GetCustomTableItem(p_data.AssignmentID, out p_message);
            if (item != null)
            {
                SetCustomTableItemValue(item, CompanyIDColumnKey, GetInt(p_data.CompanyID));
                SetCustomTableItemValue(item, CatalogIDColumnKey, GetInt(p_data.CatalogID));
                SetCustomTableItemValue(item, CategoryIDColumnKey, GetInt(p_data.CategoryID));
                SetCustomTableItemValue(item, VideoIDColumnKey, GetInt(p_data.VideoID));
                SetCustomTableItemValue(item, ActiveColumnKey, true);
                item.Update();
            }
            else
            {
                item = CreateCustomTableItem(out p_message);
                SetCustomTableItemValue(item, CompanyIDColumnKey, GetInt(p_data.CompanyID));
                SetCustomTableItemValue(item, CatalogIDColumnKey, GetInt(p_data.CatalogID));
                SetCustomTableItemValue(item, CategoryIDColumnKey, GetInt(p_data.CategoryID));
                SetCustomTableItemValue(item, VideoIDColumnKey, GetInt(p_data.VideoID));
                SetCustomTableItemValue(item, ActiveColumnKey, true);
                item.Insert();
            }
            return ret;
        }

        /// <summary>
        /// Remove or disable company assignment
        /// </summary>
        /// <param name="p_data"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public bool UnAssignedCompany(CompanyAssignmentData p_data, out string p_message)
        {
            bool ret = false;
            p_message = String.Empty;
            CustomTableItem item = GetCustomTableItem(p_data.AssignmentID, out p_message);
            if (item != null)
            {
                // go ahead and delete for now
                item = DeleteItem(item, out ret, out p_message);
            }
            return ret;
        }

        #endregion "Catalog Operations"

        #region "Custom Table Operations"

        /// <summary>
        /// Check to see if we can manage the item
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected bool CanManageCustomItem(CustomTableItem p_item, out string p_message)
        {
            bool ret = false;
            p_message = String.Empty; // future
            UserInfo authUser = UserInfoProvider.GetUserInfo(MembershipContext.AuthenticatedUser.UserID);
            if (p_item != null)
            {
                // this is always true for now.
                UserPrivilegeLevelEnum n = UserPrivilegeLevelEnum.GlobalAdmin;
                //if (!p_item.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin) && !p_item.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) &&
                //!p_item.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor) && MembershipContext.AuthenticatedUser.UserID != Convert.ToInt32(p_item.UserID))
                {
                    ret = true;
                }
            }
            return ret;
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
        /// Delete the specified item
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_success"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected CustomTableItem DeleteItem(CustomTableItem p_item, out bool p_success, out string p_message)
        {
            p_success = false;
            p_message = String.Empty;

            if (p_item != null)
            {
                try
                {
                    p_item.Delete();
                    p_message = "Record has been deleted.<br />";
                    p_success = true;
                }
                catch (Exception ex)
                {
                    p_success = false;
                    p_message = ex.Message;
                }
            }

            return p_item;
        }

        /// <summary>
        /// Copy specified Items
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_success"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected CustomTableItem CopyItem(CustomTableItem p_item, out bool p_success, out string p_message)
        {
            p_success = false;
            p_message = String.Empty;

            if (p_item != null)
            {
                try
                {
                    p_item.ItemID = 0;
                    p_item.ItemGUID = new Guid();
                    p_item.Insert();
                    p_message = "Record has been copied. new item id (" + p_item.ItemID.ToString() + ")<br />";
                    p_success = true;
                }
                catch (Exception ex)
                {
                    p_success = false;
                    p_message = ex.Message;
                }
            }

            return p_item;
        }

        /// <summary>
        /// Enable/Disable specified Items
        /// </summary>
        /// <param name="p_item"></param>
        /// <param name="p_enable"></param>
        /// <param name="p_success"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected CustomTableItem EnableItem(CustomTableItem p_item, bool p_enable, out bool p_success, out string p_message)
        {
            p_success = false;
            p_message = String.Empty;

            if (p_item != null)
            {
                try
                {
                    object test = p_item.GetValue(ActiveColumnKey);
                    p_item.SetValue(ActiveColumnKey, p_enable);
                    bool changed = p_item.HasChanged;
                    object test2 = p_item.GetValue(ActiveColumnKey);
                    p_item.Update();
                    string enableString = p_enable ? "enabled" : "disabled";
                    p_message = String.Format("Item {0}. Item ID ({1}) <br/>", enableString, p_item.ItemID);
                    p_success = true;
                }
                catch (Exception ex)
                {
                    p_success = false;
                    p_message = ex.Message;
                }
            }

            return p_item;
        }

        /// <summary>
        /// Get the Custom Table Item
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        protected CustomTableItem GetCustomTableItem(out string p_message)
        {
            p_message = String.Empty;
            int key = GetItemKeyID();
            CustomTableItem item = GetCustomTableItem(key, out p_message);
            return item;
        }

        /// <summary>
        /// Get custom table item.
        /// </summary>
        /// <param name="p_itemid"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected CustomTableItem GetCustomTableItem(object p_itemid, out string p_message)
        {
            CustomTableItem item = null;
            p_message = String.Empty;
            string customTable = CustomTable; //Code name of custom table
            int key = GetSafeInt(p_itemid, 0);
            if (key > 0)
            {
                p_message = "&nbsp;KeyId=" + key.ToString();
                //Definition of custom table in this case
                DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);
                if (dataClassInfo != null)
                {
                    //item = dataClassInfo.Get
                    item = CustomTableItemProvider.GetItem(key, customTable);
                    //ObjectQuery<CustomTableItem> item2 = CustomTableItemProvider.GetItems(customTable).WhereID("ItemID", key);
                }
                if (item == null)
                {
                    p_message += "(Item Not Found)";
                }
            }
            return item;
        }

        /// <summary>
        /// Create a new customtable item.
        /// </summary>
        /// <param name="p_message"></param>
        /// <returns></returns>
        protected CustomTableItem CreateCustomTableItem(out string p_message)
        {
            p_message = String.Empty;
            CustomTableItem item = null;
            string customTable = CustomTable; //Code name of custom table
            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(customTable);
            if (dataClassInfo != null)
            {
                item = CustomTableItem.New(customTable);
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

        #endregion "Custom Table Operations"

        #endregion "Processing"

        #region "Messaging"

        /// <summary>
        /// Set session message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_isError"></param>
        protected void SetSessionMessage(string p_sessionMessage, bool? p_isSessionError)
        {
            SessionMessage = p_sessionMessage;
            IsSessionError = p_isSessionError;
        }

        /// <summary>
        /// Clear the session error message
        /// </summary>
        protected void ClearSessionMessage()
        {
            SetSessionMessage(null, null);
        }

        /// <summary>
        /// Show the session message
        /// </summary>
        protected void ShowSessionMessage(bool p_clearSession)
        {
            if (!String.IsNullOrWhiteSpace(SessionMessage))
            {
                lblMsg.Text = SessionMessage.Replace("\r\n", "<br/>");
                if (IsSessionError.GetValueOrDefault(false))
                {
                    lblMsg.Style["color"] = "Red";
                }
                else
                {
                    lblMsg.Style["color"] = "Green";
                }
            }

            if (p_clearSession)
            {
                ClearSessionMessage();
            }
        }

        #endregion "Messaging"

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

        #endregion "Methods"
    }
}