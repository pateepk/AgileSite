using System;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Analytics context.
    /// </summary>
    [RegisterAllProperties]
    public class AnalyticsContext : AbstractContext<AnalyticsContext>
    {
        #region "Variables"

        private GeoLocation mCurrentGeoLocation;

        private int mRequestContactID;
        private VisitorStatusEnum mCurrentVisitStatus = VisitorStatusEnum.Unknown;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current GEO location
        /// </summary>
        public static GeoLocation CurrentGeoLocation
        {
            get
            {
                var c = Current;

                var activityContext = c.mCurrentGeoLocation;
                if (activityContext == null)
                {
                    c.mCurrentGeoLocation = GeoIPHelper.GetCurrentGeoLocation();
                }

                return c.mCurrentGeoLocation;
            }
        }


        /// <summary>
        /// Returns current contact ID retrieved during request.
        /// </summary>
        [NotRegisterProperty]
        public static int RequestContactID
        {
            get
            {
                return Current.mRequestContactID;
            }
            set
            {
                Current.mRequestContactID = value;
            }
        }


        /// <summary>
        /// Gets or sets the current visit status
        /// </summary>
        public static VisitorStatusEnum CurrentVisitStatus
        {
            get
            {
                return Current.mCurrentVisitStatus;
            }
            set
            {
                Current.mCurrentVisitStatus = value;
            }
        }


        /// <summary>
        /// Returns true if the visitor is returning
        /// </summary>
        public static bool IsReturningVisitor
        {
            get
            {
                return CurrentVisitStatus == VisitorStatusEnum.MoreVisits;
            }
        }


        /// <summary>
        /// Returns true, if the visitor is a new visitor
        /// </summary>
        public static bool IsNewVisitor
        {
            get
            {
                return CurrentVisitStatus == VisitorStatusEnum.FirstVisit;
            }
        }

        #endregion
    }
}