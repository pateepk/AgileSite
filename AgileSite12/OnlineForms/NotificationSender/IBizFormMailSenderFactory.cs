using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.FormEngine;
using CMS.OnlineForms;

[assembly: RegisterImplementation(typeof(IBizFormMailSenderFactory), typeof(BizFormMailSenderFactory), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineForms
{
    /// <summary>
    /// Represents factory providing on-line form email sender implementations.
    /// </summary>
    public interface IBizFormMailSenderFactory
    {
        /// <summary>
        /// Returns an email sender instance initiated with given data.
        /// </summary>
        /// <param name="formConfiguration">Configuration of the online form</param>
        /// <param name="formData">Data collected from the form</param>
        /// <param name="formDefinition">Form structure definition, may include changes from alternative form if alt.form is used; if empty, default defition from <paramref name="formConfiguration"/> is used</param>
        /// <param name="uploads">Names of file input fields via which files were recently uploaded</param>
        /// <param name="encodeEmails">Indicates if email content should be encoded; false by default</param>
        IBizFormMailSender GetFormMailSender(BizFormInfo formConfiguration, IDataContainer formData, FormInfo formDefinition = null, IEnumerable<string> uploads = null, bool encodeEmails = false);
    }
}
