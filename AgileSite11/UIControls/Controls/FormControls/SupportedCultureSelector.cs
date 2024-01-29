using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the supported culture selection.
    /// </summary>
    [ToolboxData("<{0}:SupportedCultureSelector runat=server></{0}:SupportedCultureSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SupportedCultureSelector : FormControl
    {
        #region "Private variables"

        private UniSelector mCurrentSelector;

        #endregion


        #region "Public properties"


        /// <summary>
        /// Returns ClientID of the textbox with culture.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                EnsureChildControls();
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
        /// Gets or sets whether (all) field is in dropdownlist.
        /// </summary>
        public bool DisplayAllValue
        {
            get;
            set;
        }


        /// <summary>
        /// Supported language cultures
        /// </summary>
        public string SupportedCultures
        {
            get
            {
                return GetValue("SupportedCultures", String.Empty);
            }
            set
            {
                SetValue("SupportedCultures", value);
            }
        }


        /// <summary>
        /// If true, selector allows multiple values selection.
        /// </summary>
        public bool AllowMultipleSelection
        {
            get
            {
                return GetValue("AllowMultipleSelection", false);
            }
            set
            {
                SetValue("AllowMultipleSelection", value);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public SupportedCultureSelector()
        {
            FormControlName = "SupportedCultureSelector";
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


        void CurrentSelector_OnSpecialFieldsLoaded(object sender, EventArgs e)
        {
            if (DisplayAllValue)
            {
                CurrentSelector.SpecialFields.Add(new SpecialField() { Text = ResHelper.GetString("general.selectall"), Value = "0" });
            }
        }

        #endregion
    }
}