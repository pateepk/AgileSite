using System.Collections.Generic;
using System.Text;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Default IMacroProcessingParameters implementation
    /// </summary>
    internal sealed class MacroProcessingParameters : IMacroProcessingParameters
    {
        internal MacroProcessingParameters(IDictionary<string, MacroIdentityOption> signatures = null, bool decode = false, string oldSalt = null, 
            string newSalt = null, StringBuilder builder = null, MacroIdentityOption identityOption = null)
        {
            Signatures = signatures;
            Decode = decode;
            OldSalt = oldSalt;
            NewSalt = newSalt;
            Builder = builder;
            IdentityOption = identityOption;
        }


        /// <summary>
        /// Gets hash table containing user signatures
        /// Can be null
        /// </summary>
        public IDictionary<string, MacroIdentityOption> Signatures
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets true when macro expression has to be decoded
        /// False in default
        /// </summary>
        public bool Decode
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets old salt value
        /// Can be null
        /// </summary>
        public string OldSalt
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets new salt value
        /// Can be null
        /// </summary>
        public string NewSalt
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets builder appending all resolved macros
        /// Can be null
        /// </summary>
        public StringBuilder Builder
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets userName to be signed with
        /// Can be null
        /// </summary>
        public MacroIdentityOption IdentityOption
        {
            get;
            private set;
        }
    }
}