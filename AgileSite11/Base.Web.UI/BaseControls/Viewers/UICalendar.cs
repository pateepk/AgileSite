using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base Calendar class.
    /// </summary>
    [ToolboxItem(false)]
    public class UICalendar : Calendar, IShortID
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