using CMS;
using CMS.Core;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

[assembly: RegisterImplementation(typeof(IFormBuilderConfigurationRetriever), typeof(FormBuilderConfigurationRetriever), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface for retrieving form builder configuration for a form.
    /// </summary>
    /// <remarks>
    /// Provides a cached wrapper around the <see cref="IFormBuilderConfigurationSerializer.Deserialize(string)"/> method.
    /// </remarks>
    internal interface IFormBuilderConfigurationRetriever
    {
        /// <summary>
        /// Retrieves configuration for a given form.
        /// </summary>
        /// <param name="formInfo">Form to retrieve configuration for.</param>
        FormBuilderConfiguration Retrieve(BizFormInfo formInfo);
    }
}
