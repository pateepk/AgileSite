using System.Collections.Generic;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IValidationRulesMetadataBuilder), typeof(ValidationRulesMetadataBuilder), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface to build validation rules metadata.
    /// </summary>
    internal interface IValidationRulesMetadataBuilder
    {
        /// <summary>
        /// Gets metadata of all registered validation rules.
        /// </summary>
        IList<ValidationRuleMetadata> GetAll();
    }
}
