using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.WTE.MOBLZ
{
    public partial class WTECustomOfficeParkWebPart : CMSAbstractWebPart
    {
        private HashSet<string> _citiesSelections = new HashSet<string>();

        private HashSet<string> _officeParksSelections = new HashSet<string>();

        private string _selectedCities = string.Empty;

        private string _selectedOfficeParks = string.Empty;

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Creates the child controls
            SetupControl();
            base.OnInit(e);
        }

        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Checks if the current request is a postback
            if (RequestHelper.IsPostBack())
            {
            }

            base.OnPreRender(e);
        }

        private void SetupControl()
        {
            if (this.StopProcessing)
            {
                this.Visible = false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                InitializeCitiesFilter();
            }
        }

        private void InitializeCitiesFilter()
        {
            this.citiesRepeater.DataSource = GetCities();
            this.citiesRepeater.DataBind();
        }

        private void InitializeOfficeParksFilter(string selectedCities, string selectedOfficeParks)
        {
            this.officeParkRepeater.DataSource = GetOfficeParks(selectedCities);
            this.officeParkRepeater.DataBind();

            if (!string.IsNullOrEmpty(selectedCities) && !string.IsNullOrEmpty(selectedOfficeParks))
            {
                this.officeParkRepeater.Visible = true;
                foreach (RepeaterItem item in officeParkRepeater.Items)
                {
                    CheckBox repeaterCheckBox = (CheckBox)item.FindControl("chkOfficePark");

                    if (repeaterCheckBox != null)
                    {
                        var foundId = repeaterCheckBox.Attributes["OfficeParkId"];
                        if (_officeParksSelections.Contains(foundId))
                        {
                            repeaterCheckBox.Checked = true;
                        }
                    }
                }
            }

            BindCardRepeater();
        }

        protected void chkCity_CheckedChanged(object sender, EventArgs e)
        {
            SetCitiesAndPaksIds();
            _selectedCities = _citiesSelections.Join(",");
            _selectedOfficeParks = _officeParksSelections.Join(",");
            litOfficePark.Visible = !string.IsNullOrEmpty(_selectedCities) && _selectedCities.Length > 0;
            InitializeOfficeParksFilter(_selectedCities, _selectedOfficeParks);
        }

        protected void chkOfficePark_CheckedChanged(object sender, EventArgs e)
        {
            SetCitiesAndPaksIds();

            _selectedCities = _citiesSelections.Join(",");
            _selectedOfficeParks = _officeParksSelections.Join(",");

            BindCardRepeater();
        }

        private string GetMOBLZConnectionString()
        {
            return "Data Source=10.100.1.109;Initial Catalog=AS11_production_MOBLZ;user id=sa;password=FrodoFrodo!!";
        }

        private void SetCitiesAndPaksIds()
        {
            foreach (RepeaterItem item in officeParkRepeater.Items)
            {
                CheckBox repeaterCheckBox = (CheckBox)item.FindControl("chkOfficePark");

                if (repeaterCheckBox != null)
                {
                    var foundId = repeaterCheckBox.Attributes["OfficeParkId"];
                    if (!string.IsNullOrEmpty(foundId))
                    {
                        if (repeaterCheckBox.Checked)
                        {
                            _officeParksSelections.Add(foundId);
                        }
                        else if (!repeaterCheckBox.Checked && _officeParksSelections.Contains(foundId))
                        {
                            _officeParksSelections.Remove(foundId);
                        }
                    }
                }
            }

            foreach (RepeaterItem item in citiesRepeater.Items)
            {
                CheckBox repeaterCheckBox = (CheckBox)item.FindControl("chkCity");

                if (repeaterCheckBox != null)
                {
                    var foundId = repeaterCheckBox.Attributes["CityID"];
                    if (!string.IsNullOrEmpty(foundId))
                    {
                        if (repeaterCheckBox.Checked)
                        {
                            _citiesSelections.Add(foundId);
                        }
                        else if (!repeaterCheckBox.Checked && _citiesSelections.Contains(foundId))
                        {
                            _citiesSelections.Remove(foundId);
                        }
                    }
                }
            }
        }

        private void BindCardRepeater()
        {
            this.cardRepeater.DataSource = GetCardRepeaterData();
            this.cardRepeater.DataBind();
        }

        private DataSet GetCities()
        {
            string connectionString = GetMOBLZConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("MOBLZ_Cities_Get", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                DataSet dataSet = new DataSet();
                connection.Open();
                adapter.Fill(dataSet);

                adapter.Dispose();
                connection.Close();

                return dataSet;
            }
        }

        private DataSet GetOfficeParks(string p_cityIds)
        {
            string connectionString = GetMOBLZConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("MOBLZ_OfficeParks_GetByCityIds", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("@cityIds", p_cityIds);
                DataSet dataSet = new DataSet();

                connection.Open();
                adapter.Fill(dataSet);

                adapter.Dispose();
                connection.Close();

                return dataSet;
            }
        }

        private DataSet GetCardRepeaterData()
        {
            string connectionString = GetMOBLZConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("MOBLZ_ProperyOfficeParks_GetByCityIdsOrParkIds", connection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("@cityIds", _selectedCities);
                adapter.SelectCommand.Parameters.AddWithValue("@officeParkIds", _selectedOfficeParks);
                DataSet dataSet = new DataSet();

                connection.Open();
                adapter.Fill(dataSet);

                adapter.Dispose();
                connection.Close();

                return dataSet;
            }
        }
    }
}