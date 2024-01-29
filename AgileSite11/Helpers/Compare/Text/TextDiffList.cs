using System.Collections.Generic;

namespace CMS.Helpers
{
    /// <summary>
    /// Class for comparison 2 TextData, contains 2 lists of differences.
    /// </summary>
    public class TextDiffList
    {
        #region "Variables"

        // Source and destination TextData
        private string mSrcText = null;
        private string mDestText = null;

        // 2 ArrayLists for differences
        private List<DiffTextPart> mSrcDiffList = null;
        private List<DiffTextPart> mDstDiffList = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Source text data.
        /// </summary>
        private string SrcText
        {
            get
            {
                return mSrcText;
            }
            set
            {
                mSrcText = value;
            }
        }


        /// <summary>
        /// Destination text data.
        /// </summary>
        private string DestText
        {
            get
            {
                return mDestText;
            }
            set
            {
                mDestText = value;
            }
        }


        /// <summary>
        /// List for differences in source text data.
        /// </summary>
        public List<DiffTextPart> SrcDiffList
        {
            get
            {
                if (mSrcDiffList == null)
                {
                    mSrcDiffList = new List<DiffTextPart>();
                }
                return mSrcDiffList;
            }
        }


        /// <summary>
        /// List for differences in destination text data.
        /// </summary>
        public List<DiffTextPart> DstDiffList
        {
            get
            {
                if (mDstDiffList == null)
                {
                    mDstDiffList = new List<DiffTextPart>();
                }
                return mDstDiffList;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="src">Source text</param>
        /// <param name="dst">Destination text</param>
        public TextDiffList(string src, string dst)
        {
            mSrcText = src;
            mDestText = dst;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Analyze 2 TextData and create appropriate difference lists.
        /// </summary>
        public virtual void Analyze()
        {
            // Divide TextData into pieces according to their resemblance
            TextDataComparator comp = new TextDataComparator(SrcText, DestText);
            comp.DivideIntoParts();

            // Sort by lowest index to get correct order of parts
            ComparisonTextPart.SortBy = ComparisonTextPartSortBy.SrcIndex;
            comp.LongestMatches.Sort();

            // Build difference lists
            foreach (ComparisonTextPart part in comp.LongestMatches)
            {
                if (part.SrcIndex != -1)
                {
                    if (part.DestIndex != -1)
                    {
                        AddMatch(part);
                    }
                    else
                    {
                        RemovedFromSource(part);
                    }
                }
                else
                {
                    if (part.DestIndex != -1)
                    {
                        AddedToDestination(part);
                    }
                }
            }

            // Sort destination diff lists to obtain correct order
            ComparisonTextPart.SortBy = ComparisonTextPartSortBy.DestIndex;
            DstDiffList.Sort();
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Method to replace data of destination difference list.
        /// </summary>
        /// <param name="list">New list</param>
        protected void SetNewDstList(List<DiffTextPart> list)
        {
            DstDiffList.Clear();
            mDstDiffList = new List<DiffTextPart>(list);
        }


        /// <summary>
        /// Method to replace data of source difference list.
        /// </summary>
        /// <param name="list">New list</param>
        protected void SetNewSrcList(List<DiffTextPart> list)
        {
            SrcDiffList.Clear();
            mSrcDiffList = new List<DiffTextPart>(list);
        }


        /// <summary>
        /// Add DiffTextPart with match status to both difference lists.
        /// </summary>
        /// <param name="part">Part which should be add as match</param>
        private void AddMatch(ComparisonTextPart part)
        {
            SrcDiffList.Add(new DiffTextPart(part.Text, part.SrcIndex, DiffStatus.Matched));
            DstDiffList.Add(new DiffTextPart(part.Text, part.DestIndex, DiffStatus.Matched));
        }


        /// <summary>
        /// Add TextPart as Removed from source -> means not included in newer version
        /// </summary>
        /// <param name="part">Text part to be added</param>
        private void RemovedFromSource(ComparisonTextPart part)
        {
            SrcDiffList.Add(new DiffTextPart(part.Text, part.SrcIndex, DiffStatus.RemovedFromSource));
        }


        /// <summary>
        /// Add TextPart as Added to destination -> means not included in older version
        /// </summary>
        /// <param name="part">Comparison text part object</param>
        private void AddedToDestination(ComparisonTextPart part)
        {
            DstDiffList.Add(new DiffTextPart(part.Text, part.DestIndex, DiffStatus.AddedToDestination));
        }

        #endregion
    }
}