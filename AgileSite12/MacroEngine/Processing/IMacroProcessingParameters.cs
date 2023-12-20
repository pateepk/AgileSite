using System.Collections.Generic;
using System.Text;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents all possible macro resolving parameters aggregated to one object
    /// </summary>
    internal interface IMacroProcessingParameters
    {
        /// <summary>
        /// Gets dictionary containing identity options for macros
        /// </summary>
        IDictionary<string, MacroIdentityOption> Signatures
        {
            get;
        }


        /// <summary>
        /// Gets true when macro expression has to be decoded
        /// </summary>
        bool Decode
        {
            get;
        }


        /// <summary>
        /// Gets old salt value
        /// </summary>
        string OldSalt
        {
            get;
        }


        /// <summary>
        /// Gets new salt value
        /// </summary>
        string NewSalt
        {
            get;
        }


        /// <summary>
        /// Gets builder appending all resolved macros
        /// </summary>
        StringBuilder Builder
        {
            get;
        }


        /// <summary>
        /// Gets identity option to be signed with
        /// </summary>
        MacroIdentityOption IdentityOption
        {
            get;
        }
    }
}
