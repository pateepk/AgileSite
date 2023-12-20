using System;
using System.ComponentModel;

namespace CMS.Helpers
{
    /// <summary>
    /// This class conatins reference to appropriate source string including its difference status.
    /// </summary>
    [ToolboxItem(false)]
    public class DiffTextPart : TextPart, IComparable
    {
        #region "Variables"

        // Difference status
        private DiffStatus mStatus = DiffStatus.Unknown;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Difference status.
        /// </summary>
        public DiffStatus Status
        {
            get
            {
                return mStatus;
            }
            set
            {
                mStatus = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default contructor.
        /// </summary>
        /// <param name="text">Text of TextPart</param>
        /// <param name="srcIndex">Index to original source string</param>
        /// <param name="status">Diff status</param>
        public DiffTextPart(string text, int srcIndex, DiffStatus status)
            : base(text, srcIndex)
        {
            mStatus = status;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Creates new DiffTextPart from particular index as subpart of existing DiffTextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <returns>New DiffTextPart created from already existing DiffTextPart</returns>
        public new DiffTextPart SubPart(int startIndex)
        {
            return new DiffTextPart(Text.Substring(startIndex), SrcIndex + startIndex, Status);
        }


        /// <summary>
        /// Creates new DiffTextPart from particular index as subpart of existing DiffTextPart.
        /// </summary>
        /// <param name="startIndex">Index from which should contained text start</param>
        /// <param name="count">Length of new DiffTextPart</param>
        /// <returns>New DiffTextPart created from already existing DiffTextPart</returns>
        public new DiffTextPart SubPart(int startIndex, int count)
        {
            return new DiffTextPart(Text.Substring(startIndex, count), SrcIndex + startIndex, Status);
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
            if (!(obj is DiffTextPart))
            {
                throw new InvalidCastException("This object is not of type DiffTextPart");
            }
            else
            {
                DiffTextPart compObj = (DiffTextPart)obj;

                return SrcIndex.CompareTo(compObj.SrcIndex);
            }
        }

        #endregion
    }
}