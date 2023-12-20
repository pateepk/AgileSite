using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using CMS.FormEngine;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(NameComponent.IDENTIFIER, typeof(NameComponent), "{$kentico.formbuilder.component.name.name$}", Description = "{$kentico.formbuilder.component.name.description$}", IsAvailableInFormBuilderEditor = false, IconClass = "icon-l-text", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents component for name property.
    /// </summary>
    public class NameComponent : TextInputComponent
    {
        /// <summary>
        /// Represents the <see cref="NameComponent"/> identifier.
        /// </summary>
        public new const string IDENTIFIER = "Kentico.Name";


        private FormInfo mFormInfo;
        private string mOriginalName;


        /// <summary>
        /// Maximal length of name.
        /// </summary>
        public int MaxLength
        {
            get => FormComponentDefinition.IDENTIFIER_MAX_LENGTH;
        }


        /// <summary>
        /// Binds contextual values from a <see cref="PropertiesPanelComponentContext"/> instance. The component cannot work in any other context.
        /// </summary>
        /// <param name="context">Context to bind values from.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="context"/> is of any other type than <see cref="PropertiesPanelComponentContext"/>.</exception>
        public override void BindContext(FormComponentContext context)
        {
            base.BindContext(context);

            if (context is PropertiesPanelComponentContext propertiesPanelContext)
            {
                mFormInfo = propertiesPanelContext.BizFormInfo.Form;
                mOriginalName = propertiesPanelContext.FieldName;
            }
            else
            {
                throw new ArgumentException($"The {nameof(NameComponent)} can be used in context of the properties panel only.", nameof(context));
            }
        }


        /// <summary>
        /// Determines whether Value of this component has correct code name format and whether it is unique in whole form.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = base.Validate(validationContext).ToList();

            if (String.Equals(mOriginalName, Value, StringComparison.OrdinalIgnoreCase))
            {
                return results;
            }

            if (!ValidationHelper.IsIdentifier(Value))
            {
                results.Add(new ValidationResult(ResHelper.GetStringFormat("kentico.formbuilder.component.name.invalidformat", Value), new[] { LabelForPropertyName }));

                return results;
            }

            if (Value.Length > MaxLength)
            {
                results.Add(new ValidationResult(ResHelper.GetStringFormat("kentico.formbuilder.component.name.toolong", MaxLength), new[] { LabelForPropertyName }));

                return results;
            }

            var existingFieldNames = mFormInfo.GetFields<FormFieldInfo>().Select(ffi => ffi.Name);
            if (existingFieldNames.Contains(Value, StringComparer.OrdinalIgnoreCase))
            {
                results.Add(new ValidationResult(ResHelper.GetStringFormat("kentico.formbuilder.component.name.notunique", Value), new[] { LabelForPropertyName }));
            }

            return results;
        }
    }
}
