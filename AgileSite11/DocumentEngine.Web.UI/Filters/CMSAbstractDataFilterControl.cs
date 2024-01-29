using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Abstract class for filter controls.
    /// </summary>
    public abstract class CMSAbstractDataFilterControl : CMSAbstractControlFilterControl, ICMSDataProperties
    {
        #region "Variables"

        private int? mSelectTopN;
        private int? mPageSize;
        private string mSelectedItemTransformationName;
        private bool? mRelatedNodeIsOnTheLeftSide;
        private string mRelationshipName;
        private Guid? mRelationshipWithNodeGuid;
        private bool? mFilterOutDuplicates;

        #endregion


        #region "Properties"

        /// <summary>
        /// Select top N rows.
        /// </summary>
        public virtual int SelectTopN
        {
            get
            {
                return mSelectTopN.HasValue ? mSelectTopN.Value : 0;
            }
            set
            {
                mSelectTopN = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Transformation name for selected item in format application.class.transformation.
        /// </summary>
        public virtual string SelectedItemTransformationName
        {
            get
            {
                return mSelectedItemTransformationName;
            }
            set
            {
                mSelectedItemTransformationName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// If true, the returned nodes are on the right side of the relationship.
        /// </summary>
        public virtual bool RelatedNodeIsOnTheLeftSide
        {
            get
            {
                return mRelatedNodeIsOnTheLeftSide.HasValue && mRelatedNodeIsOnTheLeftSide.Value;
            }
            set
            {
                mRelatedNodeIsOnTheLeftSide = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Name of the relationship.
        /// </summary>
        public virtual string RelationshipName
        {
            get
            {
                return mRelationshipName;
            }
            set
            {
                mRelationshipName = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Select nodes with given relationship with given node.
        /// </summary>
        public virtual Guid RelationshipWithNodeGuid
        {
            get
            {
                return mRelationshipWithNodeGuid.HasValue ? mRelationshipWithNodeGuid.Value : Guid.Empty;
            }
            set
            {
                mRelationshipWithNodeGuid = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        public virtual int PageSize
        {
            get
            {
                return mPageSize.HasValue ? mPageSize.Value : 0;
            }
            set
            {
                mPageSize = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>        
        public bool FilterOutDuplicates
        {
            get
            {
                return mFilterOutDuplicates.HasValue && mFilterOutDuplicates.Value;
            }
            set
            {
                mFilterOutDuplicates = value;
                FilterChanged = true;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Initialize data properties from property object.
        /// </summary>
        /// <param name="properties">Properties object</param>
        public override void InitDataProperties(ICMSBaseProperties properties)
        {
            base.InitDataProperties(properties);

            if (properties is ICMSDataProperties)
            {
                ICMSDataProperties controlProperties = (ICMSDataProperties)properties;

                if (mPageSize != null)
                {
                    controlProperties.PageSize = PageSize;
                }

                if (mRelatedNodeIsOnTheLeftSide != null)
                {
                    controlProperties.RelatedNodeIsOnTheLeftSide = RelatedNodeIsOnTheLeftSide;
                }

                if (mRelationshipName != null)
                {
                    controlProperties.RelationshipName = RelationshipName;
                }

                if (mRelationshipWithNodeGuid != null)
                {
                    controlProperties.RelationshipWithNodeGuid = RelationshipWithNodeGuid;
                }

                if (mSelectedItemTransformationName != null)
                {
                    controlProperties.SelectedItemTransformationName = SelectedItemTransformationName;
                }

                if (mFilterOutDuplicates != null)
                {
                    controlProperties.FilterOutDuplicates = FilterOutDuplicates;
                }
            }
        }

        #endregion
    }
}