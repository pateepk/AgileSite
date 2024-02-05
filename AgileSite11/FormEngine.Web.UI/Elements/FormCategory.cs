using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form field control for the forms.
    /// </summary>
    [ToolboxData("<{0}:FormCategory runat=server></{0}:FormCategory>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormCategory : CMSPlaceHolder
    {
        private FieldLayoutEnum mDefaultFieldLayout = FieldLayoutEnum.Default;
        private FormCategoryHeading mCategoryHeading;


        /// <summary>
        /// Form where the control is used.
        /// </summary>
        public BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Layout of the fields in this category.
        /// </summary>
        public FieldLayoutEnum DefaultFieldLayout
        {
            get
            {
                if (mDefaultFieldLayout == FieldLayoutEnum.Default)
                {
                    mDefaultFieldLayout = Form.DefaultFieldLayout;
                }

                return mDefaultFieldLayout;
            }
            set
            {
                mDefaultFieldLayout = value;
            }
        }


        /// <summary>
        /// Form layout for this category.
        /// </summary>
        private FormLayoutEnum DefaultFormLayout
        {
            get
            {
                return Form.DefaultFormLayout;
            }
        }


        /// <summary>
        /// If true, the field is visible only in development mode
        /// </summary>
        public bool DevelopmentModeOnly
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the category title.
        /// </summary>
        public string CategoryTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets resource string for the category title.
        /// </summary>
        public string CategoryTitleResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// The CSS class rendered by the Web server control on the client. 
        /// </summary>
        public string CssClass
        {
            get;
            set;
        }


        private FormCategoryHeading CategoryHeading
        {
            get
            {
                return mCategoryHeading ?? (mCategoryHeading = new FormCategoryHeading
                {
                    EnableViewState = false,
                });
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Hide control if not in development mode
            if (DevelopmentModeOnly && !SystemContext.DevelopmentMode)
            {
                Visible = false;
            }

            Controls.AddAt(0, CategoryHeading);
        }

        
        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Assign the grouping text to the category
            if (String.IsNullOrEmpty(CategoryTitle) && !String.IsNullOrEmpty(CategoryTitleResourceString))
            {
                CategoryTitle = ResHelper.GetString(CategoryTitleResourceString);
            }

            // FieldSets have legend instead of heading
            if ((DefaultFormLayout == FormLayoutEnum.FieldSets) || String.IsNullOrEmpty(CategoryTitle))
            {
                CategoryHeading.Visible = false;
            }
            else
            {
                CategoryHeading.Text = CategoryTitle;
                CategoryHeading.Level = Form.FieldGroupHeadingLevel;
                CategoryHeading.CssClass = Form.FieldGroupCaptionCssClass;
                CategoryHeading.IsAnchor = Form.FieldGroupHeadingIsAnchor;
            }
        }


        /// <summary>
        /// Render event handler
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Render the table tags or div tags around in case of multiple columns layout
            bool wrapElements = ((DefaultFieldLayout == FieldLayoutEnum.TwoColumns) || (DefaultFieldLayout == FieldLayoutEnum.ThreeColumns));
            if (wrapElements)
            {
                // Use default CSS class from parent if category class is not specified
                if (String.IsNullOrEmpty(CssClass))
                {
                    CssClass = Form.GroupCssClass;
                }

                string categoryOpeningTag;
                string categoryClosingTag;
                string fieldsOpeningTag = null;
                string fieldsClosingTag = null;

                switch (DefaultFormLayout)
                {
                    case FormLayoutEnum.FieldSets:
                        categoryOpeningTag = String.Format("<fieldset{0}>", CssHelper.GetCssClassAttribute(CssClass));
                        if (!String.IsNullOrEmpty(CategoryTitle))
                        {
                            categoryOpeningTag += String.Format("<legend>{0}</legend>", HTMLHelper.HTMLEncode(ResHelper.LocalizeString(CategoryTitle)));
                        }
                        categoryClosingTag = "</fieldset>";
                        break;

                    case FormLayoutEnum.Divs:
                        categoryOpeningTag = String.Format("<div{0}>", CssHelper.GetCssClassAttribute(CssClass));
                        categoryClosingTag = "</div>";
                        fieldsOpeningTag = String.Format("<div{0}>", CssHelper.GetCssClassAttribute(Form.FieldGroupCssClass));
                        fieldsClosingTag = "</div>";
                        break;

                    default:
                        categoryOpeningTag = String.Format("<table{0}>", CssHelper.GetCssClassAttribute(CssClass));
                        categoryClosingTag = "</table>";
                        CategoryHeading.RenderControl(writer);
                        break;
                }
                
                // Render category
                writer.Write(categoryOpeningTag);

                // Render heading
                if (DefaultFormLayout == FormLayoutEnum.Divs)
                {
                    CategoryHeading.RenderControl(writer);
                }

                // Heading already has been rendered manually
                CategoryHeading.Visible = false;

                // Render fields
                writer.Write(fieldsOpeningTag);
                RenderChildren(writer);
                writer.Write(fieldsClosingTag);

                writer.Write(categoryClosingTag);
            }
            else
            {
                RenderChildren(writer);
            }
        }
    }
}