using System;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Visitor status code.
    /// </summary>
    public static class VisitorStatusCode
    {
        #region "Constants"

        /// <summary>
        /// Unknown status.
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// Unknown status as string.
        /// </summary>
        public const string UnknownString = "0";


        /// <summary>
        /// First user visit.
        /// </summary>
        public const int FirstVisit = 1;

        /// <summary>
        /// First user visit as string.
        /// </summary>
        public const string FirstVisitString = "1";


        /// <summary>
        /// More user visits.
        /// </summary>
        public const int MoreVisits = 2;

        /// <summary>
        /// More user visits as string.
        /// </summary>
        public const string MoreVisitsString = "2";

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the enumeration representation of the Visitor status.
        /// </summary>
        /// <param name="code">Status code</param>
        public static VisitorStatusEnum ToEnum(int code)
        {
            switch (code)
            {
                case Unknown:
                    return VisitorStatusEnum.Unknown;

                case FirstVisit:
                    return VisitorStatusEnum.FirstVisit;

                case MoreVisits:
                    return VisitorStatusEnum.MoreVisits;

                default:
                    return VisitorStatusEnum.Unknown;
            }
        }


        /// <summary>
        /// Returns the visitor status code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static int FromEnum(VisitorStatusEnum value)
        {
            switch (value)
            {
                case VisitorStatusEnum.Unknown:
                    return Unknown;

                case VisitorStatusEnum.FirstVisit:
                    return FirstVisit;

                case VisitorStatusEnum.MoreVisits:
                    return MoreVisits;

                default:
                    return Unknown;
            }
        }


        /// <summary>
        /// Returns the visitor status code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static string FromEnumString(VisitorStatusEnum value)
        {
            switch (value)
            {
                case VisitorStatusEnum.Unknown:
                    return UnknownString;

                case VisitorStatusEnum.FirstVisit:
                    return FirstVisitString;

                case VisitorStatusEnum.MoreVisits:
                    return MoreVisitsString;

                default:
                    return UnknownString;
            }
        }

        #endregion
    }


    #region "Enumeration"

    /// <summary>
    /// Visitor status.
    /// </summary>
    public enum VisitorStatusEnum : int
    {
        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// First visit of the web site.
        /// </summary>
        FirstVisit = 1,

        /// <summary>
        /// More visits.
        /// </summary>
        MoreVisits = 2
    }

    #endregion
}