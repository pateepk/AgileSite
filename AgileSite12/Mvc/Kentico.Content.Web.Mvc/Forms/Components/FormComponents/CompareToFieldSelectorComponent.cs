using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(CompareToFieldSelectorComponent.IDENTIFIER, typeof(CompareToFieldSelectorComponent), "Compare to field selector", IsAvailableInFormBuilderEditor = false, ViewName = FormComponentConstants.AutomaticSystemViewName)]


namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Selector component for selecting a field to compare against in validation rules based on comparison to another field.
    /// </summary>
    /// <remarks>
    /// Implementation of the selector is bound to <see cref="PropertiesPanelComponentContext"/> and can be used in the Form builder's properties panel only.
    /// </remarks>
    /// <seealso cref="CompareToFieldValidationRule{TValue}"/>
    public class CompareToFieldSelectorComponent : FormComponent<CompareToFieldSelectorProperties, Guid>
    {
        /// <summary>
        /// Represents the <see cref="CompareToFieldSelectorComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.CompareToFieldSelector";


        /// <summary>
        /// Gets or sets the identifier of a form for which to display available fields.
        /// </summary>
        private BizFormInfo FormInfo { get; set; }


        /// <summary>
        /// Name of field for which the compare rule is being configured.
        /// </summary>
        private string CurrentFieldName { get; set; }


        /// <summary>
        /// Represents the selector value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public Guid FieldGuid { get; set; }


        /// <summary>
        /// Gets enumeration of available fields to populate the selector.
        /// </summary>
        public IEnumerable<SelectListItem> FieldNames
        {
            get
            {
                return GetSelectListItems();
            }
        }


        /// <summary>
        /// Gets name of the <see cref="FieldGuid"/> property.
        /// </summary>
        public override string LabelForPropertyName => nameof(FieldGuid);


        /// <summary>
        /// Gets enumeration of available fields to populate the selector.
        /// </summary>
        protected virtual IEnumerable<SelectListItem> GetSelectListItems()
        {
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(FormInfo.FormClassID);
            var formInfo = FormHelper.GetFormInfo(dataClassInfo.ClassName, false);
            var currentFieldInfo = formInfo.GetFields<FormFieldInfo>().FirstOrDefault(f => f.Name.Equals(CurrentFieldName, StringComparison.InvariantCultureIgnoreCase));

            if (currentFieldInfo == null)
            {
                throw new InvalidOperationException($"Could not load selector for current field named '{CurrentFieldName}'. No such field found in biz form '{FormInfo?.FormName}'.");
            }

            var fields = formInfo.GetFields<FormFieldInfo>().Where(f => f.Visible && !f.Name.Equals(CurrentFieldName, StringComparison.InvariantCultureIgnoreCase) && f.DataType.Equals(currentFieldInfo.DataType, StringComparison.OrdinalIgnoreCase) && f.Precision == currentFieldInfo.Precision);
            bool anyField = false;

            return fields.Select(f =>
            {
                anyField = true;

                var caption = f.GetDisplayName(null);

                return new SelectListItem { Value = f.Guid.ToString(), Text = ResHelper.LocalizeString(caption), Selected = f.Guid.Equals(FieldGuid) };
            }).Concat(Enumerable.Repeat(new SelectListItem { Value = "", Text = ResHelper.GetString("kentico.formbuilder.component.comparetofieldselector.nofieldsavailable") }, 1).Where(i => !anyField));
        }


        /// <summary>
        /// Gets the selected field GUID, if any field is available for selection.
        /// </summary>
        public override Guid GetValue()
        {
            return FieldGuid;
        }


        /// <summary>
        /// Sets the selected field GUID.
        /// </summary>
        public override void SetValue(Guid value)
        {
            FieldGuid = value;
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
                FormInfo = propertiesPanelContext.BizFormInfo;
                CurrentFieldName = propertiesPanelContext.FieldName;
            }
            else
            {
                throw new ArgumentException($"The {nameof(CompareToFieldSelectorComponent)} can be used in context of the properties panel only.", nameof(context));
            }
        }
    }
}