using CMS;
using CMS.DataEngine;
using CMS.Taxonomy;

[assembly: RegisterObjectType(typeof(CategoryListInfo), "cms.categorylist")]

namespace CMS.Taxonomy
{
    /// <summary>
    /// CategoryListInfo virtual object.
    /// </summary>
    public class CategoryListInfo : AbstractInfo<CategoryListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.categorylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, CategoryInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CategoryListInfo object.
        /// </summary>
        public CategoryListInfo()
            : base(TYPEINFO)
        {
        }

        #endregion
    }
}
