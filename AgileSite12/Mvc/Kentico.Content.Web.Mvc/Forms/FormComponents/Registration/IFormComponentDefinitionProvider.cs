using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormComponentDefinitionProvider), typeof(FormComponentDefinitionProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provider for retrieval of registered component definitions for Form Builder.
    /// </summary>
    public interface IFormComponentDefinitionProvider : IFormBuilderDefinitionProvider<FormComponentDefinition>
    {
    }
}