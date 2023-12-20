using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentDefaultPropertiesUrlRetriever), typeof(FormComponentDefaultPropertiesUrlRetriever), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines retriever of a form component default properties URL.
    /// </summary>
    internal interface IFormComponentDefaultPropertiesUrlRetriever
    {
        /// <summary>
        /// Gets URL providing default properties of a form component.
        /// </summary>
        /// <param name="formComponentDefinition">Form component to retrieve URL for.</param>
        /// <param name="formId">Id of BizFormInfo where form component will be used.</param>
        /// <returns>Returns URL providing the properties.</returns>
        string GetUrl(FormComponentDefinition formComponentDefinition, int formId);
    }
}
