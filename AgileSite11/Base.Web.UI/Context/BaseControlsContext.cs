using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extended controls context
    /// </summary>
    public class BaseControlsContext : AbstractContext<BaseControlsContext>, INotCopyThreadItem
    {
        #region "Variables"

        private StringSafeDictionary<Control> mCurrentFilters; 

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current filters collection
        /// </summary>
        public static StringSafeDictionary<Control> CurrentFilters
        {
            get
            {
                var c = Current;

                return c.mCurrentFilters ?? (c.mCurrentFilters = new StringSafeDictionary<Control>());
            }
        }

        #endregion
    }
}
