using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using System.Regex;

using CMS.DataEngine;
using CMS.Core;
using CMS.EventLog;
using CMS.PortalEngine.Web.UI;
using CMS.Base.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
public partial class CMSWebParts_Filters_Dashboard_Dashboard_Filter : CMSAbstractWebPart
{
    private string mFilterControlPath = null;
    private string mFilterName = null;
    private CMSAbstractBaseFilterControl mFilterControl = null;


    //== GLOBAL FILTERS
    private string _DOB = ""; //"mm/dd/yyyy";
    private string _STID = ""; // State
    private string _LocID = "-1"; // Region
    private string[] InsuranceID = { "-1", "-1" }; // InsuranceID InsuranceID2
    private string[] MedicalID = { "-1", "-1" }; // (Medical Condition) MedID, MedID2
    private string[] TherapyID = { "-1", "-1" }; // ThID ThID2
    private string SearchTerm = "";
    private string _DashboardUrl = "https://petal.mindpathcare.com/Dashboard-TEST";
    private string _absolutUrl = "https://petal.mindpathcare.com/CMSPages/PortalTemplate.aspx";
    private bool TforTherapyFforMed = false;


    /// <summary>
    /// Gets or sets the path of the filter control.
    /// </summary>
    public string FilterControlPath
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FilterControlPath"), mFilterControlPath);
        }
        set
        {
            SetValue("FilterControlPath", value);
            mFilterControlPath = value;
        }
    }


    /// <summary>
    /// Gets or sets the name of the filter control.
    /// </summary>
    public string FilterName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("FilterName"), mFilterName);
        }
        set
        {
            SetValue("FilterName", value);
            mFilterName = value;
        }
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
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        // In design mode is pocessing of control stoped
        if (StopProcessing)
        {
            // Do nothing
        }
        else
        {
            LoadFilter();
        }
    }


    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        LoadDropDowns();
        LoadFilter();
    }


    /// <summary>
    /// Load filter control according filterpath.
    /// </summary>
    private void LoadFilter()
    {
        if (mFilterControl == null)
        {
            if (FilterControlPath != null)
            {
                try
                {
                    mFilterControl = (Page.LoadUserControl(FilterControlPath)) as CMSAbstractBaseFilterControl;
                    if (mFilterControl != null)
                    {
                        mFilterControl.ID = "filterControl";
                        Controls.AddAt(0, mFilterControl);
                        mFilterControl.FilterName = FilterName;
                        if (Page != null)
                        {
                            mFilterControl.Page = Page;
                            LoadDropDowns();
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("Filter control", "LOADFILTER", ex, loggingPolicy: LoggingPolicy.ONLY_ONCE);
                }
            }
        }
    }

    ///============== [[ Filter boxes loading ]] ============================//

    private void LoadDropDowns()
    {
        LoadStateRegionAndInsuranceDropDowns();
        // LoadState();
        // DropDownLoadRegionData();
        // DropDownLoadInsurance();
        DropDownLoadMedProviderCondition();
        DropDownLooadTherapyInsDetails();
        DataSearch(4); // Load Therapy Details
        DataSearch(3); // Load Medical Conditions Details

        checkUrl();
        // Makde text box able to use enter due to kentico form
        txtDOB.Attributes.Add("onkeypress", "return clickbutton(event, '" + btnSearch.ClientID + "')");
        txtSearch.Attributes.Add("onkeypress", "return clickbutton(event, '" + btnSearch.ClientID + "')");
    }

    private void checkUrl()
    {
        var param = Request.Url.ToString().Replace(_absolutUrl, "").Replace("?", "").Replace("aliaspath=%2Dashboard", "");
        string currentState = "";
        bool shouldReload = false; // with checkboxes. if subs do not match then reload without parameters
        List<string> parameters = param.Split('&').ToList();
        List<string> newParams = param.Split('&').ToList();

        if (!String.IsNullOrWhiteSpace(Request.Params["DOB"]) && Request.Params["DOB"].ToUpper() != "MM/DD/YYYY")
        {
            txtDOB.Text = Request.Params["DOB"];
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["STID"]))
        {
            // TODO: make this check the db next
            if (Request.Params["STID"].ToUpper().Contains("NC"))
            {
                ddState.SelectedIndex = 1;
                currentState = Request.Params["STID"];
            }

            if (Request.Params["STID"].ToUpper().Contains("SC"))
            {
                ddState.SelectedIndex = 2;
                currentState = Request.Params["STID"];
            }
            ddRegion.Enabled = true;
            ddStateInsOne.Enabled = true;
            ddStateInsTwo.Enabled = true;
            //CheckRegionInsDropdowns();
            // ok.. now letd disabled items which have our state
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["LocID"]))
        {
            
            if (ddRegion.Items.FindByValue(Request.Params["LocID"]) != null) {
                if (Request.Params["LocID"] == "-1") {
                    ddRegion.SelectedIndex = 0;
                } else {
                    ddRegion.Items.FindByValue(Request.Params["LocID"]).Selected = true;
                    ddRegion.SelectedIndex = Int32.Parse(Request.Params["LocID"]);

                }
            }
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["InsuranceID"]))
        {
            ddStateInsOne.SelectedValue = Request.Params["InsuranceID"];

            if (!ddStateInsOne.SelectedItem.Text.Contains(currentState))
            {
                newParams.RemoveAll(y => y == "InsuranceID=" + Request.Params["InsuranceID"]);
                shouldReload = true;
            }
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["InsuranceID2"]))
        {
            ddStateInsTwo.SelectedValue = Request.Params["InsuranceID2"];
            if (!ddStateInsTwo.SelectedItem.Text.Contains(currentState))
            {
                newParams.RemoveAll(y => y == "InsuranceID2=" + Request.Params["InsuranceID2"]);
                // shouldReload = true;
            }
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["therBtn"]))
        {
            if (Request.Params["therBtn"] == "1")
            {
                chisTherapy.Checked = true;
                ddTherapyCondPri.Enabled = true;
                ddTherapyCondSec.Enabled = true;
            }
            else
            {
                chisTherapy.Checked = false;
                ddTherapyCondPri.Enabled = false;
                ddTherapyCondPri.SelectedIndex = 0;
                ddTherapyCondSec.Enabled = false;
                ddTherapyCondSec.SelectedIndex = 0;
            }

        }
        if (!String.IsNullOrWhiteSpace(Request.Params["medBtn"]))
        {
            if (Request.Params["medBtn"] == "1")
            {
                chisMedProv.Checked = true;
                ddPri_MedProvCond.Enabled = true;
                ddSec_MedProvCond.Enabled = true;
                ddPri_MedProvCond.Style["display"]="";
                ddSec_MedProvCond.Style["display"]="";
            }
            else
            {
                chisMedProv.Checked = false;
                ddPri_MedProvCond.Enabled = false;
                ddPri_MedProvCond.SelectedIndex = 0;
                ddPri_MedProvCond.Style["display"]="none";
                ddSec_MedProvCond.Enabled = false;
                ddSec_MedProvCond.SelectedIndex = 0;
                ddSec_MedProvCond.Style["display"]="none";
            }

        }
        if (!String.IsNullOrWhiteSpace(Request.Params["MedID"]))
        {
            ddPri_MedProvCond.SelectedValue = Request.Params["MedID"];
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["MedID2"]))
        {
            ddSec_MedProvCond.SelectedValue = Request.Params["MedID2"];
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["ThID"]))
        {
            ddTherapyCondPri.SelectedValue = Request.Params["ThID"];
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["ThID2"]))
        {
            ddTherapyCondSec.SelectedValue = Request.Params["ThID2"];
        }
        if (!String.IsNullOrWhiteSpace(Request.Params["SearchTerm"]))
        {
            txtSearch.Text = Request.Params["SearchTerm"].Replace("+", " ");
        }


        if (shouldReload)
        { // For states we need to removed the INS1&2
            int counter = 0;
            string reloadList = "?";
            newParams.ForEach(x =>
            {
                if (counter > 0)
                {
                    reloadList += "&";
                }
                reloadList += x;
                counter++;
            });
            //Response.Redirect(HttpUtility.UrlEncode(_DashboardUrl + reloadList));
        }
    }

    private void CheckRegionInsDropdowns()
    {
        if (ddRegion.Items != null)
        {
            foreach (ListItem ddItem in ddRegion.Items)
            {
                if (!ddItem.Value.Contains(ddState.SelectedValue))
                    lblDEBUG.Text = "hit";
            }
        }
    }

    private void LoadStateRegionAndInsuranceDropDowns()
    {
        // State
        ddState.Items.Clear();
        ddState.Items.Insert(0, new ListItem("Select State", "0"));
        // Region 
        ddRegion.Items.Clear();
        ddRegion.Items.Insert(0, new ListItem("Any Region", "0"));
        ddRegion.Enabled = false;
        // Insurance
        // Primary
        ddStateInsOne.Items.Clear();
        ddStateInsOne.Items.Insert(0, new ListItem("Any Insurance", "0"));
        ddStateInsOne.Enabled = false;
        // Secondary
        ddStateInsTwo.Items.Clear();
        ddStateInsTwo.Items.Insert(0, new ListItem("Any Insurance", "0"));
        ddStateInsTwo.Enabled = false;
        DataSearch(0);
        DataSearch(1);
        DataSearch(2);
    }
    private void LoadState() // OnPageLoad > Load the states
    {
        ddState.Items.Clear();
        ddState.Items.Insert(0, new ListItem("Select State", "0"));
        // Get States
        // SELECT DISTINCT [STATE] FROM Form_mindpath_Regions WHERE RegionName IS NOT NULL ORDER BY [STATE] -- AND state = STID
        DataSearch(0);
    }

    //====== [[ ON_CLICKS ]] =====

    private void DD_StatetoRegion()
    {
        if (ddState.SelectedIndex > 0)
        {
            DataSearch(1); // Load Region Details
            DataSearch(2); // Load Insurace Details
            ddRegion.Enabled = true;
            ddStateInsOne.Enabled = true;
            ddStateInsTwo.Enabled = true;
        }
        else
        {
            ddRegion.Enabled = false;
            ddStateInsOne.Enabled = false;
            ddStateInsTwo.Enabled = false;

        }
    }

    private void DropDownLoadRegionData() // PageInit load Region box
    {
        ddRegion.Items.Clear();
        ddRegion.Items.Insert(0, new ListItem("Any Region", "0"));
        ddRegion.Enabled = false;
    }

    private void DropDownLoadInsurance()
    {
        // Primary
        ddStateInsOne.Items.Clear();
        ddStateInsOne.Items.Insert(0, new ListItem("Any Insurance", "0"));
        ddStateInsOne.Enabled = false;
        // Secondary
        ddStateInsTwo.Items.Clear();
        ddStateInsTwo.Items.Insert(0, new ListItem("Any Insurance", "0"));
        ddStateInsTwo.Enabled = false;
    }


    private void DropDownLoadMedProviderCondition()
    {
        // Primary
        ddPri_MedProvCond.Items.Clear();
        ddPri_MedProvCond.Items.Insert(0, new ListItem("Primary Med Condition", "0"));
        ddPri_MedProvCond.Enabled = false;
        // Secondary
        ddSec_MedProvCond.Items.Clear();
        ddSec_MedProvCond.Items.Insert(0, new ListItem("Secondary Med Condition", "0"));
        ddSec_MedProvCond.Enabled = false;
    }
    private void DropDownLooadTherapyInsDetails()
    {
        // Primary
        ddTherapyCondPri.Items.Clear();
        ddTherapyCondPri.Items.Insert(0, new ListItem("Primary Therapy Condition", "0"));
        ddTherapyCondPri.Enabled = false;
        // Secondary
        ddTherapyCondSec.Items.Clear();
        ddTherapyCondSec.Items.Insert(0, new ListItem("Secondary Therapy Condition", "0"));
        ddTherapyCondSec.Enabled = false;
    }

    ///============== [[ Filter boxes loading END ]] ============================//


    // === On clickers
    protected void GenericOnClick(object sender, EventArgs e)
    {
        DDSelected_ChangeUrl();
    }

    protected void checkMedical(object sender, EventArgs e)
    {
        chisTherapy.Checked = false;
        ddTherapyCondPri.SelectedIndex = 0;
        ddTherapyCondSec.SelectedIndex = 0;
    }
    protected void checkTherapy(object sender, EventArgs e)
    {
        chisMedProv.Checked = false;
        ddPri_MedProvCond.SelectedIndex = 0;
        ddSec_MedProvCond.SelectedIndex = 0;
    }

    protected void filterSearch(object sender, EventArgs e)
    {

    }

    // === Onclick supporting functions
    protected void DDSelected_ChangeUrl()
    {
        string urlBuilder = "";
        

        if (txtDOB.Text != "mm/dd/yyyy") // check DOB
        {
            urlBuilder = "?DOB=" + txtDOB.Text;
        } else {
            urlBuilder = "?DOB=mm/dd/yyyy";
        }
        if (ddRegion.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?LocID=" : "&LocID=";
            urlBuilder += ddRegion.SelectedItem.Value;
        } else {
            urlBuilder += "&LocID=-1";
        }
        if (ddState.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?STID=" : "&STID=";
            urlBuilder += ddState.SelectedItem.Value;
        } 
        else if (ddState.SelectedIndex == 0)
        {
            urlBuilder += "&STID=-1";
            ddStateInsOne.SelectedIndex = 0;
            ddStateInsTwo.SelectedIndex = 0;
        }

        if (ddStateInsOne.SelectedIndex > 0)
        { // Insuracnce One
            urlBuilder += urlBuilder == "" ? "?InsuranceID=" : "&InsuranceID=";
            urlBuilder += ddStateInsOne.SelectedItem.Value;
        } else {
            urlBuilder += "&InsuranceID=-1";
        }
        if (ddStateInsTwo.SelectedIndex > 0)
        { // Insurance Two
            urlBuilder += urlBuilder == "" ? "?InsuranceID2=" : "&InsuranceID2=";
            urlBuilder += ddStateInsTwo.SelectedItem.Value;
        } else {
            urlBuilder += "&InsuranceID2=-1";
        }
        // === Btns
        if (chisMedProv.Checked == true)
        {
            urlBuilder += urlBuilder == "" ? "?medBtn=" : "&medBtn=";
            urlBuilder += 1;
            urlBuilder += "&ProvType=1";
            // Lets Disabled Therapy
            chisTherapy.Checked = false;
            ddTherapyCondPri.SelectedIndex = 0;
            ddTherapyCondSec.SelectedIndex = 0;
        }
        else if (chisMedProv.Checked == false)
        {
            urlBuilder += urlBuilder == "" ? "?medBtn=" : "&medBtn=";
            urlBuilder += 0;
            // 1
            ddPri_MedProvCond.SelectedIndex = 0;
            // 2
            ddSec_MedProvCond.SelectedIndex = 0;
        }
        //----- Therapy btn
        if (chisTherapy.Checked == true)
        {
            urlBuilder += urlBuilder == "" ? "?therBtn=" : "&therBtn=";
            urlBuilder += 1;
            urlBuilder += "&PMVIP=1";
            // Lets Disable Medical
            chisMedProv.Checked = false;
            ddPri_MedProvCond.SelectedIndex = 0;
            ddSec_MedProvCond.SelectedIndex = 0;

        }
        else if (chisTherapy.Checked == false)
        {
            urlBuilder += urlBuilder == "" ? "?therBtn=" : "&therBtn=";
            urlBuilder += 0;
            // 1
            ddTherapyCondPri.SelectedIndex = 0;
            // 2
            ddTherapyCondSec.SelectedIndex = 0;
        }

        if (chisIsAPP.Checked == true) {
            urlBuilder += urlBuilder == "" ? "?isApp=" : "&isApp=";
            urlBuilder += 1;
        }

        // Medical Conditions check
        if (ddPri_MedProvCond.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?MedID=" : "&MedID=";
            urlBuilder += ddPri_MedProvCond.SelectedItem.Value;
        } else {
            urlBuilder += "&MedID=-1";
        }
        if (ddSec_MedProvCond.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?MedID2=" : "&MedID2=";
            urlBuilder += ddSec_MedProvCond.SelectedItem.Value;
        } else {
            urlBuilder += "&MedID2=-1";
        }
        // Therapy Check
        if (ddTherapyCondPri.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?ThID=" : "&ThID=";
            urlBuilder += ddTherapyCondPri.SelectedItem.Value;
        } else {
            urlBuilder += "&ThID=-1";
        }
        if (ddTherapyCondSec.SelectedIndex > 0)
        {
            urlBuilder += urlBuilder == "" ? "?ThID2=" : "&ThID2=";
            urlBuilder += ddTherapyCondSec.SelectedItem.Value;
        } else {
            urlBuilder += "&ThID2=-1";
        }
        if (txtSearch.Text != "")
        {
            urlBuilder += urlBuilder == "" ? "?SearchTerm=" : "&SearchTerm=";
            urlBuilder += txtSearch.Text.Replace(" ", "+");
        } else {
            urlBuilder += "&SearchTerm=";
        }

        Response.Redirect(_DashboardUrl + urlBuilder);
    }

    protected void BTN_ClearFilters(Object sender, EventArgs e)
    {
        txtDOB.Text = "mm/dd/yyyy";
        ddState.SelectedIndex = 0;
        ddRegion.Enabled = false;
        ddRegion.SelectedIndex = 0;
        chisMedProv.Checked = false;
        chisTherapy.Checked = false;
        ddStateInsOne.Enabled = false;
        ddStateInsOne.SelectedIndex = 0;
        ddStateInsTwo.Enabled = false;
        ddStateInsTwo.SelectedIndex = 0;
        ddPri_MedProvCond.Enabled = false;
        ddPri_MedProvCond.SelectedIndex = 0;
        ddSec_MedProvCond.Enabled = false;
        ddSec_MedProvCond.SelectedIndex = 0;
        ddTherapyCondPri.Enabled = false;
        ddTherapyCondPri.SelectedIndex = 0;
        ddTherapyCondSec.Enabled = false;
        ddTherapyCondSec.SelectedIndex = 0;
        Response.Redirect(_DashboardUrl);

    }

    ///======== [[ Functions ]] =====

    private void DataSearch(int query)
    {
        try
        {
            string state = "";
            int counter = 1;
            string tsql = @"";
            string whereValue = @"";

            if (ddState.SelectedIndex > 0)
            {
                _STID = ddState.SelectedItem.Value;
            }

            switch (query)
            {
                case 0: // State
                    tsql = @"SELECT DISTINCT [STATE] FROM Form_mindpath_Regions WHERE RegionName IS NOT NULL AND STATE LIKE '%{0}%' ORDER BY [STATE]";
                    whereValue = _STID;
                    break;
                case 1: //Region
                    tsql = @"SELECT [RegionsID], [RegionName], [STATE] FROM Form_mindpath_Regions WHERE RegionName IS NOT NULL AND STATE LIKE '%{0}%' ORDER BY [STATE], [RegionName]";
                    whereValue = _STID;
                    break;
                case 2: // Insurance
                    tsql = "SELECT [InsuranceID], [Provider] FROM Form_mindpath_Insurance WHERE [Provider] LIKE '%{0}%' ORDER BY InsuranceID";
                    whereValue = _STID;
                    break;
                case 3: // Medical Condition
                    tsql = @"SELECT [ConditionsID], [Abrev] FROM Form_mindpath_Conditions WHERE [TypeMedTh] = 1 ORDER BY Abrev";
                    break;
                case 4:
                    tsql = @"SELECT [ConditionsID], [Abrev] FROM Form_mindpath_Conditions WHERE [TypeMedTh] = 2 ORDER BY Abrev";
                    break;
            }

            if (tsql != "")
            {
                DataSet ds2 = ConnectionHelper.ExecuteQuery(string.Format(tsql, _STID), null, QueryTypeEnum.SQLQuery);

                if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow rower in ds2.Tables[0].Rows)
                    {
                        if (query == 0)
                        { // for single
                            RowInserter(query, counter, rower["state"].ToString());
                        }
                        else if (query == 2)
                        { // for double
                            RowInserter(query, counter, rower["insuranceid"].ToString(), rower["provider"].ToString());
                        }
                        else if (query == 1)
                        { // for triple
                            RowInserter(query, counter, rower["RegionsID"].ToString(), rower["RegionName"].ToString(), rower["STATE"].ToString());
                        }
                        else if (query == 3 || query == 4)
                        { // 3 is medical , 4 istherapy
                            RowInserter(query, counter, rower["ConditionsID"].ToString(), rower["Abrev"].ToString());
                        }
                        counter++;
                    }
                }
                else
                {
                    EventLogProvider.LogEvent(EventType.INFORMATION, "Dashboard Filter Error", "CUSTOMREGESTRATION", eventDescription: "no group found");
                }
            }
        }
        catch (Exception ex)
        {
            // Logs an information event into the event log
            EventLogProvider.LogEvent(EventType.INFORMATION, "Registration Error", "CUSTOMREGESTRATION", eventDescription: ex.StackTrace.ToString());
        }
    }

    private void RowInserter(int query, int counter, string rowItemOne, string rowItemTwo = "", string rowItemThree = "")
    {
        switch (query)
        {
            case 0:
                ddState.Items.Insert(counter, new ListItem(rowItemOne, rowItemOne));
                break;
            case 1: // [RegionID], [RegionName], State
                ddRegion.Items.Insert(counter, new ListItem(rowItemTwo + ", " + rowItemThree, rowItemOne));
                break;
            case 2: // [InsuranceID], [Provider]
                // string dbString = rowItemTwo;
                // var regex = new Regex(Regex.Escape("-"));
                // var newText = regex.Replace(dbString, "~", 1);
                // string[] subString = newText.Split('~');
                // string newInsurace = "<span style='display:none'>" + subString[0].Trim() + "</span>" + subString[1].Trim();

                ddStateInsOne.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                ddStateInsTwo.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                break;
            case 3:// Medical condition
                ddPri_MedProvCond.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                ddSec_MedProvCond.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                break;
            case 4: // THerapy condition
                ddTherapyCondPri.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                ddTherapyCondSec.Items.Insert(counter, new ListItem(rowItemTwo, rowItemOne));
                break;
            default:
                break;
        }
    }
}