using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Exception to report not unique code name.
    /// </summary>
    public class CodeNameNotUniqueException : InfoObjectException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        public CodeNameNotUniqueException(GeneralizedInfo obj)
            : this(obj, null)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">Object to which the exception relates</param>
        /// <param name="message">Message</param>
        public CodeNameNotUniqueException(GeneralizedInfo obj, string message)
            : base(obj, FormatMessage(obj, message))
        {
        }


        private static string FormatMessage(GeneralizedInfo obj, string message)
        {
            if (message != null)
            {
                return message;
            }

            string niceObjectType = TypeHelper.GetNiceObjectTypeName(obj.TypeInfo.ObjectType);
            return String.Format(CoreServices.Localization.GetAPIString("general.codenamenotunique", null, "The {0} with code name '{1}' already exists."), niceObjectType.ToLowerCSafe(), HTMLHelper.HTMLEncode(obj.ObjectCodeName));
        }
    }
}