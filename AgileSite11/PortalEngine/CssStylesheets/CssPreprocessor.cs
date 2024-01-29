using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// The object of this class maintains information about CSS preprocessor.
    /// </summary>
    public class CssPreprocessor
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets the current compiled value.
        /// </summary>
        public static string CurrentCompiledValue 
        {
            get
            {
                return PortalContext.CurrentCompiledValue;
            }
            set
            {
                PortalContext.CurrentCompiledValue = value;
            }
        }


        /// <summary>
        /// Machine-friendly name of the CSS preprocessor.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// File extension associated with the preprocessor.
        /// </summary>
        public string Extension
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the preprocessor displayed in the user interface.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Callback function that performs CSS processing. The function consumes CssEventArg object and returns the processed CSS as a string.
        /// </summary>
        public Func<CssEventArgs, string> Callback
        {
            get;
            set;
        }


        /// <summary>
        /// Callback action that renders a script that will be used for client side compilation.
        /// </summary>
        public Action RegisterClientCompilationScripts
        {
            get;
            set;
        }


        /// <summary>
        /// Callback method that returns error description according to preprocessor's nature. String representing error output of JavaScript compiler is passed as the first argument.
        /// </summary>
        public Func<string, string> GetErrorDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether preprocessor uses also client-side compilation.
        /// </summary>
        public bool UsesClientSideCompilation
        {
            get
            {
                return (RegisterClientCompilationScripts != null);
            }
        }

        #endregion
    }
}
