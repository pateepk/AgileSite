using CMS;
using CMS.DataEngine;
using CMS.Localization;

[assembly: RegisterObjectType(typeof(ResourceTranslatedInfo), "cms.resourcetranslated")]

namespace CMS.Localization
{
    /// <summary>
    /// ResourceTranslated virtual object.
    /// </summary>
    public class ResourceTranslatedInfo : AbstractInfo<ResourceTranslatedInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcetranslated";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ResourceStringInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceTranslated object.
        /// </summary>
        public ResourceTranslatedInfo()
            : base(TYPEINFO)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.resourcestring", "selecttranslated");
        }

        #endregion
    }
}