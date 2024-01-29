//using System;
//using System.Data;
using System.Drawing;
//using System.Linq;
//using CMS.Controls;
//using CMS.ExtendedControls;
//using CMS.Helpers;
//using CMS.PortalControls;
using Telerik.Charting;

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class AccountAllocationGraph : CMSAbstractWebPart
    {
        #region "Variables"

        // Indicates whether control was binded
        private bool binded = false;

        // Base datasource instance
        private CMSBaseDataSource mDataSourceControl = null;

        private Color[] chartColors = new Color[]{
            Color.FromArgb(58, 83, 164), //blue
            Color.FromArgb(247,236,22), //yellow
            Color.FromArgb(49,159,84), //green
            Color.FromArgb(237,28,36), //red
            Color.FromArgb(193,164,206), //lavender
            Color.FromArgb(245,130,32), //orange
            Color.FromArgb(129,88,34), //Brown
            Color.FromArgb(115,196,143), //sea-green
            Color.FromArgb(240,76,35), //red-orange
            Color.FromArgb(130,66,68), //maroon
        };

        #endregion "Variables"

        #region "Properties"

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("DataSourceName"), "");
            }
            set
            {
                this.SetValue("DataSourceName", value);
            }
        }

        /// <summary>
        /// Gets or sets name of source.
        /// </summary>
        public string ChartTitle
        {
            get
            {
                return ValidationHelper.GetString(this.GetValue("ChartTitle"), "");
            }
            set
            {
                this.SetValue("ChartTitle", value);
            }
        }

        /// <summary>
        /// Control with data source.
        /// </summary>
        public CMSBaseDataSource DataSourceControl
        {
            get
            {
                // Check if control is empty and load it with the data
                if (this.mDataSourceControl == null)
                {
                    if (!String.IsNullOrEmpty(this.DataSourceName))
                    {
                        this.mDataSourceControl = CMSControlsHelper.GetFilter(this.DataSourceName) as CMSBaseDataSource;
                    }
                }

                return this.mDataSourceControl;
            }
            set
            {
                this.mDataSourceControl = value;
            }
        }

        #endregion "Properties"

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check whether postback was executed from current transformation item
            if (RequestHelper.IsPostBack())
            {
                // Indicates whether postback was fired from current control
                bool bindControl = false;

                // Check event target value and callback parameter value
                string eventTarget = ValidationHelper.GetString(this.Request.Form["__EVENTTARGET"], String.Empty);
                string callbackParam = ValidationHelper.GetString(this.Request.Form["__CALLBACKPARAM"], String.Empty);
                if (eventTarget.StartsWith(this.UniqueID) || callbackParam.StartsWith(this.UniqueID) || eventTarget.EndsWith(ContextMenu.CONTEXT_MENU_SUFFIX))
                {
                    bindControl = true;
                }
                // Check whether request key contains some control assigned to current control
                else
                {
                    foreach (string key in this.Request.Form.Keys)
                    {
                        if ((key != null) && key.StartsWith(this.UniqueID))
                        {
                            bindControl = true;
                            break;
                        }
                    }
                }
            }

            //base.OnLoad(e);
        }

        protected void rcAssetAllocation_ItemDataBound(object sender, ChartItemDataBoundEventArgs e)
        {
            DataRowView dataItem = (DataRowView)e.DataItem;
            e.SeriesItem.Name = dataItem["chartlegend"].ToString();

            //set pie chart colors
            int i = 0;
            foreach (ChartSeriesItem item in rcAssetAllocation.Series[0].Items)
            {
                //if (i >= chartColors.Count<Color>())
                //{
                //    i = 0;
                //}
                item.Appearance.FillStyle.MainColor = chartColors[i++];
                item.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Solid;
            }
        }

        /// <summary>
        /// OnPreRender override.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Datasource data
            object ds = null;

            // Set transformations if data source is not empty
            if (this.DataSourceControl != null)
            {
                // Get data from datasource
                ds = this.DataSourceControl.DataSource;

                // Check whether data exist
                if ((!DataHelper.DataSourceIsEmpty(ds)) && (!binded))
                {
                    // Initilaize related data if provided
                    if (this.DataSourceControl.RelatedData != null)
                    {
                        this.RelatedData = this.DataSourceControl.RelatedData;
                    }

                    DataSet dataSet = this.DataSourceControl.DataSource as DataSet;
                    DataView dataView = new DataView(dataSet.Tables[0], "", "", DataViewRowState.CurrentRows);
                    if (dataView.Count > 0)
                    {
                        int rowNum = dataView.Count;
                        int maxSlices = 10;
                        while (rowNum > maxSlices)
                        {
                            DataRowView curViewRow = dataView[rowNum - 1];
                            DataRowView sumViewRow = dataView[rowNum - 2];
                            sumViewRow["chartlabel"] = Convert.ToDecimal(sumViewRow["chartlabel"]) + Convert.ToDecimal(curViewRow["chartlabel"]);
                            curViewRow.Delete();
                            rowNum--;
                            if (rowNum == maxSlices)
                            {
                                sumViewRow["chartlegend"] = "Other";
                            }
                        }

                        rcAssetAllocation.DataSource = dataView;

                        //rcAssetAllocation.DataSource = this.DataSourceControl.DataSource;
                        this.rcAssetAllocation.DataBind();
                        rcAssetAllocation.ChartTitle.TextBlock.Text = this.ChartTitle;
                    }
                    else
                    {
                        rcAssetAllocation.Visible = false;
                    }
                }
            }

            base.OnPreRender(e);

            // Hide control for zero rows
            if (((this.DataSourceControl == null) || (DataHelper.DataSourceIsEmpty(ds))))
            {
                this.Visible = false;
            }
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
        }
    }
}