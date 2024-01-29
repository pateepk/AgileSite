
using System;

using CMS;
using CMS.Core;
using CMS.MacroEngine;
using CMS.Newsletters;

[assembly: RegisterExtension(typeof(EmailFeedMethods), typeof(EmailFeed))]
namespace CMS.Newsletters
{
    internal class EmailFeedMethods : MacroMethodContainer
    {
        [MacroMethod(typeof(string), "Returns absolute URL from relative path.", 2)]
        [MacroMethodParam(0, "emailFeed", typeof(EmailFeed), "Email feed.")]
        [MacroMethodParam(1, "relativePath", typeof(object), "Relative path.")]
        public static object GetAbsoluteUrl(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length == 2)
            {
                var emailFeed = parameters[0] as EmailFeed;
                if (emailFeed == null)
                {
                    return null;
                }
    
                var relativePath = GetParamValue(parameters, 1, default(string));
                if (string.IsNullOrEmpty(relativePath))
                {
                    return relativePath;
                }

                string baseUrl = Service.Resolve<IIssueUrlService>().GetBaseUrl(emailFeed.Newsletter);
                return LinkConverter.ConvertUrlToAbsolute(relativePath, baseUrl);
            }

            throw new NotSupportedException();
        }
    }
}
