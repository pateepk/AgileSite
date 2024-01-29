using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro resolving context
    /// </summary>
    public class MacroContext : AbstractContext<MacroContext>, INotCopyThreadItem
    {
        #region "Variables"

        private static MacroResolver mGlobalResolver;

        /// <summary>
        /// Current macro resolver
        /// </summary>
        private MacroResolver mCurrentResolver;
        
        /// <summary>
        /// Locker object
        /// </summary>
        protected static object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Current macro resolver
        /// </summary>
        public static MacroResolver CurrentResolver
        {
            get
            {
                var c = Current;

                return c.mCurrentResolver ?? (c.mCurrentResolver = MacroResolver.GetInstance());
            }
            set
            {
                Current.mCurrentResolver = value;
            }
        }


        /// <summary>
        /// Global static resolver - contains all the global objects. Context resolvers are children of this resolver.
        /// </summary>
        public static MacroResolver GlobalResolver
        {
            get
            {
                return LockHelper.Ensure(ref mGlobalResolver, CreateGlobalResolver, lockObject);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates an instance of the global macro resolver
        /// </summary>
        private static MacroResolver CreateGlobalResolver()
        {
            var r = new MacroResolver(null);

            r.mIsGlobalResolver = true;

            return r;
        }

        #endregion
    }
}
