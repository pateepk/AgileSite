using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// HierarchicalTransformationInfo.
    /// </summary>
    public class HierarchicalTransformationInfo
    {
        #region "Variables"

        private string mTransfromationName = String.Empty;
        private int mItemLevel = -1;
        private UniViewItemType mItemType = UniViewItemType.Item;
        private Guid mItemID = Guid.Empty;
        private string mValue = String.Empty;
        private bool mApplyToSublevels = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the current item identifier(GUID).
        /// </summary>
        public Guid ItemID
        {
            get
            {
                return mItemID;
            }
            set
            {
                mItemID = value;
            }
        }


        /// <summary>
        /// Indicates whether this item is usable only for this level.
        /// </summary>
        public bool ApplyToSublevels
        {
            get
            {
                return mApplyToSublevels;
            }
            set
            {
                mApplyToSublevels = value;
            }
        }


        /// <summary>
        /// Gets or sets the transformation name.
        /// </summary>
        public string TransformationName
        {
            get
            {
                return mTransfromationName;
            }
            set
            {
                mTransfromationName = value;
            }
        }


        /// <summary>
        /// Gets or sets the item level.
        /// </summary>
        public int ItemLevel
        {
            get
            {
                return mItemLevel;
            }
            set
            {
                mItemLevel = value;
            }
        }


        /// <summary>
        /// Gets or sets the Item type.
        /// </summary>
        public UniViewItemType ItemType
        {
            get
            {
                return mItemType;
            }
            set
            {
                mItemType = value;
            }
        }


        /// <summary>
        ///  Gets or sets the condition value.
        /// </summary>
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
            }
        }

        #endregion
    }
}