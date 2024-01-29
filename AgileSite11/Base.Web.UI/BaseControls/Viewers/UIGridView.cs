using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Grid view.
    /// </summary>
    [ToolboxItem(false)]
    public class UIGridView : GridView, IShortID
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
        /// Constructor.
        /// </summary>
        public UIGridView()
        {
            EnableViewState = false;
            GridLines = GridLines.None;
            CellPadding = -1;
            CellSpacing = -1;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            this.SetShortID();

            base.OnInit(e);

            this.AddCssClass("table table-hover " + HTMLHelper.NO_DIVS);
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (HeaderRow != null && ShowHeader)
            {
                HeaderRow.TableSection = TableRowSection.TableHeader;
            }

            if (FooterRow != null && ShowFooter)
            {
                FooterRow.TableSection = TableRowSection.TableFooter;
            }
        }

        #endregion
    }
}