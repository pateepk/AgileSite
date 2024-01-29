using System;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Exception to report not valid code name.
    /// </summary>
    public class CodeNameNotValidException : InfoObjectException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        public CodeNameNotValidException(GeneralizedInfo obj)
            : this(obj, null)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        /// <param name="message">Message</param>
        public CodeNameNotValidException(GeneralizedInfo obj, string message)
            : base(obj, FormatMessage(obj, message))
        {
        }


        private static string FormatMessage(GeneralizedInfo obj, string message)
        {
            if (message == null)
            {
                return String.Format(CoreServices.Localization.GetAPIString("general.codenamenotvalid", null, "The object code name '{0}' is not valid. The code name can contain only alphanumeric characters, some special characters (_, -, .) and cannot start or end with '.'."), HTMLHelper.HTMLEncode(obj.ObjectCodeName));
            }

            return message;
        }
    }
}