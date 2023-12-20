using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base class for table-based layouts.
    /// </summary>
    public abstract class AbstractTableDefaultLayout : AbstractDefaultLayout
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">Reference to BasicForm</param>
        /// <param name="categoryListPanel">Panel where category list will be placed</param>
        protected AbstractTableDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm, categoryListPanel)
        {
        }

        #endregion


        #region "Overrides"

        /// <summary>
        /// Creates the field.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected override void CreateField(FieldCreationArgs args)
        {
            if (BasicForm.FieldsToHide.Contains(args.FormFieldInfo.Name))
            {
                args.FormFieldInfo.Visible = false;
            }

            bool table = (BasicForm.DefaultFieldLayout != FieldLayoutEnum.Inline);

            if (table)
            {
                // Label cell
                string captionCssClass = BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.CaptionCellCssClass);
                var labelCellLiteral = GetLiteralForLayoutWithTitle(args, $"<td{CssHelper.GetCssClassAttribute(captionCssClass)}{{0}}>");
                AddControlToPanel(labelCellLiteral, args.FormFieldInfo);
            }
            else
            {
                // Single cell
                var singleCellLiteral = GetLiteralForLayoutWithTitle(args, "<td{0}>");
                AddControlToPanel(singleCellLiteral, args.FormFieldInfo);
            }

            // Create field label
            AddControlToPanel(CreateFieldLabel(args.FormFieldInfo, false), args.FormFieldInfo);

            if (table)
            {
                // Value cell
                string controlCellCssClass = BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.ControlCellCssClass);
                var valueCellLiteral = GetLiteralForLayoutWithTitle(args, $"</td><td{CssHelper.GetCssClassAttribute(controlCellCssClass)}{{0}}>");
                AddControlToPanel(valueCellLiteral, args.FormFieldInfo);
            }

            // Create the field control
            AddControlToPanel(CreateEditingFormControl(args.FormFieldInfo), args.FormFieldInfo);

            if (BasicForm.DefaultFieldLayout == FieldLayoutEnum.ThreeColumns)
            {
                // Error cell
                AddControlToPanel(new LiteralControl(String.Format("</td><td{0}>", CssHelper.GetCssClassAttribute(BasicForm.FieldErrorCellCssClass))), args.FormFieldInfo);
            }

            // Create the field validation label
            CreateErrorLabel(args.FormFieldInfo);

            // Append visibility control to allow user to change field's visibility
            if (BasicForm.AllowEditVisibility)
            {
                if (table)
                {
                    // Value cell
                    AddControlToPanel(new LiteralControl(String.Format("</td><td{0}>", CssHelper.GetCssClassAttribute(BasicForm.FieldVisibilityCellCssClass))), args.FormFieldInfo);
                }

                // Create the visibility control
                CreateVisibilityControl(args.FormFieldInfo);
            }

            AddControlToPanel(new LiteralControl("</td>"), args.FormFieldInfo);
        }

        #endregion
    }
}