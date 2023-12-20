using System;

namespace CMS.Helpers
{
    /// <summary>
    /// General TextPart referencing to index of original Data as a start of TextPart.
    /// </summary>
    public class TextPart : IComparable
    {
        #region "Variables"

        // Index to source string
        private int mSrcIndex = -1;

        // String text part
        private string mText;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Readonly index to source string.
        /// </summary>
        public int SrcIndex
        {
            get
            {
                return mSrcIndex;
            }
        }


        /// <summary>
        /// Relevant string value.
        /// </summary>
        public string Text
        {
            get
            {
                return mText;
            }
        }


        /// <summary>
        /// Length of stored string value.
        /// </summary>
        public int Length
        {
            get
            {
                return Text.Length;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// TextPart class constructor.
        /// </summary>
        /// <param name="text">Particular text part</param>
        /// <param name="srcIndex">Index to previous version string</param>
        public TextPart(string text, int srcIndex)
        {
            mSrcIndex = srcIndex;
            mText = text;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates new TextPart from particular index as subpart of existing TextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <param name="count">Length of new TextPart</param>
        /// <returns>New TextPart create from already existing TextPart</returns>
        public TextPart SubPart(int startIndex, int count)
        {
            return new TextPart(Text.Substring(startIndex, count), SrcIndex + startIndex);
        }


        /// <summary>
        /// Creates new TextPart from particular index as subpart of existing TextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <returns>New TextPart create from already existing TextPart</returns>
        public TextPart SubPart(int startIndex)
        {
            return new TextPart(Text.Substring(startIndex), SrcIndex + startIndex);
        }

        #endregion


        #region "IComparable Members"

        /// <summary>
        /// Implementation of CompareTo method for sorting i ArrayList.
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Integer with result of comparison</returns>
        public int CompareTo(object obj)
        {
            TextPart compObj = (TextPart)obj;
            return SrcIndex.CompareTo(compObj.SrcIndex);
        }

        #endregion
    }
}