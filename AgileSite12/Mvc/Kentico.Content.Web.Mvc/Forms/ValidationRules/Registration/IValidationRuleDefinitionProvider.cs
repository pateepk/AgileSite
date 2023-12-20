using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IValidationRuleDefinitionProvider), typeof(ValidationRuleDefinitionProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provider for retrieval of registered form component validation rule definitions for Form builder.
    /// </summary>
    public interface IValidationRuleDefinitionProvider : IFormBuilderDefinitionProvider<ValidationRuleDefinition>
    {
    }
}