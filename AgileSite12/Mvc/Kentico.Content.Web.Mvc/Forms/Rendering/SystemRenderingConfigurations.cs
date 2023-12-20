namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides predefined system rendering configurations.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public static class SystemRenderingConfigurations
    {
        /// <summary>
        /// Gets rendering configuration for properties editor field.
        /// </summary>
        public static FormFieldRenderingConfiguration PropertiesEditorField
        {
            get;
        } = new FormFieldRenderingConfiguration
        {
            LabelWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-label-property" } }
            },
            LabelHtmlAttributes = { { "class", "ktc-control-label" } },

            EditorWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-field-property" } }
            },
            EditorHtmlAttributes = { { "class", "ktc-form-control" } },

            ExplanationTextWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-explanation-text" } }
            }
        };


        /// <summary>
        /// Gets rendering configuration for editor field.
        /// </summary>
        public static FormFieldRenderingConfiguration EditorField
        {
            get;
        } = new FormFieldRenderingConfiguration
        {
            RootConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-form-group" } },
                ChildConfiguration = new ElementRenderingConfiguration
                {
                    ElementName = "div",
                    HtmlAttributes = { { "class", "ktc-field-wrapper" } },
                }
            },

            LabelWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-label-cell" } }
            },
            LabelHtmlAttributes = { { "class", "ktc-control-label ktc-editing-form-label" } },

            ShowSmartFieldIcon = true,

            SmartFieldIconWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-icon-cell" } }
            },
            SmartFieldIconHtmlAttributes = { { "class", "ktc-control-icon icon-scheme-path-circles-flipped ktc-cms-icon-80" } },

            EditorWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-value-cell" } },
            },

            ComponentWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-control-nested-control" } },
            },

            EditorHtmlAttributes = { { "class", "ktc-form-control" } },

            ExplanationTextWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-explanation-text" } }
            }
        };


        /// <summary>
        /// Gets rendering configuration for preview field.
        /// </summary>
        public static FormFieldRenderingConfiguration PreviewField
        {
            get;
        } = new FormFieldRenderingConfiguration
        {
            RootConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-form-group" } },
                ChildConfiguration = new ElementRenderingConfiguration
                {
                    ElementName = "div",
                    HtmlAttributes = { { "class", "ktc-field-wrapper" } },
                }
            },

            LabelWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-label-cell" } }
            },
            LabelHtmlAttributes = { { "class", "ktc-control-label" } },

            EditorWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-value-cell" } },
            },

            ComponentWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-editing-form-control-nested-control" } },
            },

            EditorHtmlAttributes = { { "class", "ktc-form-control" } },

            ExplanationTextWrapperConfiguration = new ElementRenderingConfiguration
            {
                ElementName = "div",
                HtmlAttributes = { { "class", "ktc-explanation-text" } }
            },

            SuppressValidationMessages = true
        };
    }
}
