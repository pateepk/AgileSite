using System;
using System.Net;

using CMS.EventLog;

namespace CMS.Newsletters
{
    /// <summary>
    /// Inlining service.
    /// </summary>
    internal class PreMailerCssInliner : ICssInlinerService
    {
        /// <summary>
        /// Moves CSS to inline style attributes, to gain maximum E-mail client compatibility.
        /// </summary>
        /// <param name="htmlSource">Input HTML string.</param>
        /// <param name="baseUri">Base URL to apply to link elements with href values ending with *.css.</param>
        /// <returns>HTML with inlined styles. Returns original <paramref name="htmlSource" /> when inline process fail.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlSource"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUri"/> is null.</exception>
        public string InlineCss(string htmlSource, Uri baseUri)
        {
            if (String.IsNullOrEmpty(htmlSource))
            {
                throw new ArgumentNullException(nameof(htmlSource));
            }

            if (baseUri == null)
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            var preMailer = new PreMailer.Net.PreMailer(htmlSource, baseUri);

            try
            {
                return preMailer.MoveCssInline(ignoreElements: "#ignore").Html;
            }
            catch (WebException ex)
            {
                EventLogProvider.LogException(nameof(PreMailerCssInliner), "PROCESSEXTERNALCSS", ex);

                return htmlSource;
            }
        }
    }
}
