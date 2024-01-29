using System;
using System.ComponentModel;

namespace CMS.Helpers
{
    /// <summary>
    /// Class suitable for comparing 2 TextData, contains index to both source and destination string.
    /// </summary>
    [ToolboxItem(false)]
    public class ComparisonTextPart : TextPart, IComparable
    {
        #region "Variables"

        // Index to destination string
        private int mDestIndex = -1;

        // Sort by setting
        private static ComparisonTextPartSortBy mSortBy = ComparisonTextPartSortBy.SrcIndex;

        // Diff status
        private ComparisonStatus mStatus = ComparisonStatus.Unknown;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Index to destination string.
        /// </summary>
        public int DestIndex
        {
            get
            {
                return mDestIndex;
            }
        }


        /// <summary>
        /// Determines using which property will be sorting done.
        /// </summary>
        public static ComparisonTextPartSortBy SortBy
        {
            get
            {
                return mSortBy;
            }
            set
            {
                mSortBy = value;
            }
        }


        /// <summary>
        /// Determines if string match was found.
        /// </summary>
        public ComparisonStatus Status
        {
            get
            {
                return mStatus;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// TextPart class constructor.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="srcIndex">Index in older version string</param>
        /// <param name="dstIndex">Index to newer version string</param>
        /// <param name="status">Result of the comparison</param>
        public ComparisonTextPart(string text, int srcIndex, int dstIndex, ComparisonStatus status)
            : base(text, srcIndex)
        {
            mDestIndex = dstIndex;
            mStatus = status;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Creates new ComparisonTextPart from particular index as subpart of existing ComparisonTextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <returns>New ComparisonTextPart create from already existing ComparisonTextPart</returns>
        public new ComparisonTextPart SubPart(int startIndex)
        {
            return new ComparisonTextPart(Text.Substring(startIndex), SrcIndex + startIndex, DestIndex + startIndex, Status);
        }


        /// <summary>
        /// Creates new ComparisonTextPart from particular index as subpart of existing ComparisonTextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <param name="count">Length of new ComparisonTextPart</param>
        /// <returns>New ComparisonTextPart create from already existing ComparisonTextPart</returns>
        public new ComparisonTextPart SubPart(int startIndex, int count)
        {
            return new ComparisonTextPart(Text.Substring(startIndex, count), SrcIndex + startIndex, DestIndex + startIndex, Status);
        }

        #endregion


        #region "IComparable Members"

        /// <summary>
        /// Implementation of CompareTo method for sorting i ArrayList.
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Integer with result of comparison</returns>
        int IComparable.CompareTo(object obj)
        {
            if (!(obj is ComparisonTextPart))
            {
                throw new InvalidCastException("This object is not of type ComparisonTextPart");
            }
            else
            {
                ComparisonTextPart compObj = (ComparisonTextPart)obj;
                switch (SortBy)
                {
                        // Comparing using text length 
                    case ComparisonTextPartSortBy.TextLength:
                        return Length.CompareTo(compObj.Length);

                        // Comparing using index to source string
                    case ComparisonTextPartSortBy.SrcIndex:
                        return SrcIndex.CompareTo(compObj.SrcIndex);

                        // Comparing using index to destination string
                    case ComparisonTextPartSortBy.DestIndex:
                        return DestIndex.CompareTo(compObj.DestIndex);

                        // Comparing using lower value of both index
                    case ComparisonTextPartSortBy.BothIndexes:
                        return Math.Max(DestIndex, SrcIndex).CompareTo(
                            Math.Max(compObj.DestIndex, compObj.SrcIndex));

                        // By default comparing using src index 
                    default:
                        return SrcIndex.CompareTo(compObj.SrcIndex);
                }
            }
        }

        #endregion
    }
}