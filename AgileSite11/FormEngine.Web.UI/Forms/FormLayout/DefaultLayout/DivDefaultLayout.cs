using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Divs layout
    /// </summary>
    public class DivDefaultLayout : AbstractDefaultLayout
    {
        #region "Constructor and overriding methods"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">BasicForm with settings</param>
        /// <param name="categoryListPanel">Panel where categories will be placed</param>
        public DivDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm, categoryListPanel)
        {
            RequiredMarkCssClass = "required-mark";
        }


        /// <summary>
        /// Loads field sets layout.
        /// </summary>
        protected override void Load()
        {
            base.Load();

            RegisterButtonPanel();
        }


        /// <summary>
        /// Renders whole category (iterates through fields and renders them).
        /// </summary>
        /// <param name="formFields">List of fields to render</param>
        /// <param name="args">Parameters needed for rendering</param>
        protected override void CreateCategory(List<FormFieldInfo> formFields, FieldCreationArgs args)
        {
            FormCategoryInfo categoryInfo = args.FormCategoryInfo;

            string categoryId = "category_" + ValidationHelper.GetCodeName(categoryInfo.CategoryName).TrimEnd();

            // Prepare category CSS class
            string cssClass = String.Format("{0} {1}", BasicForm.GroupCssClass, categoryId);

            bool collapsible = ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false);

            // Disable collapsibility in design mode
            collapsible &= !BasicForm.IsDesignMode;

            if (collapsible)
            {
                cssClass += " collapsible-div";

                if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.CollapsedByDefault, BasicForm.ContextResolver), false))
                {
                    cssClass += " Collapsed";
                }
            }

            string caption = GetLocalizedCategoryCaption(categoryInfo);
            args.AnchorID = GetAnchorID(caption);

            // Create category element
            AddControlToPanel(new LiteralControl(String.Format("<div id=\"{0}\"{1}>", args.AnchorID, CssHelper.GetCssClassAttribute(cssClass))), categoryInfo);

            if (!String.IsNullOrEmpty(caption) || collapsible)
            {
                var heading = new FormCategoryHeading
                {
                    Text = caption,
                    Level = BasicForm.FieldGroupHeadingLevel,
                    CssClass = BasicForm.FieldGroupCaptionCssClass,
                    IsAnchor = BasicForm.FieldGroupHeadingIsAnchor
                };

                if (collapsible && !String.IsNullOrEmpty(categoryInfo.CategoryName))
                {
                    heading.Attributes.Add("onclick", "ToggleDiv(this); return false;");

                    CollapsibleImage collapsibleImage = CreateCollapsibleImage(categoryInfo, args.AnchorID, false);

                    // Add button for collapsing category
                    AddControlToPanel(collapsibleImage, categoryInfo);

                    // Set corresponding image, CSS class and action
                    collapsibleImage.CssClass = "toggle-image";
                    collapsibleImage.Attributes.Add("onclick", "ToggleDiv(this); return false;");
                    collapsibleImage.ImageUrl = collapsibleImage.Collapsed ? BasicForm.ExpandCategoryImageUrl : BasicForm.CollapseCategoryImageUrl;
                    args.CollapsibleImage = collapsibleImage;
                }

                // Create category title element
                AddControlToPanel(heading, categoryInfo);
            }

            // Hide the table if category is collapsed
            string rowStyle = String.Empty;
            if ((args.CollapsibleImage != null) && args.CollapsibleImage.Collapsed)
            {
                rowStyle = " style=\"display: none;\"";
            }

            // Create div element around all fields in category
            AddControlToPanel(new LiteralControl(String.Format("<div{0}{1}>", rowStyle, CssHelper.GetCssClassAttribute(BasicForm.FieldGroupCssClass))), categoryInfo);

            // Control fields
            base.CreateCategory(formFields, args);

            // Close elements
            AddControlToPanel(new LiteralControl("</div></div>"), categoryInfo);
        }


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

            UpdatePanel pnlUpdate = null;

            string fieldName = "field_" + args.FormFieldInfo.Name;
            string fieldCssClass = BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.FieldCssClass);

            if (BasicForm.IsDesignMode)
            {
                // Create update panel
                pnlUpdate = new UpdatePanel();
                pnlUpdate.ID = fieldName;
                pnlUpdate.Attributes.Add("class", fieldCssClass);
                pnlUpdate.ClientIDMode = ClientIDMode.Static;
                pnlUpdate.ChildrenAsTriggers = true;
                pnlUpdate.UpdateMode = UpdatePanelUpdateMode.Conditional;
                BasicForm.FieldUpdatePanels[args.FormFieldInfo.Name] = pnlUpdate;

                // Add field wrapper (used for field tooltip)
                var fieldWrapperOpenTagLiteral = GetLiteralForLayoutWithTitle(args, $"<div{CssHelper.GetCssClassAttribute("field-wrapper")}{{0}}>");
                AddControl(fieldWrapperOpenTagLiteral, args.FormFieldInfo, pnlUpdate);
            }
            else
            {
                // Start div tag
                var fieldOpenTagLiteral = GetLiteralForLayoutWithTitle(args, $"<div id=\"{fieldName}\"{CssHelper.GetCssClassAttribute(fieldCssClass)}{{0}}>");
                AddControlToPanel(fieldOpenTagLiteral, args.FormFieldInfo);
            }

            bool isInline = BasicForm.DefaultFieldLayout == FieldLayoutEnum.Inline;

            if (!isInline)
            {
                string captionCellCssClass = BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.CaptionCellCssClass);
                string captionCellOpenTag = String.Format("<div{0}>", CssHelper.GetCssClassAttribute(captionCellCssClass));
                AddControl(new LiteralControl(captionCellOpenTag), args.FormFieldInfo, pnlUpdate);
            }

            // Create field label
            AddControl(CreateFieldLabel(args.FormFieldInfo, false), args.FormFieldInfo, pnlUpdate);

            if (!isInline)
            {
                // Value cell
                string controlCellCssClass = BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.ControlCellCssClass);
                AddControl(new LiteralControl(String.Format("</div><div{0}>", CssHelper.GetCssClassAttribute(controlCellCssClass))), args.FormFieldInfo, pnlUpdate);
            }

            // Create the field control
            AddControl(CreateEditingFormControl(args.FormFieldInfo), args.FormFieldInfo, pnlUpdate);

            if (BasicForm.DefaultFieldLayout == FieldLayoutEnum.ThreeColumns)
            {
                // Error cell
                AddControl(new LiteralControl(String.Format("</div><div{0}>", CssHelper.GetCssClassAttribute(BasicForm.FieldErrorCellCssClass))), args.FormFieldInfo, pnlUpdate);
            }

            // Create the field validation label
            CreateErrorLabel(args.FormFieldInfo);

            // Append visibility control to allow user to change field's visibility
            if (BasicForm.AllowEditVisibility)
            {
                if (!isInline)
                {
                    // Value cell
                    LiteralControl visibilityControlCell = new LiteralControl(String.Format("</div><div{0}>", CssHelper.GetCssClassAttribute(BasicForm.FieldVisibilityCellCssClass)));
                    AddControl(visibilityControlCell, args.FormFieldInfo, pnlUpdate);
                }

                // Create the visibility control
                CreateVisibilityControl(args.FormFieldInfo);
            }

            // Add additional field actions in design mode
            if (BasicForm.IsDesignMode)
            {
                if (!isInline)
                {
                    AddControl(new LiteralControl(String.Format("</div><div{0}>", CssHelper.GetCssClassAttribute("field-actions"))), args.FormFieldInfo, pnlUpdate);
                }
                AddControl(CreateFieldActions(args.FormFieldInfo), args.FormFieldInfo, pnlUpdate);
            }

            if (!isInline)
            {
                // Value cell
                AddControl(new LiteralControl("</div>"), args.FormFieldInfo, pnlUpdate);
            }

            if (BasicForm.IsDesignMode)
            {
                // Close field wrapper
                AddControl(new LiteralControl("</div>"), args.FormFieldInfo, pnlUpdate);

                AddControlToPanel(pnlUpdate, args.FormFieldInfo);
            }
            else
            {
                // Close div around the field
                AddControlToPanel(new LiteralControl("</div>"), args.FormFieldInfo);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds control to update panel in design mode or directly to the panel otherwise.
        /// </summary>
        /// <param name="control">Control to add</param>
        /// <param name="parentControl">IField to which this control belongs to</param>
        /// <param name="updatePanel">Update panel to which the control will be added in design mode</param>
        private void AddControl(Control control, IDataDefinitionItem parentControl, UpdatePanel updatePanel)
        {
            if (BasicForm.IsDesignMode)
            {
                updatePanel.ContentTemplateContainer.Controls.Add(control);
            }
            else
            {
                AddControlToPanel(control, parentControl);
            }
        }


        /// <summary>
        /// Registers script for layout.
        /// </summary>
        protected override void RegisterScripts()
        {
            string script = String.Format(@"
function ToggleDiv(label){{
    var div = $cmsj(label).parent();    
    var image = div.find('.ToggleImage');    
    var hiddenField = image.next();
    
    var affectedElements =  div.find('.editing-form-category-caption').next();
    if(hiddenField.val() == 'True')
    {{
        div.removeClass('Collapsed');
        hiddenField.val('False')
        image.attr('src', '{0}');
        affectedElements.show();
    }}
    else
    {{
        div.addClass('Collapsed');
        hiddenField.val('True');
        image.attr('src', '{1}');
        affectedElements.hide();
    }}
}}", BasicForm.CollapseCategoryImageUrl, BasicForm.ExpandCategoryImageUrl);

            ScriptHelper.RegisterJQuery(BasicForm.Page);
            ScriptHelper.RegisterClientScriptBlock(BasicForm, typeof(string), "hidingScriptDivs", ScriptHelper.GetScript(script));

            if (BasicForm.AutomaticLabelWidth)
            {
                script = @"
function RecalculateFormWidth(formId){
	var widestLabelWidth = 0;
	$cmsj(""#"" + formId + "" .EditingFormLabelCell"").each(function(index, value){
        value.style.width = null;
		if (widestLabelWidth < value.offsetWidth) {
			widestLabelWidth = value.offsetWidth;
		}
	});

	$cmsj(""#"" + formId + "" .EditingFormLabelCell"").each(function(index, value){
        // IE returns 1px smaller value than width really is
        widestLabelWidth += 1;
		value.style.width = widestLabelWidth + ""px"";
	});

    $cmsj(""#"" + formId + "" input[type=submit]"").css(""margin-left"", widestLabelWidth);
}";

                ScriptHelper.RegisterClientScriptBlock(BasicForm, typeof(string), "RecalculateFormWidth", ScriptHelper.GetScript(script));

                script = String.Format("RecalculateFormWidth('{0}');", FormPanel.ClientID);
                ScriptHelper.RegisterStartupScript(BasicForm, typeof(string), "recalculateFormWidth" + BasicForm.UniqueID, ScriptHelper.GetScript(script));
            }
        }

        #endregion
    }
}