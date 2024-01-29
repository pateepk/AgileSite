using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for captcha type selection.
    /// </summary>
    [ToolboxData("<{0}:CaptchaSelector runat=server></{0}:CaptchaSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class CaptchaSelector : FormControl
    {
        #region "Private variables"

        private CMSDropDownList drpList = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether enable dropdown list autopostback.
        /// </summary>
        public bool AllowAutoPostBack
        {
            get
            {
                return AutoPostBack;
            }
            set
            {
                AutoPostBack = value;
            }
        }


        /// <summary>
        /// This property exists only for backward compatibility. Has no effect to reloading drop down list.
        /// </summary>
        public bool ReloadDataOnPostback
        {
            get;
            set;
        }


        /// <summary>
        /// Returns ClientID of the CMSDropDownList with bad word action.
        /// </summary>
        public string ValueElementID
        {
            get
            {
                EnsureChildControl<CMSDropDownList>(ref drpList);

                return drpList.ClientID;
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public CaptchaSelector()
        {
            FormControlName = "CaptchaSelector";
        }
    }
}