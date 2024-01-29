using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace CMS.DataEngine
{
    /// <summary>
    /// General info methods and properties
    /// </summary>
    public class InfoHelper
    {
        #region "Variables"

        /// <summary>
        /// Empty info object.
        /// </summary>
        private static readonly Lazy<NotImplementedInfo> mEmptyInfo = new Lazy<NotImplementedInfo>(() => new NotImplementedInfo(), LazyThreadSafetyMode.ExecutionAndPublication);
        
        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo UNKNOWN_TYPEINFO = new ObjectTypeInfo(null, null, null, null, null, null, null, null, null, null, null, null);


        /// <summary>
        /// Constant for automatic code name
        /// </summary>
        public static string CODENAME_AUTOMATIC = "__AUTO__";

        #endregion


        #region "Properties"

        /// <summary>
        /// Empty info object. Can be used instead of null in BaseInfo variables to mark the loaded value as not found to prevent additional calls.
        /// </summary>
        public static BaseInfo EmptyInfo
        {
            get
            {
                return mEmptyInfo.Value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures that the info variable is loaded with the given info. Caches the null value as BaseInfo.Empty info if the value wasn't provided, and returns null in that case.
        /// </summary>
        /// <param name="variable">Variable to use for the value</param>
        /// <param name="getMethod">Method to get the value</param>
        public static BaseInfo EnsureInfo(ref BaseInfo variable, Func<BaseInfo> getMethod)
        {
            var result = variable ?? (variable = getMethod() ?? EmptyInfo);

            // Return null for cached value
            if (result is NotImplementedInfo)
            {
                return null;
            }

            return result;
        }

        #endregion
    }
}
