using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Localization
{
    /// <summary>
    /// Localization event argument.
    /// </summary>
    public class LocalizationEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Macro to be resolved.
        /// </summary>
        public string MacroValue
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the macro.
        /// </summary>
        public string MacroResult
        {
            get;
            set;
        }


        /// <summary>
        /// Resource string key.
        /// </summary>
        public string ResourceStringKey
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code.
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether original string localization was modified.
        /// </summary>
        public bool IsMatch
        {
            get;
            set;
        }


        /// <summary>
        /// Macro type that determines the way how the macro is resolved.
        /// </summary>
        public string MacroType
        {
            get;
            set;
        }
    }
}
