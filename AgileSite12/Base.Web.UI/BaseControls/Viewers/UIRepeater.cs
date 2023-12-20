using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base repeater class.
    /// </summary>
    [ToolboxItem(false)]
    public class UIRepeater : Repeater, IShortID
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


        /// <summary>
        /// Gets or sets the object from which data-bound control retrieves its list of data item.
        /// </summary>
        public override object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = ControlsHelper.GetDataSourceForControl(value);
            }
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