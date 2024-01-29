using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Globalization;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the time zone type selection.
    /// </summary>
    [ToolboxData("<{0}:TimeZoneTypeSelector runat=server></{0}:TimeZoneTypeSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class TimeZoneTypeSelector : FormControl
    {
        #region "Private variables"

        private CMSDropDownList drpList = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Type of time zone.
        /// </summary>
        public TimeZoneTypeEnum TimeZoneType
        {
            get
            {
                string type = ValidationHelper.GetString(Value, String.Empty);
                return type.ToEnum<TimeZoneTypeEnum>();
            }
            set
            {
                Value = value.ToStringRepresentation<TimeZoneTypeEnum>();
            }
        }


        /// <summary>
        /// Inner drop down list with time zone types.
        /// </summary>
        public CMSDropDownList TimeZoneTypeDropDownList
        {
            get
            {
                return EnsureChildControl<CMSDropDownList>(ref drpList);
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public TimeZoneTypeSelector()
        {
            FormControlName = "TimeZoneTypeSelector";
        }
    }
}