using System;
using System.Collections;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Object array comarer.
    /// </summary>
    public class ObjectArrayComparer : IComparer
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


        #region IComparer Members

        /// <summary>
        /// Compares two specified object arrays.
        /// </summary>
        /// <param name="x">First array to compare (object[])</param>
        /// <param name="y">Second array to compare  (object[])</param>
        public int Compare(object x, object y)
        {
            object[] typeX = x as object[];
            object[] typeY = y as object[];

            return CMSString.Compare(Convert.ToString(typeX[ComparableIndex]), Convert.ToString(typeY[ComparableIndex]), true);
        }

        #endregion
    }
}