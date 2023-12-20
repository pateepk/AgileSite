using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(ISectionDefinitionProvider), typeof(SectionDefinitionProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provider for retrieval of registered section definitions for Form Builder.
    /// </summary>
    public interface ISectionDefinitionProvider : IFormBuilderDefinitionProvider<SectionDefinition>
    {
    }
}
