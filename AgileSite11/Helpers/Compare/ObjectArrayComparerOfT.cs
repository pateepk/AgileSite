using System.Collections.Generic;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Object array comparer. Compares 2 lists just by string at index set in ComparableIndex property.
    /// </summary>
    public class ObjectArrayComparer<T> : IComparer<T> where T : class, IList<string>
    {
        #region "Variables"

        private int mComparableIndex = 1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the comparison index in object array. Default is 1.
        /// </summary>
        public int ComparableIndex
        {
            get
            {
                return mComparableIndex;
            }
            set
            {
                mComparableIndex = value;
            }
        }

        #endregion


        #region IComparer<T> Members

        /// <summary>
        /// Compares two specified object arrays.
        /// </summary>
        /// <param name="x">First list to compare</param>
        /// <param name="y">Second list to compare</param>
        public int Compare(T x, T y)
        {
            return CMSString.Compare(x[ComparableIndex], y[ComparableIndex], true);
        }

        #endregion
    }
}