using CMS.Base;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Translation culture mapping event arguments
    /// </summary>
    public class TranslationCultureMappingEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Culture code representation for the system
        /// </summary>
        public string SystemCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code representation for the service
        /// </summary>
        public string ServiceCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Direction of the mapping
        /// </summary>
        public TranslationCultureMappingDirectionEnum Direction
        {
            get;
            internal set;
        }
    }
}