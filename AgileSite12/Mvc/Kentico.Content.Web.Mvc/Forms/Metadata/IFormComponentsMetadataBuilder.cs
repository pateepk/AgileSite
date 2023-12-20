using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentsMetadataBuilder), typeof(FormComponentsMetadataBuilder), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface to build form components metadata.
    /// </summary>
    internal interface IFormComponentsMetadataBuilder
    {
        /// <summary>
        /// Gets metadata of all registered form components.
        /// </summary>
        /// <param name="formId">Id of a BizFormInfo where form components will be used.</param>
        FormComponentsMetadata GetAll(int formId);
    }
}
