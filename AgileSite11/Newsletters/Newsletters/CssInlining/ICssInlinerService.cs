using System;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ICssInlinerService), typeof(PreMailerCssInliner), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for CSS inlining service. Used for moving CSS to inline style attributes.
    /// </summary>
    public interface ICssInlinerService
    {
        /// <summary>
        /// Moves CSS to inline style attributes, to gain maximum E-mail client compatibility.
        /// </summary>
        /// <param name="htmlSource">Input HTML string.</param>
        /// <param name="baseUri">Base URL to apply to link elements with href values ending with *.css.</param>
        /// <returns>HTML with inlined styles.</returns>
        string InlineCss(string htmlSource, Uri baseUri);
    }
}
