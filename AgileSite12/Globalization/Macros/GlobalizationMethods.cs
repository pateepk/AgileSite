using System;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Globalization
{
    /// <summary>
    /// Globalization macro methods.
    /// </summary>
    public sealed class GlobalizationMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns date time string according to user or current site time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns date time string according to user or current site time zone.", 2)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        [MacroMethodParam(1, "userName", typeof(string), "User name.")]
        public static string GetCurrentDateTimeString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    var userName = ValidationHelper.GetString(parameters[1], "");
                    var user = Service.Resolve<IAuthenticationService>().GetUser(userName);

                    return TimeZoneHelper.ConvertToUserTimeZone(Convert.ToDateTime(parameters[0]), true, user, Service.Resolve<ISiteService>().CurrentSite);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns current user date time DateTime according to user time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns current user date time DateTime according to user time zone.", 1)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        public static object GetUserDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TimeZoneHelper.ConvertToUserDateTime(Convert.ToDateTime(parameters[0]), Service.Resolve<IAuthenticationService>().CurrentUser);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns site date time according to site time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns site date time DateTime according to user time zone.", 1)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        public static object GetSiteDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TimeZoneHelper.ConvertToSiteDateTime(Convert.ToDateTime(parameters[0]), Service.Resolve<ISiteService>().CurrentSite);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns date time with dependence on selected time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns date time with dependence on selected time zone.", 2)]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTim to convert (server time zone).")]
        [MacroMethodParam(1, "timeZoneName", typeof(string), "Time zone code name.")]
        public static object GetCustomDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    var timeZone = TimeZoneInfoProvider.GetTimeZoneInfo(ValidationHelper.GetString(parameters[1], ""));
                    if (timeZone != null)
                    {
                        return TimeZoneHelper.ConvertTimeZoneDateTime(Convert.ToDateTime(parameters[0]), TimeZoneHelper.ServerTimeZone, timeZone);
                    }
                    return ValidationHelper.GetDateTime(parameters[0], DateTimeHelper.ZERO_TIME);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
