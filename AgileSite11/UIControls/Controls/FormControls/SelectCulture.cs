using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;


namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the site culture selection.
    /// </summary>
    [ToolboxData("<{0}:SelectCulture runat=server></{0}:SelectCulture>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectCulture : FormControl
    {
        #region "Private variables"

        private UniSelector mCurrentSelector;
        private CMSDropDownList mDropDownList;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets Value display name.
        /// </summary>
        public string ValueDisplayName
        {
            get
            {
                return CurrentSelector.ValueDisplayName;
            }
        }
        

        /// <summary>
        /// Returns ClientID of the textbox with culture.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                return CurrentSelector.TextBoxSelect.ClientID;
            }
        }
                       

        /// <summary>
        /// Gets or sets the value which determines, whether to display Clear button.
        /// </summary>
        public bool DisplayClearButton
        {
            get
            {
                return CurrentSelector.AllowEmpty;
            }
            set
            {
                CurrentSelector.AllowEmpty = value;
            }
        }


        /// <summary>
        /// Gets or sets the ID of the site for which the cultures should be returned. 0 means current site. -1 all sites = all cultures.
        /// </summary>
        public int SiteID
        {
            get
            {
                return GetValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// Gets current uniselector control.
        /// </summary>
        public UniSelector CurrentSelector
        {
            get
            {
                return EnsureChildControl<UniSelector>(ref mCurrentSelector);
            }
        }


        /// <summary>
        /// Gets drop down control (drop down list mode)
        /// </summary>
        public CMSDropDownList SingleSelectDropDown
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref mDropDownList);
            }
        }


        /// <summary>
        /// Gets or sets column name, default value is culture name.
        /// </summary>
        public string ReturnColumnName
        {
            get
            {
                return CurrentSelector.ReturnColumnName;
            }
            set
            {
                CurrentSelector.ReturnColumnName = value;
            }
        }


        /// <summary>
        /// Gets or sets whether (all) field is in CMSDropDownList.
        /// </summary>
        public bool DisplayAllValue
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public SelectCulture()
        {
            FormControlName = "SelectCulture";
        }


        #region "Methods"

        /// <summary>
        /// OnInit event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CurrentSelector.OnSpecialFieldsLoaded += CurrentSelector_OnSpecialFieldsLoaded;
        }


        /// <summary>
        /// OnSpecialFieldsLoaded event handler
        /// </summary>
        protected void CurrentSelector_OnSpecialFieldsLoaded(object sender, EventArgs e)
        {
            if (DisplayAllValue)
            {
                CurrentSelector.SpecialFields.Add(new SpecialField() { Text = ResHelper.GetString("general.selectall"), Value = "0" });
            }
        }


        /// <summary>
        /// OnLoad event handler
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (SiteID < 0)
            {
                SetValue("DisplayAllCultures", true);
            }
        }

        #endregion
    }
}