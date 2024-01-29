using System;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Enumeration for the direction of culture mapping
    /// </summary>
    public enum TranslationCultureMappingDirectionEnum
    {
        /// <summary>
        /// Mapping from system to translation service
        /// </summary>
        SystemToService = 0,


        /// <summary>
        /// Mapping from translation service to the system
        /// </summary>
        ServiceToSystem = 1
    }
}