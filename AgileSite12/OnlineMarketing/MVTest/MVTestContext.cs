using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVT Test context
    /// </summary>
    public class MVTContext : AbstractContext<MVTContext>
    {
        #region "Variables"

        private string mCurrentMVTestName;
        private string mCurrentMVTCombinationName;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the current multivariate test name
        /// </summary>
        public static string CurrentMVTestName
        {
            get
            {
                return Current.mCurrentMVTestName;
            }
            set
            {
                Current.mCurrentMVTestName = value;
            }
        }


        /// <summary>
        /// Current MVT combination name
        /// </summary>
        public static string CurrentMVTCombinationName
        {
            get
            {
                return Current.mCurrentMVTCombinationName;
            }
            set
            {
                Current.mCurrentMVTCombinationName = value;
            }
        }

        #endregion
    }
}
