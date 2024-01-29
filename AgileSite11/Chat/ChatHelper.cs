using System;

using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.DocumentEngine;


namespace CMS.Chat
{
    /// <summary>
    /// Provides helper methods for chat module.
    /// </summary>
    public class ChatHelper : AbstractHelper<ChatHelper>
    {
        /// <summary>
        /// Gets Enum from its underlying type. If the specified <paramref name="value"/> doesn't have its representation in the enum, <paramref name="defaultValue"/> is returned.
        /// 
        /// Exception is thrown if TEnum is not Enum.
        /// </summary>
        /// <example>
        /// enum Days { Saturday = 3, Sunday = 4, Monday = 5, Tuesday = 6, Wednesday = 7, Thursday = 8, Friday = 9};
        /// Days day = GetEnum(5, Days.Saturday); // returns Days.Monday
        /// Days day = GetEnum(1, Days.Saturday); // returns Days.Saturday, (Days)1 would return 1
        /// </example>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <typeparam name="TUnderlayingType">Type of underyling field of an enum (can be any integral type except char)</typeparam>
        /// <param name="value">Value of an enum</param>
        /// <param name="defaultValue">Default value wich will be returned if <paramref name="value"/> is not found within an enum</param>
        /// <returns>Enum element or <paramref name="defaultValue"/></returns>
        public static TEnum GetEnum<TEnum, TUnderlayingType>(TUnderlayingType value, TEnum defaultValue)
            where TEnum : struct, IConvertible
            where TUnderlayingType : struct
        {
            return HelperObject.GetEnumInternal(value, defaultValue);
        }


        /// <summary>
        /// Gets absolute URL from relative document path (if given path is not empty).
        /// </summary>
        /// <param name="documentPath">Relative document path.</param>
        /// <returns>Absolute URL to the given document path.</returns>
        public static string GetDocumentAbsoluteUrl(string documentPath)
        {
            return HelperObject.GetDocumentAbsoluteUrlInternal(documentPath);
        }


        /// <summary>
        /// Gets Enum from its underlying type. If the specified <paramref name="value"/> doesn't have its representation in the enum, <paramref name="defaultValue"/> is returned.
        /// 
        /// Exception is thrown if TEnum is not Enum.
        /// </summary>
        /// <example>
        /// enum Days { Saturday = 3, Sunday = 4, Monday = 5, Tuesday = 6, Wednesday = 7, Thursday = 8, Friday = 9};
        /// Days day = GetEnum(5, Days.Saturday); // returns Days.Monday
        /// Days day = GetEnum(1, Days.Saturday); // returns Days.Saturday, (Days)1 would return 1
        /// </example>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <typeparam name="TUnderlayingType">Type of underyling field of an enum (can be any integral type except char)</typeparam>
        /// <param name="value">Value of an enum</param>
        /// <param name="defaultValue">Default value wich will be returned if <paramref name="value"/> is not found within an enum</param>
        /// <returns>Enum element or <paramref name="defaultValue"/></returns>
        protected virtual TEnum GetEnumInternal<TEnum, TUnderlayingType>(TUnderlayingType value, TEnum defaultValue)
            where TEnum : struct, IConvertible
            where TUnderlayingType : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum");
            }

            if (Enum.IsDefined(typeof(TEnum), value))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }

            return defaultValue;
        }


        /// <summary>
        /// Gets absolute URL from relative document path (if given path is not empty).
        /// </summary>
        /// <param name="documentPath">Relative document path.</param>
        /// <returns>Absolute URL to the given document path.</returns>
        protected virtual string GetDocumentAbsoluteUrlInternal(string documentPath)
        {
            if (!String.IsNullOrWhiteSpace(documentPath))
            {
                return URLHelper.GetAbsoluteUrl(DocumentURLProvider.GetUrl(MacroProcessor.DecodeMacros(documentPath)));
            }

            return String.Empty;
        }
    }
}
