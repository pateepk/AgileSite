using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Panels layout - each category single panel
    /// </summary>
    public class FieldSetDefaultLayout : AbstractTableDefaultLayout
    {
        #region "Constructor and overriding methods"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="basicForm">BasicForm with settings</param>
        /// <param name="categoryListPanel">Panel where categories will be placed</param>
        public FieldSetDefaultLayout(BasicForm basicForm, Panel categoryListPanel)
            : base(basicForm, categoryListPanel)
        {
        }


        /// <summary>
        /// Loads field sets layout.
        /// </summary>
        protected override void Load()
        {
            InitImages();

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
            string categoryName = "category_" + ValidationHelper.GetCodeName(categoryInfo.CategoryName).TrimEnd();
            string cssClass = String.Format("{0} {1}", BasicForm.GroupCssClass, categoryName).TrimEnd();

            bool collapsible = ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false);
            if (collapsible)
            {
                cssClass += " CollapsibleFieldSet";

                if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.CollapsedByDefault, BasicForm.ContextResolver), false))
                {
                    cssClass += " Collapsed";
                }
            }

            string caption = GetLocalizedCategoryCaption(categoryInfo);
            args.AnchorID = GetAnchorID(caption);

            string collapsibleOnClick = collapsible ? " onclick=\"ToggleFieldSet(this)\"" : String.Empty;
            AddControlToPanel(new LiteralControl("<fieldset class=\"" + cssClass + "\" id=\"" + args.AnchorID + "\"><legend" + collapsibleOnClick + ">" + caption), categoryInfo);

            if (ValidationHelper.GetBoolean(categoryInfo.GetPropertyValue(FormCategoryPropertyEnum.Collapsible, BasicForm.ContextResolver), false))
            {
                CollapsibleImage collapsibleImage = CreateCollapsibleImage(categoryInfo, args.AnchorID, false);
                AddControlToPanel(collapsibleImage, categoryInfo);
                // Set appropriate image
                collapsibleImage.ImageUrl = collapsibleImage.Collapsed ? BasicForm.ExpandCategoryImageUrl : BasicForm.CollapseCategoryImageUrl;
                args.CollapsibleImage = collapsibleImage;
            }

            // Hide the table if category is collapsed
            string rowStyle = String.Empty;
            if ((args.CollapsibleImage != null) && args.CollapsibleImage.Collapsed)
            {
                rowStyle = " style=\"display: none;\"";
            }
            AddControlToPanel(new LiteralControl(String.Format("</legend><table{0}>", rowStyle)), categoryInfo);

            // Control fields
            base.CreateCategory(formFields, args);

            AddControlToPanel(new LiteralControl("</table></fieldset>"), categoryInfo);
        }


        /// <summary>
        /// Creates the field.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected override void CreateField(FieldCreationArgs args)
        {
            CreateFieldInternal(args, true);
        }


        /// <summary>
        /// Creates invisible field with default value if field is not public and it is not allowed to be empty.
        /// </summary>
        /// <param name="args">Form field settings</param>
        protected override void CreateInvisibleField(FieldCreationArgs args)
        {
            CreateFieldInternal(args, false);
        }


        /// <summary>
        /// Creates the field based on given parameters.
        /// </summary>
        /// <param name="args">Field arguments</param>
        /// <param name="visible">Indicates if the field should be visible or invisible</param>
        private void CreateFieldInternal(FieldCreationArgs args, bool visible)
        {
            string rowAttribute = GetRowCssClassAttribute(args);
            AddControlToPanel(new LiteralControl(string.Format("<tr{0}>", rowAttribute)), args.FormFieldInfo);

            if (visible)
            {
                // Create visible field
                base.CreateField(args);
            }
            else
            {
                // Create invisible field
                base.CreateInvisibleField(args);
            }

            AddControlToPanel(new LiteralControl("</tr>"), args.FormFieldInfo);
        }

        #endregion


        #region "Protected virtual methods"

        /// <summary>
        /// Inits basicform's collapse/expand images.
        /// </summary>
        private void InitImages()
        {
            BasicForm.CollapseCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_Form/collapsecategory.png");
            BasicForm.ExpandCategoryImageUrl = UIHelper.GetImageUrl(null, "CMSModules/CMS_Form/expandcategory.png");
        }


        /// <summary>
        /// Registers script for layout.
        /// </summary>
        protected override void RegisterScripts()
        {
            string script = @"
function ToggleFieldSet(label){
    var hiddenField = $cmsj(label).find('.ToggleImage').next();
    var fieldSet = $cmsj(label).parent();
    var image = $cmsj(label).find('.ToggleImage');
    var affectedElements = $cmsj(label).next();
    if(hiddenField.val() == 'True')
    {
        fieldSet.removeClass('Collapsed');
        hiddenField.val('False')
        image.attr('src', '" + BasicForm.CollapseCategoryImageUrl + @"');
        affectedElements.show();
    }
    else
    {
        fieldSet.addClass('Collapsed');
        hiddenField.val('True');
        image.attr('src', '" + BasicForm.ExpandCategoryImageUrl + @"');
        affectedElements.hide();
    }
}";
            ScriptHelper.RegisterJQuery(BasicForm.Page);
            ScriptHelper.RegisterClientScriptBlock(BasicForm, typeof(string), "hidingScriptFieldSets", ScriptHelper.GetScript(script));
        }

        #endregion
    }
}