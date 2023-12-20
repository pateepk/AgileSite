using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormComponent(ConsentSelectorComponent.IDENTIFIER, typeof(ConsentSelectorComponent), "{$kentico.formbuilder.component.consentselector.name$}", Description = "{$kentico.formbuilder.component.consentselector.description$}", IsAvailableInFormBuilderEditor = false, IconClass = "icon-l-text", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides drop-down list for selecting text of <see cref="ConsentInfo"/>.
    /// </summary>
    [RequiresFeatures(FeatureEnum.DataProtection)]
    public class ConsentSelectorComponent : DropDownComponent
    {
        /// <summary>
        /// Represents the <see cref="ConsentSelectorComponent"/> identifier.
        /// </summary>
        public new const string IDENTIFIER = "Kentico.ConsentSelector";


        /// <summary>
        /// Validates whether valid consent is selected.
        /// </summary>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = base.Validate(validationContext).ToList();

            if (ConsentInfoProvider.GetConsentInfo(SelectedValue) == null)
            {
                results.Add(new ValidationResult(ResHelper.GetString("kentico.formbuilder.component.consentselector.invalidconsentselected")));
            }

            return results;
        }


        /// <summary>
        /// Loads option label property.
        /// </summary>
        public override void LoadProperties(FormComponentProperties properties)
        {
            base.LoadProperties(properties);

            Properties.OptionLabel = ResHelper.GetString("kentico.formbuilder.component.consentselector.optionlabel");
        }


        /// <summary>
        /// Returns data source containing <see cref="ConsentInfo.ConsentName"/> and <see cref="ConsentInfo.ConsentDisplayName"/> pairs, for populating drop-down list.
        /// </summary>
        protected override IEnumerable<SelectListItem> GetItems()
        {
            var columns = new List<string>() { ConsentInfo.TYPEINFO.CodeNameColumn, ConsentInfo.TYPEINFO.DisplayNameColumn };

            var consents = ConsentInfoProvider.GetConsents().Columns(columns).TypedResult.Select(c => new SelectListItem() { Value = c.ConsentName, Text = c.ConsentDisplayName });

            return consents;
        }
    }
}
