using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc.Internal;

[assembly: RegisterImplementation(typeof(IRecordedDataFormProvider), typeof(RecordedDataFormProvider), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Contains methods for forms and their fields retrieval inside Recorded data tab.
    /// </summary>
    public interface IRecordedDataFormProvider : IFormProvider
    {
    }
}
