using System;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Data controls interface definition.
    /// </summary>
    public interface ICMSDataProperties : ICMSControlProperties
    {
        /// <summary>
        /// If true, the returned nodes are on the right side of the relationship.
        /// </summary>
        bool RelatedNodeIsOnTheLeftSide
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the relationship.
        /// </summary>
        string RelationshipName
        {
            get;
            set;
        }


        /// <summary>
        /// Select nodes with given relationship with given node.
        /// </summary>
        Guid RelationshipWithNodeGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Transformation name for selected item in format application.class.transformation.
        /// </summary>
        string SelectedItemTransformationName
        {
            get;
            set;
        }


        /// <summary>
        /// Number of items per page.
        /// </summary>
        int PageSize
        {
            get;
            set;
        }


        /// <summary>
        /// Select top N rows.
        /// </summary>
        int SelectTopN
        {
            get;
            set;
        }
        

        /// <summary>
        /// Property to set and get the category name for filtering documents.
        /// </summary>
        string CategoryName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the duplicated (linked) items should be filtered out from the data.
        /// </summary>        
        bool FilterOutDuplicates
        {
            get;
            set;
        }
    }
}