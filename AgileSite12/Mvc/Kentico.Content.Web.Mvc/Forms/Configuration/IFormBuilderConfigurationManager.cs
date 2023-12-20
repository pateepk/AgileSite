using System;

using CMS;
using CMS.Core;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormBuilderConfigurationManager), typeof(FormBuilderConfigurationManager), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Declares methods for managing the state of a form builder configuration.
    /// </summary>
    internal interface IFormBuilderConfigurationManager
    {
        /// <summary>
        /// Loads Form builder configuration from the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to load.</param>
        /// <returns>Configuration of Form builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> is null.</exception>
        FormBuilderConfiguration Load(BizFormInfo bizFormInfo);


        /// <summary>
        /// Stores Form builder configuration to the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to save.</param>
        /// <param name="configurationJson">JSON configuration of Form builder.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="configurationJson"/> is null.</exception>
        void Store(BizFormInfo bizFormInfo, string configurationJson);
    }
}
