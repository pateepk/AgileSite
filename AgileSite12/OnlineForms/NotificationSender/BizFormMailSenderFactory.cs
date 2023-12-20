using System.Collections.Generic;

using CMS.Base;
using CMS.FormEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Default implementation of <see cref="IBizFormMailSenderFactory"/> providing on-line form email sender <see cref="BizFormMailSender"/>.
    /// </summary>
    public class BizFormMailSenderFactory : IBizFormMailSenderFactory
    {
        /// <summary>
        /// Returns an email sender instance initiated with given data.
        /// </summary>
        /// <param name="formConfiguration">Configuration of the online form</param>
        /// <param name="formData">Data collected from the form</param>
        /// <param name="formDefinition">Form structure definition, may include changes from alternative form if alt.form is used; if empty, default defition from <paramref name="formConfiguration"/> is used</param>
        /// <param name="uploads">Names of file input fields via which files were recently uploaded</param>
        /// <param name="encodeEmails">Indicates if email content should be encoded; false by default</param>
        public IBizFormMailSender GetFormMailSender(BizFormInfo formConfiguration, IDataContainer formData, FormInfo formDefinition = null, IEnumerable<string> uploads = null, bool encodeEmails = false)
        {
            return new BizFormMailSender(formConfiguration, formData, formDefinition, uploads, encodeEmails);
        }
    }
}
