using System.Web;
using System.Collections.Generic;
using System.Text;

using CMS.Core;

namespace CMS.Newsletters.Filters
{
    /// <summary>
    /// Filter for replacing macros in text without using the macro resolver, but simple text replace to speed up macro resolving.
    /// </summary>
    internal sealed class MacroReplacerContentFilter : IEmailContentFilter
    {
        private readonly string emailAddress;


        /// <summary>
        /// Creates an instance of <see cref="MacroReplacerContentFilter"/> class.
        /// </summary>
        /// <param name="emailAddress">Email address to be used for the macros.</param>
        public MacroReplacerContentFilter(string emailAddress)
        {
            this.emailAddress = emailAddress;
        }


        /// <summary>
        /// Applies the filter to the given <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to transform.</param>
        /// <returns>Text with applied filter transformation.</returns>
        public string Apply(string text)
        {
            return Replace(text, GetReplacements());
        }


        private Dictionary<string, string> GetReplacements()
        {
            return new Dictionary<string, string>
            {
                { "{%UrlEncode(Email)%}", HttpUtility.UrlEncode(emailAddress) },
                { "{%Hash%}", Service.Resolve<IEmailHashGenerator>().GetEmailHash(emailAddress) }
            };
        }


        private static string Replace(string inputText, Dictionary<string, string> replace)
        {
            var stringBuilder = new StringBuilder(inputText);
            foreach (var key in replace.Keys)
            {
                stringBuilder.Replace(key, replace[key]);
            }

            return stringBuilder.ToString();
        }
    }
}
