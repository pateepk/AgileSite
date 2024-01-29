using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Context for AB tests.
    /// </summary>
    public sealed class ABTestContext : AbstractContext<ABTestContext>
    {
        #region "Variables"

        private bool mIsFirstABRequest;
        private ABTestInfo mCurrentABTest;
        private ABVariantInfo mCurrentABTestVariant;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current AB test.
        /// </summary>
        public static ABTestInfo CurrentABTest
        {
            get
            {
                return Current.mCurrentABTest;
            }
            set
            {
                Current.mCurrentABTest = value;
            }
        }


        /// <summary>
        /// Current AB test variant.
        /// </summary>
        public static ABVariantInfo CurrentABTestVariant
        {
            get
            {
                return Current.mCurrentABTestVariant;
            }
            set
            {
                Current.mCurrentABTestVariant = value;
            }
        }


        /// <summary>
        /// Indicates whether this request is the first AB request for the page.
        /// </summary>
        /// <remarks>If false, either no AB test is running on the page or the user has already visited it and has valid AB variant saved in the cookie.</remarks>
        public static bool IsFirstABRequest
        {
            get
            {
                return Current.mIsFirstABRequest;
            }
            set
            {
                Current.mIsFirstABRequest = value;
            }
        }

        #endregion
    }
}