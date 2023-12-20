using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentMarkupUrlRetriever), typeof(FormComponentMarkupUrlRetriever), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines retriever of a form component markup URL.
    /// </summary>
    internal interface IFormComponentMarkupUrlRetriever
    {
        /// <summary>
        /// Gets URL providing markup of a form component.
        /// </summary>
        /// <param name="formComponentDefinition">Form component to retrieve URL for.</param>
        /// <param name="formId">Identifier of biz form the component belongs to.</param>
        /// <returns>Returns URL providing the markup.</returns>
        string GetUrl(FormComponentDefinition formComponentDefinition, int formId);
    }
}
