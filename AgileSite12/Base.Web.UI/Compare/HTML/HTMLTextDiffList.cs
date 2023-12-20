using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Creates two difference lists for strings containing HTML markup.
    /// </summary>
    public class HTMLTextDiffList : TextDiffList
    {
        #region "Variables"

        private string mRawSrcText = null;
        private string mRawDestText = null;

        private bool mIgnoreHTMLTags = false;
        private bool mConsiderHTMLTagsEqual = false;
        private bool mBalanceContent = true;
        private bool mPlainTextMode = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// If true, the HTML tags are ignored in the comparison.
        /// </summary>
        public bool IgnoreHTMLTags
        {
            get
            {
                return mIgnoreHTMLTags;
            }
            set
            {
                mIgnoreHTMLTags = value;
            }
        }


        /// <summary>
        /// If true, the found HTML tags are considered equal in ignore mode.
        /// </summary>
        public bool ConsiderHTMLTagsEqual
        {
            get
            {
                return mConsiderHTMLTagsEqual;
            }
            set
            {
                mConsiderHTMLTagsEqual = value;
            }
        }


        /// <summary>
        /// If true, the resulting content is balance to include the same content on both sides.
        /// </summary>
        public bool BalanceContent
        {
            get
            {
                return mBalanceContent;
            }
            set
            {
                mBalanceContent = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="src">Source HTML string</param>
        /// <param name="dst">Destination HTML string</param>
        public HTMLTextDiffList(string src, string dst)
            : this(src, dst, false)
        {
        }


        /// <summary>
        /// Constructor allowing to set how to treat HTML tags.
        /// </summary>
        /// <param name="src">Source HTML string</param>
        /// <param name="dst">Destination HTML string</param>
        /// <param name="treatAsPlainText">Indicates if compared text should be treated as plain text even if contains HTML</param>
        public HTMLTextDiffList(string src, string dst, bool treatAsPlainText)
            : base(treatAsPlainText ? src : HTMLHelper.StripTags(src, false), treatAsPlainText ? dst : HTMLHelper.StripTags(dst, false))
        {
            mRawSrcText = HTMLHelper.RegExStripTagsSpaces.Replace(src, "><");
            mRawDestText = HTMLHelper.RegExStripTagsSpaces.Replace(dst, "><");
            mPlainTextMode = treatAsPlainText;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Analyze 2 TextData and create appropriate difference lists.
        /// </summary>
        public override void Analyze()
        {
            base.Analyze();

            // Run the comparison
            SetNewSrcList(MapToSource(SrcDiffList, mRawSrcText));
            SetNewDstList(MapToSource(DstDiffList, mRawDestText));

            // Ensure the same content in the lists
            if (BalanceContent)
            {
                BalanceLists();
            }
        }

        #endregion


        #region "Private helper methods"

        /// <summary>
        /// Remap list of text parts to source with HTML markup.
        /// </summary>
        /// <param name="list">List of parts</param>
        /// <param name="source">Source string</param>
        /// <returns>New Difference list</returns>
        private List<DiffTextPart> MapToSource(List<DiffTextPart> list, string source)
        {
            // New List<DiffTextPart> for DiffParts
            List<DiffTextPart> resultList = new List<DiffTextPart>();

            // Process only when list of parts contains some parts
            if (list.Count > 0)
            {
                ComparisonTextPart part = null;
                DiffTextPart diffPart = null;
                int diffID = 0;

                // Sort the part list by the starting index of parts
                list.Sort();

                // Obtain comparison of text without html tags and with tags
                string strippedSource = (mPlainTextMode ? source : HTMLHelper.StripTags(source, false));
                TextDataComparator comp = new TextDataComparator(source, strippedSource);
                comp.CompareWithHTMLVersion();

                // Go through list of text parts
                while (comp.LongestMatches.Count != 0)
                {
                    part = (ComparisonTextPart)comp.LongestMatches[0];

                    // HTML markup has NoMatch status
                    if (part.Status == ComparisonStatus.NoMatch)
                    {
                        // HTML code, add to the list and continue
                        resultList.Add(new DiffTextPart(part.Text, part.SrcIndex, DiffStatus.HTMLPart));
                        comp.LongestMatches.Remove(part);
                    }
                    else
                    {
                        // If outside of the list, end
                        if (diffID >= list.Count)
                        {
                            break;
                        }

                        diffPart = (DiffTextPart)list[diffID];

                        // Divide into parts if don't match with HTML parts
                        if (part.Length > diffPart.Length)
                        {
                            // Matched as substring of the HTML version, cut this part from the HTML version and continue
                            resultList.Add(new DiffTextPart(diffPart.Text, part.SrcIndex, diffPart.Status));
                            comp.LongestMatches[0] = part.SubPart(diffPart.Length);
                            diffID++;
                        }
                        else
                        {
                            // HTML version matches part of the comparison list item, cut the list item and continue
                            resultList.Add(new DiffTextPart(part.Text, part.SrcIndex, diffPart.Status));
                            list[diffID] = diffPart.SubPart(part.Length);
                            comp.LongestMatches.Remove(part);
                        }
                    }
                }
            }
            else if (!String.IsNullOrEmpty(source))
            {
                // All source is HTML code
                resultList.Add(new DiffTextPart(source, 0, DiffStatus.HTMLPart));
            }

            return resultList;
        }


        /// <summary>
        /// Ensure that in both difference list will be same number of parts.
        /// </summary>
        private void BalanceLists()
        {
            int partID = 0;

            // Ensure same number of matches
            EnsureMatchNumber();

            // Inspect if each part has appropriate opposite in second list
            while (partID < Math.Max(SrcDiffList.Count, DstDiffList.Count))
            {
                partID = InspectDiffPair(SrcDiffList, DstDiffList, partID);
                partID = InspectDiffPair(DstDiffList, SrcDiffList, partID);
            }
        }


        /// <summary>
        /// Inspect if particular match from source list has correct opposite in destination list.
        /// </summary>
        /// <param name="src">Source difference list</param>
        /// <param name="dst">Destination difference list</param>
        /// <param name="index">Actual index to be investigated</param>
        /// <returns>Next index to be inspected</returns>
        private int InspectDiffPair(List<DiffTextPart> src, List<DiffTextPart> dst, int index)
        {
            int newIndex = index;

            // Check if index isn't out of range source List<DiffTextPart>
            if (index < src.Count)
            {
                DiffTextPart srcPart = (DiffTextPart)src[index];

                // Check if index isn't out of range destination List<DiffTextPart>
                if (index < dst.Count)
                {
                    DiffTextPart dstPart = (DiffTextPart)dst[index];

                    // Check source part status
                    switch (srcPart.Status)
                    {
                            // Check pair for matched status
                        case DiffStatus.Matched:
                            switch (dstPart.Status)
                            {
                                    // Only matched pair is accepted
                                case DiffStatus.Matched:
                                    newIndex++;
                                    break;

                                    // Other way skip check and stay on same index
                                default:
                                    break;
                            }
                            break;

                            // HTML
                        case DiffStatus.HTMLPart:
                            switch (dstPart.Status)
                            {
                                    // HTMLNotIncluded status is accepted  
                                case DiffStatus.HTMLNotIncluded:
                                    break;

                                    // HTML parts may match
                                case DiffStatus.HTMLPart:
                                    if (IgnoreHTMLTags && ((srcPart.Text == dstPart.Text) || ConsiderHTMLTagsEqual))
                                    {
                                        // HTML part matches
                                        newIndex++;
                                    }
                                    else
                                    {
                                        // HTML does not match - add HTMLNotIncluded part
                                        dst.Insert(index, new DiffTextPart(srcPart.Text, srcPart.SrcIndex, DiffStatus.HTMLNotIncluded));
                                    }
                                    break;

                                    // If NotIncluded isn't in pair create it
                                default:
                                    dst.Insert(index, new DiffTextPart(srcPart.Text, srcPart.SrcIndex, DiffStatus.HTMLNotIncluded));
                                    break;
                            }
                            newIndex++;
                            break;

                            // Other cases need to have NotIncluded in pair
                        default:
                            switch (dstPart.Status)
                            {
                                    // NotIncluded status is accepted  
                                case DiffStatus.NotIncluded:
                                    break;

                                    // If NotIncluded isn't in pair create it
                                default:
                                    dst.Insert(index, new DiffTextPart(srcPart.Text, srcPart.SrcIndex, DiffStatus.NotIncluded));
                                    break;
                            }
                            newIndex++;
                            break;
                    }
                }
                else
                {
                    // If we are at the end and there isn't any part in second list
                    if (srcPart.Status != DiffStatus.NotIncluded)
                    {
                        if (srcPart.Status == DiffStatus.HTMLPart)
                        {
                            // Not included HTML
                            dst.Insert(index, new DiffTextPart(srcPart.Text, srcPart.SrcIndex, DiffStatus.HTMLNotIncluded));
                        }
                        else
                        {
                            // Not included standard text
                            dst.Insert(index, new DiffTextPart(srcPart.Text, srcPart.SrcIndex, DiffStatus.NotIncluded));
                        }
                        newIndex++;
                    }
                    else
                    {
                        src.RemoveAt(index);
                    }
                }
            }
            return newIndex;
        }


        /// <summary>
        /// Ensure same number of matches and their length in both difference lists.
        /// </summary>
        private void EnsureMatchNumber()
        {
            int srcID = 0, dstID = 0;
            DiffTextPart srcPart = null, dstPart = null;

            // Go through the difference list until we get to the end 
            while ((srcID < SrcDiffList.Count) && (dstID < DstDiffList.Count))
            {
                srcPart = (DiffTextPart)SrcDiffList[srcID];
                dstPart = (DiffTextPart)DstDiffList[dstID];

                // Check for match in source list
                if (srcPart.Status == DiffStatus.Matched)
                {
                    // Check for match in destination list
                    if (dstPart.Status == DiffStatus.Matched)
                    {
                        // If same length, skip this pair
                        if (srcPart.Length != dstPart.Length)
                        {
                            // Not same length, divide longer part into 2 parts
                            if (srcPart.Length > dstPart.Length)
                            {
                                SrcDiffList.Insert(srcID, srcPart.SubPart(0, dstPart.Length));
                                SrcDiffList[srcID + 1] = srcPart.SubPart(dstPart.Length);
                            }
                            else
                            {
                                DstDiffList.Insert(dstID, dstPart.SubPart(0, srcPart.Length));
                                DstDiffList[dstID + 1] = dstPart.SubPart(srcPart.Length);
                            }
                        }

                        srcID++;
                        dstID++;
                    }
                    else
                    {
                        dstID++;
                    }
                }
                else
                {
                    srcID++;
                }
            }
        }

        #endregion
    }
}