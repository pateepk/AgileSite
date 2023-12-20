using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// WIF context.
    /// </summary>
    internal class WIFContext : AbstractContext<WIFContext>, INotCopyThreadItem
    {
        #region "Variables"

        private bool mRequestIsSignOut;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if current request is WIF sign out request.
        /// </summary>
        public static bool RequestIsSignOut
        {
            get
            {
                return Current.mRequestIsSignOut;
            }
            set
            {
                Current.mRequestIsSignOut = value;
            }
        }

        #endregion
    }
}
