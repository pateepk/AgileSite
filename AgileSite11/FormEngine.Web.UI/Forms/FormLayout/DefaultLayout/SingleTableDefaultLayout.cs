using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Standard layout - single table
    /// </summary>
    public class SingleTableDefaultLayout : AbstractTableDefaultLayout
    {
        #region "Constructor and overriding methods"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">BasicForm with settings</param>
        /// <param name="categoryListPanel">Panel where categories will be placed</param>
        public SingleTableDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm, categoryListPanel)
        { }


        /// <summary>
        /// Loads standard layout.
        /// </summary>
        protected override void Load()
        {
            InitImages();

            // Begin of the form control
            AddControlToPanel(new LiteralControl("<table class=\"EditingFormTable\">"), null);

            base.Load();

            // Begin of the form control
            CreateSubmitRow();
            AddControlToPanel(new LiteralControl("</table>"), null);

            BasicForm.ForceReloadCategories = false;
        }


        /// <summary>
        /// Renders whole category (iterates through fields and renders them).
        /// </summary>
        /// <param name="formFields">List of fields to render</param>
        /// <param name="args">Parameters needed for rendering</param>
        protected override void CreateCategory(List<FormFieldInfo> formFields, FieldCreationArgs args)
        {
            FormCategoryInfo categoryInfo = args.FormCategoryInfo;
            BasicForm.AssociatedControls.Add(categoryInfo, new List<Control>());

            string caption = GetLocalizedCategoryCaption(categoryInfo);
            args.AnchorID = GetAnchorID(caption);
            if (!string.IsNullOrEmpty(caption))
            {
                string categoryName = String.Format("category_{0}", ValidationHelper.GetCodeName(categoryInfo.CategoryName));
                string rowStyle = String.Format("{0} {1}", BasicForm.GroupCssClass, categoryName).TrimEnd();
                AddControlToPanel(new LiteralControl("<tr class=\"" + rowStyle + "\" id=\"" + args.AnchorID + "\"><td colspan=\"" + TotalColumns + "\">" + caption), categoryInfo);

                if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false))
                {
                    CollapsibleImage collapsibleImage = CreateCollapsibleImage(categoryInfo, args.AnchorID, BasicForm.ForceReloadCategories);
                    AddControlToPanel(collapsibleImage, categoryInfo);
                    collapsibleImage.ImageUrl = (collapsibleImage.Collapsed ? BasicForm.ExpandCategoryImageUrl : BasicForm.CollapseCategoryImageUrl);
                    collapsibleImage.AddCssClass("StandardLayout");
                    collapsibleImage.Attributes.Add("onclick", "ToggleCategory(this);");
                    args.CollapsibleImage = collapsibleImage;
                }

                AddControlToPanel(new LiteralControl("</td></tr>"), categoryInfo);
            }

            // Fields in the category
            base.CreateCategory(formFields, args);
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
                rowStyle = " style=\"display: none;\"";
            }

            string rowAttribute = GetRowCssClassAttribute(args);
            AddControlToPanel(new LiteralControl(String.Format("<tr id=\"{0}\"{1}{2}>", args.FormFieldInfo.Name, rowAttribute, rowStyle)), args.FormFieldInfo);

            base.CreateField(args);

            AddControlToPanel(new LiteralControl("</tr>"), args.FormFieldInfo);
        }


        /// <summary>
        /// Inits basicform's collapse/expand images.
        /// </summary>
        private void InitImages()
        {
            BasicForm.CollapseCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_Form/collapsecategory.png");
            BasicForm.ExpandCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_Form/expandcategory.png");
        }

        #endregion


        #region "Protected virtual methods"

        /// <summary>
        /// Appends submit row to the end of the table.
        /// </summary>
        protected virtual void CreateSubmitRow()
        {
            AddControlToPanel(new LiteralControl("<tr><td class=\"EditingFormButtonLeftCell\"></td><td class=\"EditingFormButtonCell\">"), null);
            AddControlToPanel((Control)GetSubmitButton(), null);
            AddControlToPanel(new LiteralControl("</td></tr>"), null);
        }


        /// <summary>
        /// Register scripts used by layout.
        /// </summary>
        protected override void RegisterScripts()
        {
            if (categories.Exists(x => ValidationHelper.GetBoolean(x.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false)))
            {
                string script = String.Format(@"
function ToggleCategory(element){{
    var label = $cmsj(element);
    var context = label.closest('.EditingFormTable');
    var image = label.find('.ToggleImage');
    var affectedElements = $cmsj('.HiddenBy' + image.attr('alt'), context);
    var hiddenField = image.next();

    if(hiddenField.val() != 'True')
    {{
            hiddenField.val('True')
            image.attr('src','{0}')
            affectedElements.hide()
    }}
    else
    {{ 
            hiddenField.val('False')
            image.attr('src', '{1}')
            affectedElements.show()
    }}
}}", BasicForm.ExpandCategoryImageUrl, BasicForm.CollapseCategoryImageUrl);

                ScriptHelper.RegisterJQuery(BasicForm.Page);
                ScriptHelper.RegisterClientScriptBlock(BasicForm, typeof(string), "hidingScriptStandard", ScriptHelper.GetScript(script));
            }
        }

        #endregion
    }
}