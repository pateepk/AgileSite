using System;

using CMS;
using CMS.Core;
using CMS.FormEngine;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IInvalidComponentConfigurationBuilder), typeof(InvalidComponentConfigurationBuilder), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides interface to build invalid component configuration.
    /// </summary>
    internal interface IInvalidComponentConfigurationBuilder
    {
        /// <summary>
        /// Creates invalid component configuration from <seealso cref="FormFieldInfo"/> with <seealso cref="Exception"/> and localized error message.
        /// </summary>
        /// <param name="formFieldInfo">Source form field info.</param>
        /// <param name="errorMessage">Localized error message.</param>
        /// <param name="exception">Exception related to the invalid component.</param>
        FormComponentConfiguration CreateInvalidFormComponentConfiguration(FormFieldInfo formFieldInfo, string errorMessage, Exception exception);
    }
}
