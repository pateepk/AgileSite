using System;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IFormProvider), typeof(FormProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains methods for forms and their fields retrieval.
    /// </summary>
    public interface IFormProvider
    {
        /// <summary>
        /// Gets a list of form components as configured in <paramref name="bizFormInfo"/>.
        /// </summary>
        /// <param name="bizFormInfo">Biz form for which to return a list of corresponding form components.</param>
        /// <param name="bizFormComponentContext">
        /// Biz form component context, which is propagated into created components.
        /// If <see cref="BizFormComponentContext.FormInfo"/> is null, it is set to <paramref name="bizFormInfo"/>.
        /// </param>
        /// <returns>Returns a list of form components.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> is null.</exception>
        List<FormComponent> GetFormComponents(BizFormInfo bizFormInfo, BizFormComponentContext bizFormComponentContext = null);


        /// <summary>
        /// Sets data of a form represented by a list of its components.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose data are to be set.</param>
        /// <param name="formComponents">Form components containing values to be set.</param>
        /// <param name="contactGuid">Guid of current contact, can be null.</param>
        /// <returns>Returns the biz form item set.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="formComponents"/> is null.</exception>
        BizFormItem SetFormData(BizFormInfo bizFormInfo, List<FormComponent> formComponents, Guid? contactGuid);


        /// <summary>
        /// Updates form data of already existing <see cref="BizFormItem" />.
        /// </summary>
        /// <param name="bizFormInfo">Form to be updated.</param>
        /// <param name="bizFormItemId">Identifier of form record to be updated.</param>
        /// <param name="formComponents">Form components containing values to be set.</param>
        /// <param name="contactGuid">Guid of current contact, can be null.</param>
        /// <returns>Returns the updated biz form item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="formComponents"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bizFormItemId"/> does not specify an existing <see cref="BizFormItem"/>.</exception>
        BizFormItem UpdateFormData(BizFormInfo bizFormInfo, int bizFormItemId, List<FormComponent> formComponents, Guid? contactGuid);


        /// <summary>
        /// Sends notification and autoresponder emails if the form is configured to do so.
        /// </summary>
        /// <param name="bizFormInfo">Online form for which an emails are to be send</param>
        /// <param name="bizFormItem">Form's submited data which can be included within sent emails</param>
        /// <param name="fileUploadFieldNames">Names of fields containing file uploads to be included as attachments.</param>
        void SendEmails(BizFormInfo bizFormInfo, BizFormItem bizFormItem, IEnumerable<string> fileUploadFieldNames = null);
    }
}
