using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Data;
using System.Web.UI.Design;

using CMS.Base.Web.UI;
using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Table control that displays specified records in columns instead of rows.
    /// </summary>
    [ToolboxData("<{0}:BasicMultiColumnTable runat=server></{0}:BasicMultiColumnTable>"), Serializable()]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class BasicMultiColumnTable : CMSWebControl, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// DataSource variable.
        /// </summary>
        protected DataTable mDataSource;

        /// <summary>
        /// Table params.
        /// </summary>
        protected string[,] mTableParams;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if false value of boolean type-columns should be displayed using the template for empty value.
        /// </summary>
        [Category("Behavior"), DefaultValue(false), Description("Indicates if false value of boolean type-columns should be displayed using the template for empty value."), NotifyParentProperty(true)]
        public bool RenderFalseValueUsingEmptyTemplate
        {
            get
            {
                if (ViewState["RenderFalseValueUsingEmptyTemplate"] == null)
                {
                    ViewState["RenderFalseValueUsingEmptyTemplate"] = 0;
                }
                if (Convert.ToInt32(ViewState["RenderFalseValueUsingEmptyTemplate"]) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    ViewState["RenderFalseValueUsingEmptyTemplate"] = 1;
                }
                else
                {
                    ViewState["RenderFalseValueUsingEmptyTemplate"] = 0;
                }
            }
        }


        /// <summary>
        /// Table cell spacing.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Table cell spacing."), NotifyParentProperty(true)]
        public int TableCellSpacing
        {
            get
            {
                if (ViewState["TableCellSpacing"] == null)
                {
                    ViewState["TableCellSpacing"] = 0;
                }
                return Convert.ToInt32(ViewState["TableCellSpacing"]);
            }
            set
            {
                ViewState["TableCellSpacing"] = value;
            }
        }


        /// <summary>
        /// Table cell padding.
        /// </summary>
        [Category("Behavior"), DefaultValue(0), Description("Table cell padding"), NotifyParentProperty(true)]
        public int TableCellPadding
        {
            get
            {
                if (ViewState["TableCellPadding"] == null)
                {
                    ViewState["TableCellPadding"] = 0;
                }
                return Convert.ToInt32(ViewState["TableCellPadding"]);
            }
            set
            {
                ViewState["TableCellPadding"] = value;
            }
        }


        /// <summary>
        /// Table CSS class.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Table CSS class."), NotifyParentProperty(true)]
        public string TableCssClass
        {
            get
            {
                if (ViewState["TableCssClass"] == null)
                {
                    ViewState["TableCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableCssClass"]);
            }
            set
            {
                ViewState["TableCssClass"] = value;
            }
        }


        /// <summary>
        /// Table row (TR) CSS class.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Table row (TR) CSS class."), NotifyParentProperty(true)]
        public string TableRowCssClass
        {
            get
            {
                if (ViewState["TableRowCssClass"] == null)
                {
                    ViewState["TableRowCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableRowCssClass"]);
            }
            set
            {
                ViewState["TableRowCssClass"] = value;
            }
        }


        /// <summary>
        /// First table row (TR) CSS class.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("First table row (TR) CSS class."), NotifyParentProperty(true)]
        public string TableFirstRowCssClass
        {
            get
            {
                if (ViewState["TableFirstRowCssClass"] == null)
                {
                    ViewState["TableFirstRowCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableFirstRowCssClass"]);
            }
            set
            {
                ViewState["TableFirstRowCssClass"] = value;
            }
        }


        /// <summary>
        /// Table cell (TD) CSS class.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Table cell (TD) CSS class."), NotifyParentProperty(true)]
        public string TableCellCssClass
        {
            get
            {
                if (ViewState["TableCellCssClass"] == null)
                {
                    ViewState["TableCellCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableCellCssClass"]);
            }
            set
            {
                ViewState["TableCellCssClass"] = value;
            }
        }


        /// <summary>
        /// Table cell (TD) CSS class for the first row of the table.
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("Table cell (TD) CSS class for the first row of the table."), NotifyParentProperty(true)]
        public string TableFirstRowCellCSSClass
        {
            get
            {
                if (ViewState["TableFirstRowCellCSSClass"] == null)
                {
                    ViewState["TableFirstRowCellCSSClass"] = "";
                }
                return Convert.ToString(ViewState["TableFirstRowCellCSSClass"]);
            }
            set
            {
                ViewState["TableFirstRowCellCSSClass"] = value;
            }
        }


        /// <summary>
        /// CSS class of first table columns' cells (TD).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("CSS class of first table columns' cells (TD)."), NotifyParentProperty(true)]
        public string TableFirstColumnCellCssClass
        {
            get
            {
                if (ViewState["TableFirstColumnCellCssClass"] == null)
                {
                    ViewState["TableFirstColumnCellCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableFirstColumnCellCssClass"]);
            }
            set
            {
                ViewState["TableFirstColumnCellCssClass"] = value;
            }
        }


        /// <summary>
        /// CSS class of the separator row (TR).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("CSS class of the separator row (TR)."), NotifyParentProperty(true)]
        public string TableSeparatorRowCssClass
        {
            get
            {
                if (ViewState["TableSeparatorRowCssClass"] == null)
                {
                    ViewState["TableSeparatorRowCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableSeparatorRowCssClass"]);
            }
            set
            {
                ViewState["TableSeparatorRowCssClass"] = value;
            }
        }


        /// <summary>
        /// CSS class of the separator cell (TD).
        /// </summary>
        [Category("Behavior"), DefaultValue(""), Description("CSS class of the separator cell (TD)."), NotifyParentProperty(true)]
        public string TableSeparatorCellCssClass
        {
            get
            {
                if (ViewState["TableSeparatorCellCssClass"] == null)
                {
                    ViewState["TableSeparatorCellCssClass"] = "";
                }
                return Convert.ToString(ViewState["TableSeparatorCellCssClass"]);
            }
            set
            {
                ViewState["TableSeparatorCellCssClass"] = value;
            }
        }


        /// <summary>
        /// DataTable object containing rows to be displayed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public DataTable DataSource
        {
            get
            {
                return mDataSource;
            }

            set
            {
                mDataSource = value;
            }
        }


        /// <summary>
        /// Params of the table to be displayed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), NotifyParentProperty(true)]
        public string[,] TableParams
        {
            get
            {
                return mTableParams;
            }

            set
            {
                mTableParams = value;
            }
        }


        /// <summary>
        /// Overrides the generation of the SPAN tag with custom tag.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return CMSControlsHelper.GetControlTagKey();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (TableParams == null)
            {
                Controls.Add(new LiteralControl(""));
                return;
            }

            int rowIndex;
            StringBuilder strBuild = new StringBuilder();

            strBuild.Append("<table");
            if (!String.IsNullOrEmpty(TableCssClass))
            {
                strBuild.Append(@" class=""");
                strBuild.Append(TableCssClass);
                strBuild.Append(@"""");
            }
            strBuild.Append(@" cellspacing=""");
            strBuild.Append(TableCellSpacing);
            strBuild.Append(@""" cellpadding=""");
            strBuild.Append(TableCellPadding);
            strBuild.Append(@""">");
            for (rowIndex = 0; rowIndex <= TableParams.GetUpperBound(0); rowIndex++)
            {
                strBuild.Append("<tr");
                if (TableParams[rowIndex, 1] == null & TableParams[rowIndex, 2] == null & TableParams[rowIndex, 3] == null)
                {
                    // this is separator                
                    if ((TableSeparatorRowCssClass != null) && (TableSeparatorRowCssClass.Trim() != null) && (TableSeparatorRowCssClass.Trim() != ""))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableSeparatorRowCssClass);
                        strBuild.Append(@"""");
                    }
                    strBuild.Append(@"><td colspan=""");
                    strBuild.Append((DataSource.Rows.Count + 1).ToString());
                    strBuild.Append(@"""");
                    if (TableSeparatorCellCssClass.Trim() + "" != "")
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableSeparatorCellCssClass);
                        strBuild.Append(@"""");
                    }
                    strBuild.Append(">");
                    strBuild.Append(TableParams[rowIndex, 0]);
                    strBuild.Append("</td>");
                }
                else
                {
                    // this is header or standard row
                    if (rowIndex == 0 & !String.IsNullOrEmpty(TableFirstRowCssClass))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableFirstRowCssClass);
                        strBuild.Append(@"""");
                    }
                    else if (!String.IsNullOrEmpty(TableRowCssClass))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableRowCssClass);
                        strBuild.Append(@"""");
                    }
                    strBuild.Append("><td");
                    if (rowIndex == 0 & !String.IsNullOrEmpty(TableFirstRowCellCSSClass))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableFirstRowCellCSSClass);
                        strBuild.Append(@"""");
                    }
                    else if (!String.IsNullOrEmpty(TableFirstColumnCellCssClass))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableFirstColumnCellCssClass);
                        strBuild.Append(@"""");
                    }
                    else if (!String.IsNullOrEmpty(TableCellCssClass))
                    {
                        strBuild.Append(@" class=""");
                        strBuild.Append(TableCellCssClass);
                        strBuild.Append(@"""");
                    }
                    strBuild.Append(">");
                    strBuild.Append(TableParams[rowIndex, 0]);
                    strBuild.Append("</td>");
                    if (DataSource != null)
                    {
                        // render columns with data
                        foreach (DataRow row in DataSource.Rows)
                        {
                            var dr = row;
                            strBuild.Append("<td");
                            if (rowIndex == 0 & !String.IsNullOrEmpty(TableFirstRowCellCSSClass))
                            {
                                strBuild.Append(@" class=""");
                                strBuild.Append(TableFirstRowCellCSSClass);
                                strBuild.Append(@"""");
                            }
                            else if (!String.IsNullOrEmpty(TableCellCssClass))
                            {
                                strBuild.Append(@" class=""");
                                strBuild.Append(TableCellCssClass);
                                strBuild.Append(@"""");
                            }
                            strBuild.Append(">");
                            if (dr[TableParams[rowIndex, 1]] == DBNull.Value | (dr[TableParams[rowIndex, 1]] != DBNull.Value && dr[TableParams[rowIndex, 1]].GetType().FullName.ToLowerCSafe() == "system.boolean" && RenderFalseValueUsingEmptyTemplate && Convert.ToBoolean(dr[TableParams[rowIndex, 1]]) == false))
                            {
                                // empty value template
                                if ((TableParams.GetUpperBound(1) > 2) && (TableParams[rowIndex, 3] != null) && (TableParams[rowIndex, 3] != ""))
                                {
                                    // use template to display the empty value
                                    strBuild.Append(ControlsHelper.ResolveMacros(TableParams[rowIndex, 3], dr, true));
                                }
                                else
                                {
                                    // display column value
                                    strBuild.Append("&nbsp;");
                                }
                            }
                            else
                            {
                                // non-empty value template
                                if ((TableParams.GetUpperBound(1) > 1) && (TableParams[rowIndex, 2] != null) && (TableParams[rowIndex, 2] != ""))
                                {
                                    // use template to display the non-empty value
                                    //ControlsHelper.ResolveMacros
                                    strBuild.Append(ControlsHelper.ResolveMacros(TableParams[rowIndex, 2], dr, true));
                                }
                                else
                                {
                                    // display column value
                                    strBuild.Append(Convert.ToString(dr[TableParams[rowIndex, 1]]));
                                }
                            }
                            strBuild.Append("</td>");
                        }
                    }
                }
                strBuild.Append("</tr>");
            }
            strBuild.Append("</table>");
            Controls.Add(new LiteralControl(strBuild.ToString()));
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            EnableViewState = false;
            if (Context == null)
            {
                output.Write("[ BasicMultiColumnTable \"" + ID + "\" ]");
            }
            else
            {
                base.Render(output);
            }
        }


        /// <summary>
        /// Processes template definition and replaces macros with their values.
        /// </summary>
        /// <param name="templateStr">Template string</param>
        /// <param name="dr">Datarow object with values</param>
        protected string ProcessTemplate(string templateStr, DataRow dr)
        {
            if (templateStr.IndexOfCSafe("{%") < 0)
            {
                return templateStr;
            }

            foreach (DataColumn dataColumn in dr.Table.Columns)
            {
                var col = dataColumn;
                string value;
                if (dr[col.ColumnName] != DBNull.Value)
                {
                    value = Convert.ToString(dr[col.ColumnName]);
                }
                else
                {
                    value = "";
                }
                templateStr = templateStr.Replace("{%" + Convert.ToString(dr[col.ColumnName]) + "%}", value);
            }
            return templateStr;
        }

        #endregion
    }
}