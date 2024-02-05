using System.Collections.Generic;

using CMS;
using CMS.OnlineForms.Web.UI;

[assembly: RegisterImplementation(typeof(IContactSubmittedFormsControllerService), typeof(ContactSubmittedFormsControllerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Provides service methods regarding contact and its submitted forms.
    /// </summary>
    internal interface IContactSubmittedFormsControllerService
    {
        IEnumerable<ContactSubmittedFormsViewModel> GetSubmittedForms(int contactID);
    }
}