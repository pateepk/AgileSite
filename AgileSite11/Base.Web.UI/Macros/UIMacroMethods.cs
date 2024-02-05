using System.Linq;
using System.Text;
using System.Collections.Generic;

using CMS;
using CMS.MacroEngine;
using CMS.Base.Web.UI.Internal;

[assembly: RegisterExtension(typeof(UIMacroMethods), typeof(SystemUINamespace))]

namespace CMS.Base.Web.UI.Internal
{
    internal class UIMacroMethods : MacroMethodContainer
    {
        /// <summary>
        /// Gets the collection of text encodings available in system.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IEnumerable<string>), "Gets the collection of text encodings available in system.", 1)]
        public static IEnumerable<string> TextEncodings(EvaluationContext context, params object[] parameters)
        {
            return Encoding.GetEncodings().Select(enc => enc.Name).OrderBy(x => x).ToList();
        }
    }
}
