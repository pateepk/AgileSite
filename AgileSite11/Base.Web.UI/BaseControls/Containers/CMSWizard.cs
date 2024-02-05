using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Wizard.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSWizard : Wizard, IShortID
    {
        #region "Properties"

        /// <summary>
        /// Short ID of the control.
        /// </summary>
        public string ShortID
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);
        }

        #endregion
    }
}