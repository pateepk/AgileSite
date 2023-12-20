using System.Collections.Generic;
using System.Linq;

using CMS.FormEngine;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Contains methods for forms and their fields retrieval inside Recorded data tab.
    /// </summary>
    public class RecordedDataFormProvider : FormProvider, IRecordedDataFormProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordedDataFormProvider"/> class.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Provider for registered form components retrieval.</param>
        /// <param name="formComponentActivator">Activator for form components.</param>
        /// <param name="formComponentPropertiesMapper">Mapper for form component properties.</param>
        public RecordedDataFormProvider(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator, IFormComponentPropertiesMapper formComponentPropertiesMapper) 
            : base(formComponentDefinitionProvider, formComponentActivator, formComponentPropertiesMapper)
        {
        }


        /// <summary>
        /// Sets data from <paramref name="visibleFormComponents"/> to given <paramref name="formItem"/> as well as setting all other non-system form fields to NULL.
        /// </summary>
        /// <remarks>
        /// Overriding base implementation to set all other non-system fields in <paramref name="formItem"/> to NULL as editing <paramref name="formItem"/>
        /// in the Recorder data tab might hide a form component previously used to fill a column, therefore, the value of that column is set to NULL.
        /// </remarks>
        /// <param name="formItem">Form item to which data is set.</param>
        /// <param name="visibleFormComponents">Collection of visible form components from which to set data inside <paramref name="formItem"/>.</param>
        protected override void SetBizFormItemData(BizFormItem formItem, List<FormComponent> visibleFormComponents)
        {
            var formInfo = GetFormInfo(formItem.BizFormInfo);
            foreach (var formField in formInfo.GetFields<FormFieldInfo>().Where(f => f.Visible))
            {
                formItem.SetValue(formField.Name, null);
            }

            base.SetBizFormItemData(formItem, visibleFormComponents);
        }
    }
}
