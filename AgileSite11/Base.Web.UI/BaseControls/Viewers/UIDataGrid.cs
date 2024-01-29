using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base DataGrid class.
    /// </summary>
    [ToolboxItem(false)]
    public class UIDataGrid : DataGrid, IShortID
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


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Ensure no conversion to DIVs
            this.CssClass = CssHelper.EnsureClass(this.CssClass, HTMLHelper.NO_DIVS);
        }

        #endregion
    }
}