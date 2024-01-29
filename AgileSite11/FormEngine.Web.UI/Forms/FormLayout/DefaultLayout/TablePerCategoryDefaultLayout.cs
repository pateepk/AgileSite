using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Tables layout - each category single table
    /// </summary>
    public class TablePerCategoryDefaultLayout : AbstractTableDefaultLayout
    {
        #region "Overriding methods and constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">BasicForm with settings</param>
        /// <param name="categoryListPanel">Panel where categories will be placed</param>
        public TablePerCategoryDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm, categoryListPanel)
        { }


        /// <summary>
        /// Loads table layout.
        /// </summary>
        protected override void Load()
        {
            base.Load();

            // Table for submit button
            CreateSubmitButton();
        }

        #endregion


        #region "Methods for creating controls"

        /// <summary>
        /// Registers script on the page.
        /// </summary>
        protected override void RegisterScripts()
        {
            string script = @"
function ToggleCategory(e)
{
 var image = $cmsj(e).find('.ToggleImage')
        var hiddenField = $cmsj(image).next()
        var context =  $cmsj(e).closest('" + BasicForm.GroupCssClass + @"')
        var affectedElements =  $cmsj('.HiddenBy' + context[0].id, context)
        if(hiddenField.val() != 'True')
        {{
            hiddenField.val('True')
            image.attr('src','" + BasicForm.ExpandCategoryImageUrl + @"')
            affectedElements.hide()
        }}
        else
        {{
            hiddenField.val('False')
            image.attr('src', '" + BasicForm.CollapseCategoryImageUrl + @"')
            affectedElements.show()
        }}
}

$cmsj(document).on('click', '.EditingFormCategoryRow.Toggle', function(e) {ToggleCategory(e.currentTarget);});";

            ScriptHelper.RegisterJQuery(BasicForm.Page);
            ScriptHelper.RegisterClientScriptBlock(BasicForm, typeof(string), "hidingScriptTables", ScriptHelper.GetScript(script));
        }


        /// <summary>
        /// Renders whole category (iterates through fields and renders them).
        /// </summary>
        /// <param name="formFields">List of fields to render</param>
        /// <param name="args">Parameters needed for rendering</param>
        protected override void CreateCategory(List<FormFieldInfo> formFields, FieldCreationArgs args)
        {
            // Init properties
            FormCategoryInfo categoryInfo = args.FormCategoryInfo;

            string categoryName = "category_" + ValidationHelper.GetCodeName(categoryInfo.CategoryName).TrimEnd();
            string caption = GetLocalizedCategoryCaption(categoryInfo);
            args.AnchorID = GetAnchorID(caption);
            string cssClass = String.Format("{0} {1}", BasicForm.GroupCssClass, categoryName);
            AddControlToPanel(new LiteralControl(String.Format("<table class=\"{0}\" style=\"border-collapse: collapse;\" cellspacing=\"0\" id=\"{1}\">", cssClass, args.AnchorID)), categoryInfo);

            // Create header row
            LiteralControl headerRow = new LiteralControl(String.Format("<tr class=\"{0}\">", BasicForm.FieldGroupCaptionCssClass));
            AddControlToPanel(headerRow, categoryInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormLeftBorder", Text = "&nbsp;" }, categoryInfo);
            var headingCell = new TableCell
            {
                ColumnSpan = TotalColumns,
                CssClass = "EditingFormCategory"
            };
            headingCell.Controls.Add(new FormCategoryHeading { Text = caption, Level = BasicForm.FieldGroupHeadingLevel, IsAnchor = BasicForm.FieldGroupHeadingIsAnchor });
            AddControlToPanel(headingCell, categoryInfo);

            // Create image for collapsing category
            TableCell imageCell = new TableCell { CssClass = "EditingFormRightBorder", Text = "&nbsp;" };

            AddControlToPanel(imageCell, categoryInfo);
            if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false))
            {
                headerRow.Text = headerRow.Text.Replace("\">", " Toggle\">");
                CollapsibleImage collapsibleImage = CreateCollapsibleImage(categoryInfo, args.AnchorID, BasicForm.ForceReloadCategories);
                imageCell.Controls.Add(collapsibleImage);
                collapsibleImage.ImageUrl = (collapsibleImage.Collapsed ? BasicForm.ExpandCategoryImageUrl : BasicForm.CollapseCategoryImageUrl);
                args.CollapsibleImage = collapsibleImage;
            }

            AddControlToPanel(new LiteralControl("</tr>"), categoryInfo);

            // Create controls
            base.CreateCategory(formFields, args);

            // Add footer
            CreateFooterRow(categoryInfo, args.AnchorID, args.CollapsibleImage);

            AddControlToPanel(new LiteralControl("</table>"), categoryInfo);
        }


        /// <summary>
        /// Create table for submit button.
        /// </summary>
        private void CreateSubmitButton()
        {
            var submitTable = new Table { CssClass = "EditingFormButtonTable", CellSpacing = 0, ID = "SubmitTable" };
            AddControlToPanel(submitTable, null);

            var submitRow = new TableRow { CssClass = "EditingFormButtonRow", ID = "SubmitRow" };
            submitTable.Rows.Add(submitRow);
            submitRow.Cells.Add(new TableCell { CssClass = "EditingFormLeftBorder", Text = "&nbsp;", ID = "SubmitLeftBorder" });
            submitRow.Cells.Add(new TableCell { CssClass = "EditingFormButtonLeftCell", Text = "&nbsp;", ID = "SubmitLeftCell" });

            var submitCell = new TableCell { CssClass = "EditingFormButtonCell", ID = "SubmitCell" };
            submitRow.Cells.Add(submitCell);

            submitCell.Controls.Add((Control)GetSubmitButton());
            submitRow.Cells.Add(new TableCell { CssClass = "EditingFormRightBorder", Text = "&nbsp;", ID = "SubmitRightBorder" });
        }


        /// <summary>
        /// Creates the field.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected override void CreateField(FieldCreationArgs args)
        {
            // Add style to hide or show the row depending on +- button
            string rowStyle = String.Empty;
            if ((args.CollapsibleImage != null) && args.CollapsibleImage.Collapsed)
            {
                rowStyle += " display: none;";
            }

            // Get row class
            string rowAttribute = GetRowCssClassAttribute(args, BasicForm.FieldCssClass);

            AddControlToPanel(new LiteralControl(String.Format("<tr{0}" + (string.IsNullOrEmpty(rowStyle) ? "" : " style=\"{1}\"") + ">", rowAttribute, rowStyle)), args.FormFieldInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormLeftBorder", Text = "&nbsp;" }, args.FormFieldInfo);

            base.CreateField(args);

            // Single cell
            AddControlToPanel(new TableCell { CssClass = "EditingFormRightBorder", Text = "&nbsp;" }, args.FormFieldInfo);
            AddControlToPanel(new LiteralControl("</tr>"), args.FormFieldInfo);
        }


        /// <summary>
        /// Creates footer row.
        /// </summary>
        private void CreateFooterRow(FormCategoryInfo categoryInfo, string anchor, CollapsibleImage collapsibleImage)
        {
            string collapsibleClass = string.Empty;
            string rowStyle = String.Empty;
            if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false))
            {
                collapsibleClass += " HiddenBy" + anchor;
                if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.CollapsedByDefault, BasicForm.ContextResolver), false))
                {
                    collapsibleClass += " HiddenRow";
                }

                if ((collapsibleImage != null) && collapsibleImage.Collapsed)
                {
                    rowStyle += " display: none;";
                    collapsibleImage.ImageUrl = BasicForm.ExpandCategoryImageUrl;
                }
            }
            AddControlToPanel(new LiteralControl(String.Format("<tr class=\"EditingFormFooterRow {0}\" style=\"{1}\">", collapsibleClass, rowStyle)), categoryInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormLeftBorder", Text = "&nbsp;" }, categoryInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormLabelCell", Text = "&nbsp;" }, categoryInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormValueCell", Text = "&nbsp;" }, categoryInfo);
            AddControlToPanel(new TableCell { CssClass = "EditingFormRightBorder", Text = "&nbsp;" }, categoryInfo);
            AddControlToPanel(new LiteralControl("</tr>"), categoryInfo);
        }

        #endregion
    }
}