using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Extended bound field for use within UniGrid.
    /// </summary>
    public class ExtendedBoundField : BoundField
    {
        /// <summary>
        /// Event raised when external source data required.
        /// </summary>
        public event OnExternalDataBoundEventHandler OnExternalDataBound;


        #region "Variables"

        /// <summary>
        /// Constant representing that all data will be provided to the field
        /// </summary>
        public const string ALL_DATA = "##ALL##";

        private string[] actionParameters;
        private bool mTooltipEncode = true;

        private UniGrid mUnigrid;

        private bool tooltipHooked;

        private StringSafeDictionary<ObjectTransformation> tooltips;
        private StringSafeDictionary<WebControl> tooltipControls;


        /// <summary>
        /// Constructor
        /// </summary>
        public ExtendedBoundField()
        {
            MaxLength = 0;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Item action parameters fields.
        /// </summary>
        public string ActionParameters
        {
            set
            {
                actionParameters = value.Split(';');
            }
        }


        /// <summary>
        /// Field hyperlink url.
        /// </summary>
        public string NavigateUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Field action.
        /// </summary>
        public string Action
        {
            get;
            set;
        }


        /// <summary>
        /// Command argument.
        /// </summary>
        public string CommandArgument
        {
            get;
            set;
        }


        /// <summary>
        /// Field hyperlink target.
        /// </summary>
        public string Target
        {
            get;
            set;
        }


        /// <summary>
        /// External source name.
        /// </summary>
        public string ExternalSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Item style.
        /// </summary>
        public string Style
        {
            get;
            set;
        }


        /// <summary>
        /// Localize the string in the displayed data?
        /// </summary>
        public bool LocalizeStrings
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip source name.
        /// </summary>
        public string TooltipSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip external source name.
        /// </summary>
        public string TooltipExternalSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Tooltip width.
        /// </summary>
        public string TooltipWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if tooltip should be encoded.
        /// </summary>
        public bool TooltipEncode
        {
            get
            {
                return mTooltipEncode;
            }
            set
            {
                mTooltipEncode = value;
            }
        }


        /// <summary>
        /// UniGrid column icon.
        /// </summary>
        public string Icon
        {
            get;
            set;
        }


        /// <summary>
        /// Field text max length.
        /// </summary>
        public int MaxLength
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Bind data event handler.
        /// </summary>
        protected override void OnDataBindField(object sender, EventArgs e)
        {
            bool allData = DataField.EqualsCSafe(ALL_DATA, StringComparison.InvariantCultureIgnoreCase);
            if (!allData)
            {
                base.OnDataBindField(sender, e);
            }

            DataControlFieldCell cell = (DataControlFieldCell)sender;
            GridViewRow container = (GridViewRow)cell.NamingContainer;

            // Get object tooltip
            object tooltip = GetTooltip(sender, container);
            WebControl tooltipControl = null;

            // Use external binding if set
            if (ExternalSourceName != null)
            {
                object data;

                if (!allData)
                {
                    data = OnExternalDataBound(sender, ExternalSourceName, ((DataRowView)container.DataItem)[DataField]);
                }
                else
                {
                    data = OnExternalDataBound(sender, ExternalSourceName, (container.DataItem));
                }
                if (data != null)
                {
                    cell.Controls.Clear();

                    if (data is Control)
                    {
                        // If is tooltip 
                        if (tooltip != null)
                        {
                            // If is icon set tooltip for image
                            if (Icon != null)
                            {
                                cell.Controls.Add((Control)data);
                                tooltipControl = AddIcon(cell);
                            }
                            // Set tooltip for object
                            else
                            {
                                // Create panel around the control
                                Panel panel = new Panel();
                                panel.EnableViewState = false;

                                panel.Controls.Add((Control)data);
                                cell.Controls.Add(panel);

                                tooltipControl = panel;
                            }
                        }
                        else
                        {
                            cell.Controls.Add((Control)data);
                        }
                    }
                    else
                    {
                        string text = PreProcessText(data.ToString(), false);

                        AddCellText(tooltip, cell, text, out tooltipControl);
                    }
                }
                else
                {
                    cell.Text = PreProcessText(cell.Text, false);
                }
            }
            else
            {
                // Prepare the text
                string text = String.Empty;
                if (!allData)
                {
                    text = PreProcessText(((DataRowView)container.DataItem)[DataField].ToString());
                }

                AddCellText(tooltip, cell, text, out tooltipControl);
            }

            // Create link
            if ((NavigateUrl != null) || (Action != null))
            {
                HyperLink link = new HyperLink();
                link.EnableViewState = false;

                if (Action != null)
                {
                    DataRowView drv = ((DataRowView)container.DataItem);

                    // Process command argument
                    string commandArgument = CommandArgument;
                    if (String.IsNullOrEmpty(commandArgument))
                    {
                        // First found column
                        commandArgument = drv[0].ToString();
                    }
                    else
                    {
                        string columnName = commandArgument;
                        if (drv.Row.Table.Columns.Contains(columnName))
                        {
                            // Regular column
                            commandArgument = drv[columnName].ToString();
                        }
                        else
                        {
                            // Generated with parameters
                            commandArgument = ApplyParameters(commandArgument, container);
                        }
                    }

                    // Init the unigrid control
                    if (mUnigrid == null)
                    {
                        mUnigrid = (UniGrid)ControlsHelper.GetParentControl(cell, typeof(UniGrid));
                    }

                    // Add to the allowed actions
                    if (!mUnigrid.ActionsID.Contains(commandArgument))
                    {
                        mUnigrid.ActionsID.Add(commandArgument);
                    }

                    // Grid action
                    link.Attributes.Add("onclick", "return " + mUnigrid.GetJSModule() + ".command(" + ScriptHelper.GetString(Action) + ", " + ScriptHelper.GetString(commandArgument) + ");");
                    link.Attributes.Add("href", "#");
                }
                else
                {
                    // Standard hyperlink
                    if (Target != null)
                    {
                        link.Target = Target;
                    }
                    link.NavigateUrl = cell.ResolveUrl(ApplyParameters(NavigateUrl, container));
                }

                // If cell contains controls move it into hyperlink
                if (cell.Controls.Count > 0)
                {
                    foreach (Control ctrl in cell.Controls)
                    {
                        link.Controls.Add(ctrl);
                    }
                    cell.Controls.Clear();
                    cell.Controls.Add(link);
                }
                else
                {
                    link.Text = PreProcessText(cell.Text);
                }
            }

            // Icon
            if ((Icon != null) && (tooltip == null))
            {
                AddIcon(cell);
            }

            if (tooltip != null)
            {
                SetTooltip(tooltip, tooltipControl);
            }

            // Apply the style
            if (!String.IsNullOrEmpty(Style))
            {
                cell.Style.Value = Style;
            }
        }


        /// <summary>
        /// Pre-processes source text based on settings.
        /// </summary>
        private string PreProcessText(string source, bool encode = true)
        {
            string result = source;

            if (LocalizeStrings)
            {
                result = ResHelper.LocalizeString(result);
            }

            if (encode)
            {
                result = HTMLHelper.HTMLEncode(result);
            }

            // Ensure max length if defined
            if (MaxLength > 0)
            {
                result = TextHelper.LimitLength(result, MaxLength);
            }

            return result;
        }


        /// <summary>
        /// Adds label to given field cell.
        /// </summary>
        private WebControl AddLabel(WebControl container, string text)
        {
            Label label = new Label();
            label.EnableViewState = false;
            label.Text = text;

            container.Controls.Add(label);

            return label;
        }


        /// <summary>
        /// Adds icon to given field cell.
        /// </summary>
        private WebControl AddIcon(WebControl container)
        {
            Image icon = new Image();
            icon.ImageUrl = Icon;
            icon.EnableViewState = false;

            container.Controls.Add(new LiteralControl("&nbsp;"));
            container.Controls.Add(icon);

            return icon;
        }


        /// <summary>
        /// Adds plain text or text with tooltip and icon to given field cell.
        /// </summary>
        private void AddCellText(object tooltip, WebControl container, string text, out WebControl tooltipControl)
        {
            tooltipControl = null;

            if (tooltip == null)
            {
                // Add basic literal control
                container.Controls.Add(new LiteralControl(text));
            }
            else
            {
                // Create label with text from datacolumn
                tooltipControl = AddLabel(container, text);

                if (Icon != null)
                {
                    // Create icon image
                    tooltipControl = AddIcon(container);
                }
            }
        }


        /// <summary>
        /// Applies the action parameters to the given string.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="container">Container with the data</param>
        private string ApplyParameters(string text, GridViewRow container)
        {
            if (actionParameters != null)
            {
                string result = text;
                int index = 0;
                DataRowView rowView = (DataRowView)container.DataItem;
                foreach (string param in actionParameters)
                {
                    result = result.Replace("{" + index.ToString() + "}", rowView[param].ToString());
                    index++;
                }
                return result;
            }
            else
            {
                return text;
            }
        }


        /// <summary>
        /// Returns tooltip object.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="container">Grid view row</param>
        private object GetTooltip(object sender, GridViewRow container)
        {
            // If is set tooltip source
            if (TooltipSourceName != null)
            {
                bool allData = TooltipSourceName.EqualsCSafe(ALL_DATA, StringComparison.InvariantCultureIgnoreCase);

                // If is set external source name for tooltip
                if (TooltipExternalSourceName != null)
                {
                    if (!allData)
                    {
                        // Get tooltip from external data bound
                        return OnExternalDataBound(sender, TooltipExternalSourceName, ((DataRowView)container.DataItem)[TooltipSourceName]);
                    }

                    // Get tooltip from external data bound
                    return OnExternalDataBound(sender, TooltipExternalSourceName, (container.DataItem));
                }

                if (!allData)
                {
                    // Get tooltip from data row
                    return ((DataRowView)container.DataItem)[TooltipSourceName];
                }
            }
            return null;
        }


        /// <summary>
        /// Sets tooltip to webcontrol object.
        /// </summary>
        private void SetTooltip(object tooltip, WebControl tooltipControl)
        {
            if (tooltip == null || tooltipControl == null)
            {
                return;
            }

            var tooltipObjectTransformation = tooltip as ObjectTransformation;
            if (tooltipObjectTransformation != null)
            {
                if ((tooltipControl.Page != null) && !tooltipHooked)
                {
                    // Hook on webcontrol's PreRender to ensure data if ObjectTransformation object is used as a tooltip
                    tooltipControl.Page.PreRenderComplete += Page_PreRenderComplete;

                    tooltipHooked = true;
                }

                EnsureDictionaries();

                // Store tooltip for later processing (for prerender complete of the page)
                string key = tooltipControl.ClientID;
                tooltips.Add(key, tooltipObjectTransformation);
                tooltipControls.Add(key, tooltipControl);
            }
            else
            {
                // Set tooltip javascript
                AppendTooltip(tooltipControl, tooltip.ToString());
            }
        }


        /// <summary>
        /// Ensures tooltip dictionaries.
        /// </summary>
        private void EnsureDictionaries()
        {
            if (tooltips == null)
            {
                tooltips = new StringSafeDictionary<ObjectTransformation>();
                tooltipControls = new StringSafeDictionary<WebControl>();
            }
        }


        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            foreach (string key in tooltipControls.Keys)
            {
                ObjectTransformation tooltip = tooltips[key];
                WebControl tooltipControl = tooltipControls[key];

                if (tooltip != null && tooltipControl != null)
                {
                    // Set tooltip javascript
                    AppendTooltip(tooltipControl, tooltip.ToString());
                }
            }
        }


        private void AppendTooltip(WebControl control, string tooltip)
        {
            if (LocalizeStrings)
            {
                // Localize tooltip
                tooltip = ResHelper.LocalizeString(tooltip);
            }

            ScriptHelper.AppendTooltip(control, tooltip, "help", ValidationHelper.GetInteger(TooltipWidth, 0), TooltipEncode);
        }
    }
}
