using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the gender selection.
    /// </summary>
    [ToolboxData("<{0}:GenderSelector runat=server></{0}:GenderSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class GenderSelector : FormControl
    {
        private CMSDropDownList drpList = null;
        private CMSRadioButtonList rdbList = null;

        #region "Properties"

        /// <summary>
        /// Returns value converted to int.
        /// </summary>
        public int Gender
        {
            get
            {
                return ValidationHelper.GetInteger(Value, 0);
            }
            set
            {
                Value = value;
            }
        }


        /// <summary>
        /// Returns current control - RadioButtonList or CMSDropDownList.
        /// </summary>
        public Control CurrentSelector
        {
            get
            {
                EnsureChildControl<CMSRadioButtonList>(ref rdbList);
                EnsureChildControl<CMSDropDownList>(ref drpList);

                if (rdbList.Visible)
                {
                    return rdbList;
                }
                else if (drpList.Visible)
                {
                    return drpList;
                }
                
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public GenderSelector()
        {
            FormControlName = "GenderSelector";
        }
    }
}