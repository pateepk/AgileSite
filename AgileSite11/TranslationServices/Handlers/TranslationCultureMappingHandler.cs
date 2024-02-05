using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation handler to be used to map culture code from the system to a service in both directions
    /// </summary>
    public class TranslationCultureMappingHandler : SimpleHandler<TranslationCultureMappingHandler, TranslationCultureMappingEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="systemCultureCode">Culture code representation for the system</param>
        /// <param name="serviceCultureCode">Culture code representation for the service</param>
        /// <param name="direction">Direction fo the mapping</param>
        public TranslationCultureMappingEventArgs StartEvent(string systemCultureCode, string serviceCultureCode, TranslationCultureMappingDirectionEnum direction)
        {
            var e = new TranslationCultureMappingEventArgs
            {
                SystemCultureCode = systemCultureCode,
                ServiceCultureCode = serviceCultureCode,
                Direction = direction,
            };

            return StartEvent(e);
        }
    }
}