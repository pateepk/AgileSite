using System;
using System.Collections;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide util methods from GlobalHelper in the MacroEngine.
    /// </summary>
    internal class UtilMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns base path for images.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns base image path", 1)]
        public static object BaseImagePath(EvaluationContext context, params object[] parameters)
        {
            return SystemContext.DevelopmentMode ? "~/App_Themes/Default/ZippedImages" : "~/App_Themes/Default/Images/[Images.zip]";
        }


        /// <summary>
        /// Resolves BB code.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Resolves BB code.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be resolved.")]
        public static object ResolveBBCode(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    DiscussionMacroResolver resolver = new DiscussionMacroResolver();
                    resolver.EnableBold = true;
                    resolver.EnableCode = true;
                    resolver.EnableColor = true;
                    resolver.EnableImage = true;
                    resolver.EnableItalics = true;
                    resolver.EnableQuote = true;
                    resolver.EnableSize = true;
                    resolver.EnableStrikeThrough = true;
                    resolver.EnableUnderline = true;
                    resolver.EnableURL = true;

                    return resolver.ResolveMacros(GetStringParam(parameters[0], context.Culture));

                case 2:
                    if (parameters[1] is DiscussionMacroResolver)
                    {
                        DiscussionMacroResolver resolverParam = (DiscussionMacroResolver)parameters[1];
                        return resolverParam.ResolveMacros(GetStringParam(parameters[0], context.Culture));
                    }
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Escapes the string for usage in SQL string to avoid SQL injection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Escapes the string for usage in SQL string to avoid SQL injection.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        public static object SQLEscape(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return SqlHelper.EscapeQuotes(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Strips the HTML tags.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Strips the HTML tags.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        public static object StripTags(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return HTMLHelper.StripTags(GetStringParam(parameters[0], context.Culture), false);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Resolves the URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Resolves the URL.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "URL to be resolved.")]
        public static object ResolveUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return URLHelper.ResolveUrl(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Unresolves the URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Unresolves the URL.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "URL to be unresolved.")]
        public static object UnresolveUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return URLHelper.UnResolveUrl(GetStringParam(parameters[0], context.Culture), SystemContext.ApplicationPath);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Limits the length to specified number of chars.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Limits the length to specified number of chars.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        [MacroMethodParam(1, "length", typeof(int), "Length of the result.")]
        [MacroMethodParam(2, "padString", typeof(string), "Padding string.")]
        [MacroMethodParam(3, "wholeWords", typeof(bool), "Indicates whether the whole words should be preserved.")]
        public static object LimitLength(EvaluationContext context, params object[] parameters)
        {
            string stringResult = "";
            int chars = 100;
            string padString = TextHelper.DEFAULT_ELLIPSIS;
            bool wholeWords = false;

            switch (parameters.Length)
            {
                case 2:
                    stringResult = GetStringParam(parameters[0], context.Culture);
                    chars = GetIntParam(parameters[1]);
                    break;

                case 3:
                    stringResult = GetStringParam(parameters[0], context.Culture);
                    chars = GetIntParam(parameters[1]);
                    padString = GetStringParam(parameters[2], context.Culture);
                    break;

                case 4:
                    stringResult = GetStringParam(parameters[0], context.Culture);
                    chars = GetIntParam(parameters[1]);
                    padString = GetStringParam(parameters[2], context.Culture);
                    wholeWords = GetBoolParam(parameters[3]);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return TextHelper.LimitLength(stringResult, chars, padString, wholeWords);
        }


        /// <summary>
        /// If amount is equal to 1, expression using singular formatting string is returned, otherwise expression using plural formatting string is returned.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// Amount - Amount to be used; 
        /// Singular - Singular formatting string. If formatting item {0} is not included, it is formatted as '[amount] [singular]', e.g.: 1 unit; 
        /// Plural - Plural formatting string. If formatting item {0} is not included, it is formatted as '[amount] [plural]', e.g.: 3 units
        /// </param>
        [MacroMethod(typeof(string), "If amount is equal to 1, expression using singular formatting string is returned, otherwise expression using plural formatting string is returned.", 3)]
        [MacroMethodParam(0, "Amount", typeof(int), "Amount to be used.")]
        [MacroMethodParam(1, "Singular", typeof(string), "Singular formatting string. If formatting item {0} is not included, it is formatted as '[amount] [singular]', e.g.: 1 unit.")]
        [MacroMethodParam(2, "Plural", typeof(string), "Plural formatting string. If formatting item {0} is not included, it is formatted as '[amount] [plural]', e.g.: 3 units.")]
        public static string GetAmountText(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    // Prepare parameters
                    int amount = ValidationHelper.GetInteger(parameters[0], 0);
                    string singular = ValidationHelper.GetString(parameters[1], "");
                    string plural = ValidationHelper.GetString(parameters[2], "");

                    // Get text based on amount
                    return TextHelper.GetAmountText(amount, singular, plural);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Translates given resource string
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Translates given resource string.", 1)]
        [MacroMethodParam(0, "resourceStringKey", typeof(string), "Name of the resource string.")]
        [MacroMethodParam(1, "culture", typeof(string), "Required culture of the translation.")]
        public static object GetResourceString(EvaluationContext context, params object[] parameters)
        {
            // Use LocalizeExpression method to support in-place localization macros for backward compatibility
            switch (parameters.Length)
            {
                case 1:
                    return CoreServices.Localization.LocalizeExpression(GetStringParam(parameters[0], context.Culture), context.Culture);

                case 2:
                    return CoreServices.Localization.LocalizeExpression(GetStringParam(parameters[0], context.Culture), GetStringParam(parameters[1], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the localized object type.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns the nice object type name.", 1)]
        [MacroMethodParam(0, "typeInfo", typeof(ObjectTypeInfo), "Object type constant.")]
        public static object GetNiceObjectTypeName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        var ti = parameters[0] as ObjectTypeInfo;
                        if (ti != null)
                        {
                            return ti.GetNiceObjectTypeName();
                        }
                        else
                        {
                            var obj = parameters[0] as BaseInfo;
                            if (obj != null)
                            {
                                return obj.TypeInfo.GetNiceObjectTypeName();
                            }
                        }
                    }

                    throw new NotSupportedException();

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the full object name
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Gets the full object name.", 1)]
        [MacroMethodParam(0, "obj", typeof(BaseInfo), "Object.")]
        [MacroMethodParam(1, "includeParent", typeof(bool), "If true, the parent object name is included to the object name.")]
        [MacroMethodParam(2, "includeSite", typeof(bool), "If true, the site information is included if available.")]
        [MacroMethodParam(2, "includeGroup", typeof(bool), "If true, the group information is included if available.")]
        public static object GetFullObjectName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                case 2:
                case 3:
                    {
                        var obj = parameters[0] as BaseInfo;
                        if (obj != null)
                        {
                            bool includeParent = GetOptionalParam(parameters, 1, true);
                            bool includeSite = GetOptionalParam(parameters, 2, true);
                            bool includeGroup = GetOptionalParam(parameters, 3, true);

                            return obj.Generalized.GetFullObjectName(includeParent, includeSite, includeGroup);
                        }
                    }

                    throw new NotSupportedException();

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Localizes given text (resolves localization macros).
        /// </summary>
        /// <param name="context">Child resolver object which fired the method evaluation</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Localizes given text (resolves localization macros).", 1)]
        [MacroMethodParam(0, "inputText", typeof(string), "Text to be localized.")]
        [MacroMethodParam(1, "culture", typeof(string), "Required culture of the translation.")]
        public static object Localize(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return ResHelper.LocalizeString(GetStringParam(parameters[0], context.Culture), context.Culture);

                case 2:
                    return ResHelper.LocalizeString(GetStringParam(parameters[0], context.Culture), GetStringParam(parameters[1], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Resolves the data macro expression (expects expression without {% %} brackets).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Resolves macros in given text.", 1)]
        [MacroMethodParam(0, "expression", typeof(string), "Macro expression without {% %} brackets.")]
        public static object ResolveMacroExpression(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        if (!CheckGlobalAdmin(context))
                        {
                            return null;
                        }

                        // Get expression to evaluate
                        var toEval = GetStringParam(parameters[0], context.Culture);

                        // Evaluate the expression
                        var evalResult = context.Resolver.ResolveMacroExpression(toEval, true, true);
                        if (evalResult != null)
                        {
                            return evalResult.Result;
                        }

                        return null;
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Resolves macros in given text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Resolves macros in given text.", 1)]
        [MacroMethodParam(0, "inputText", typeof(string), "Text in which macros should be resolved.")]
        public static object ResolveMacros(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    {
                        var toEval = GetStringParam(parameters[0], context.Culture);

                        // If integrity of the outer macro is fine, it means that the user with which the macro is signed
                        // is correct and it is the user under which the inner macros in the method should be evaluated
                        // So sign the inner macro with same user.
                        if (context.IntegrityPassed)
                        {
                            if (!string.IsNullOrEmpty(context.IdentityName) || !string.IsNullOrEmpty(context.UserName))
                            {
                                toEval = MacroSecurityProcessor.AddSecurityParameters(toEval, new MacroIdentityOption { IdentityName = context.IdentityName, UserName = context.UserName }, null);
                            }
                        }
                        else
                        {
                            // Outer macro is not correctly signed, log the failure to event log
                            MacroDebug.LogSecurityCheckFailure(context.OriginalExpression, context.UserName, context.IdentityName, context.User?.UserName);
                            return null;
                        }

                        return context.Resolver.ResolveMacros(toEval, context);
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Maps the virtual path to the disk.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Maps the virtual path to the disk.", 1)]
        [MacroMethodParam(0, "path", typeof(string), "Virtual path.")]
        public static object MapPath(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    if (HttpContext.Current != null)
                    {
                        return HttpContext.Current.Server.MapPath(GetStringParam(parameters[0], context.Culture));
                    }
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Creates an ArrayList from given list of items.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(ArrayList), "Creates an ArrayList from given list of items.", 1)]
        [MacroMethodParam(0, "items", typeof(object), "List of items to add.")]
        public static object List(EvaluationContext context, params object[] parameters)
        {
            ArrayList list = new ArrayList();
            foreach (object obj in parameters)
            {
                list.Add(obj);
            }
            return list;
        }


        /// <summary>
        /// Encodes the URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Encodes the URL.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "URL to be encoded.")]
        public static object UrlEncode(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return HttpUtility.UrlEncode(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Decodes the URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Decodes the URL.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "URL to be decoded.")]
        public static object UrlDecode(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return HttpUtility.UrlDecode(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Decodes the URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns HTML string representing macro created by encapsulating the expression specified in parameter with specified macro brackets", 1, IsHidden = true)]
        [MacroMethodParam(0, "expression", typeof(string), "Expression to be encapsulated in the macro brackets")]
        [MacroMethodParam(1, "macroType", typeof(string), "Type of the macro (% is default).")]
        public static object CreateHTMLMacroExample(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return "{&#37; " + parameters[0] + " &#37;}";

                case 2:
                    string type = ValidationHelper.GetString(parameters[1], "%");
                    switch (type)
                    {
                        case "$":
                            type = "&#36;";
                            break;
                        case "#":
                            type = "&#35;";
                            break;
                        case "&":
                            type = "&#38;";
                            break;
                        case "@":
                            type = "&#64;";
                            break;
                        default:
                            type = "&#37;";
                            break;
                    }
                    return "{" + type + " " + parameters[0] + " " + type + "}";

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns list of fields of given class.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns list of fields of given class.", 1, IsHidden = true)]
        [MacroMethodParam(0, "className", typeof(string), "Code name of the class")]
        public static object GetClassFields(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    var dc = DataClassInfoProvider.GetDataClassInfo(GetStringParam(parameters[0], ""));
                    if (dc != null)
                    {
                        // Parse the form definition
                        var def = new DataDefinition(dc.ClassFormDefinition);

                        // Return list of names
                        return def.GetFields<FieldInfo>()
                                  .Select(x => x.Name)
                                  .ToList();
                    }
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if given value is valid e-mail.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if given value is valid e-mail.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "Value to check.")]
        public static object IsEmail(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return ValidationHelper.IsEmail(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns if given value is valid U.S. Phone number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns if given value is valid U.S. Phone number.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "Value to check.")]
        public static object IsUsPhoneNumber(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return ValidationHelper.IsUsPhoneNumber(GetStringParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}