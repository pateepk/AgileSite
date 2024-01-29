using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Default layout
    /// </summary>
    public abstract class AbstractDefaultLayout : AbstractLayout
    {
        #region "Variables"

        private Dictionary<FormCategoryInfo, List<FormFieldInfo>> mFormElements = null;

        private readonly List<Tuple<string, LiteralControl>> mTooltipLiterals = new List<Tuple<string, LiteralControl>>();

        private int mTotalColumns = -1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates number of form columns.
        /// </summary>
        protected int TotalColumns
        {
            get
            {
                if (mTotalColumns == -1)
                {
                    switch (BasicForm.DefaultFieldLayout)
                    {
                        case FieldLayoutEnum.Inline:
                            mTotalColumns = 1;
                            break;

                        case FieldLayoutEnum.ThreeColumns:
                            mTotalColumns = 3;
                            break;

                        default:
                            mTotalColumns = 2;
                            break;
                    }

                    if (BasicForm.AllowEditVisibility)
                    {
                        mTotalColumns++;
                    }
                }

                return mTotalColumns;
            }
        }


        /// <summary>
        /// Gets visible form elements.
        /// </summary>
        protected Dictionary<FormCategoryInfo, List<FormFieldInfo>> FormElements
        {
            get
            {
                if ((mFormElements == null) && (BasicForm != null) && (BasicForm.FormInformation != null))
                {
                    mFormElements = BasicForm.FormInformation.GetHierarchicalFormElements(x => (x.Visible || BasicForm.FieldsToHide.Contains(x.Name)) && !(BasicForm.HideSystemFields && x.System) && (x.PublicField || BasicForm.ShowPrivateFields));
                }
                return mFormElements;
            }
        }


        /// <summary>
        /// Stores initiated categories(with translated captions).
        /// </summary>
        protected List<FormCategoryInfo> categories = new List<FormCategoryInfo>();


        /// <summary>
        /// Category list panel, may be displayed above formPanel.
        /// </summary>
        protected Panel CategoryListPanel
        {
            get;
            set;
        }

        #endregion


        #region "Constructor and public methods"

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="basicForm">Reference to BasicForm</param>
        /// <param name="categoryListPanel">Panel where category list will be placed</param>
        protected AbstractDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm)
        {
            CategoryListPanel = categoryListPanel;
        }


        /// <summary>
        /// Loads the default form layout from the given form information.
        /// </summary>
        public override void LoadLayout()
        {
            if (BasicForm.FormInformation != null)
            {
                Load();
            }
        }

        #endregion


        #region "Abstract and virtual methods"

        /// <summary>
        /// Loads layout based on specific type.
        /// </summary>
        protected virtual void Load()
        {
            foreach (KeyValuePair<FormCategoryInfo, List<FormFieldInfo>> category in (BasicForm.IsDesignMode ? FormElements : FormElements.Where(x => x.Value.Any())))
            {
                CreateCategory(category.Value, new FieldCreationArgs { FormCategoryInfo = category.Key });

                categories.Add(category.Key);
            }

            RegisterScripts();

            BasicForm.Page.PreRender += EnsureTooltips;
        }


        /// <summary>
        /// Renders whole category (iterates through fields and renders them).
        /// </summary>
        /// <param name="formFields">List of fields to render</param>
        /// <param name="args">Parameters needed for rendering</param>
        protected virtual void CreateCategory(List<FormFieldInfo> formFields, FieldCreationArgs args)
        {
            foreach (FormFieldInfo field in formFields)
            {
                BasicForm.AssociatedControls.Add(field, new List<Control>());
                args.FormFieldInfo = field;

                if (field.PublicField || BasicForm.ShowPrivateFields)
                {
                    // Create the field controls
                    CreateField(args);
                }
                else
                {
                    // Optionally create invisible field
                    CreateInvisibleField(args);
                }
            }
        }


        /// <summary>
        /// Creates the field.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected abstract void CreateField(FieldCreationArgs args);


        /// <summary>
        /// Creates invisible field with default value if field is not public and it is not allowed to be empty.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected virtual void CreateInvisibleField(FieldCreationArgs args)
        {
            // Create invisible field with default value if field is not public
            // and it is not allowed to be empty.
            if (!args.FormFieldInfo.AllowEmpty || !String.IsNullOrEmpty(args.FormFieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue)) && BasicForm.IsInsertMode)
            {
                // Create invisible field control
                Control ctrl = CreateEditingFormControl(args.FormFieldInfo);
                args.FormFieldInfo.Visible = false;
                ctrl.Visible = false;
                AddControlToPanel(ctrl, args.FormFieldInfo);

                // Create invisible field validation label
                FormPanel.Controls.Add(CreateErrorLabel(args.FormFieldInfo));
            }
        }


        /// <summary>
        /// Registers necessary JavaScripts.
        /// </summary>
        protected virtual void RegisterScripts()
        {

        }


        /// <summary>
        /// Gets identifier for anchor element.
        /// </summary>
        /// <param name="caption"></param>
        /// <returns></returns>
        protected virtual string GetAnchorID(string caption)
        {
            return "id" + caption.GetHashCode();
        }


        /// <summary>
        /// Creates submit panel.
        /// </summary>
        protected virtual Panel RegisterButtonPanel()
        {
            Panel buttonPanel = new Panel()
                {
                    CssClass = BasicForm.FormButtonPanelCssClass,
                    EnableViewState = false
                };

            FormPanel.Controls.Add(buttonPanel);
            buttonPanel.Controls.Add((Control)GetSubmitButton());

            BasicForm.FormButtonPanel = buttonPanel;

            return buttonPanel;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Adds the submit button to the form.
        /// </summary>
        protected IButtonControl GetSubmitButton()
        {
            if (BasicForm.ShowImageButton)
            {
                if (BasicForm.SubmitImageButton.Visible)
                {
                    FormPanel.DefaultButton = BasicForm.SubmitImageButton.ID;
                }
                return BasicForm.SubmitImageButton;
            }

            // Add button
            if (BasicForm.SubmitButton.Visible)
            {
                // Set default button
                FormPanel.DefaultButton = BasicForm.SubmitButton.ID;
            }
            return BasicForm.SubmitButton;
        }


        /// <summary>
        /// Gets row CSS class attribute for form field 
        /// </summary>
        /// <param name="args">Form field settings</param>
        /// <param name="extraClass">Additional CSS class to add to the attribute</param>
        protected string GetRowCssClassAttribute(FieldCreationArgs args, string extraClass = null)
        {
            string rowClass = extraClass;

            // Get field class
            if (args.FormFieldInfo != null)
            {
                rowClass = TextHelper.Merge(" ", rowClass, BasicForm.GetFieldCssClass(args.FormFieldInfo, FormFieldPropertyEnum.FieldCssClass));
            }

            // Get category class
            if ((args.FormCategoryInfo != null) && !String.IsNullOrEmpty(args.AnchorID) && ValidationHelper.GetBoolean(args.FormCategoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false))
            {
                rowClass = String.Format("{0} HiddenBy{1}", rowClass, args.AnchorID);
            }

            return CssHelper.GetCssClassAttribute(rowClass);
        }


        /// <summary>
        /// Returns caption from category info or default category name from parent BasicForm with resolved macros (data and/or localization).
        /// </summary>
        /// <param name="categoryInfo">Category info</param>
        protected string GetLocalizedCategoryCaption(FormCategoryInfo categoryInfo)
        {
            string caption = string.Empty;
            if (categoryInfo != null)
            {
                caption = categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Caption, BasicForm.ContextResolver);
            }
            caption = ResHelper.LocalizeString(string.IsNullOrEmpty(caption) ? BasicForm.DefaultCategoryName : caption);

            return caption;
        }


        /// <summary>
        /// Returns title attribute with encoded text or empty string if no title is defined.
        /// </summary>
        /// <param name="title">Title</param>
        protected string GetTitleAttribute(string title)
        {
            return string.IsNullOrEmpty(title) ? string.Empty : " title=\"" + HTMLHelper.HTMLEncode(title) + "\"";
        }


        /// <summary>
        /// Return literal control initialized with text that will be part of the form's layout and contain title attribute based on field's description.
        /// </summary>
        /// <param name="args">Container with objects necessary for form layout creation</param>
        /// <param name="layoutText">Layout text for literal; title attribute is put into the text on the place specified by placeholder "{0}"</param>
        /// <remarks>Creation of title attribute - tooltip (based on field's description) is postponed to form's PreRender event.</remarks>
        protected LiteralControl GetLiteralForLayoutWithTitle(FieldCreationArgs args, string layoutText)
        {
            var layoutLiteral = new LiteralControl(layoutText);

            // Store literal for later processing in form's PreRender event
            mTooltipLiterals.Add(new Tuple<string, LiteralControl>(args.FormFieldInfo.Name, layoutLiteral));

            return layoutLiteral;
        }

        #endregion


        #region "Private methods"

        private void EnsureTooltips(object sender, EventArgs e)
        {
            // Add title attributes to layout literals
            foreach (var item in mTooltipLiterals)
            {
                var field = BasicForm.FormInformation?.GetFormField(item.Item1);
                if (field != null)
                {
                    var tooltip = ResHelper.LocalizeString(field.GetPropertyValue(FormFieldPropertyEnum.FieldDescription, BasicForm.ContextResolver));

                    var literal = item.Item2;
                    if (literal != null)
                    {
                        literal.Text = String.Format(literal.Text, GetTitleAttribute(tooltip));
                    }
                }
            }
        }

        #endregion
    }
}