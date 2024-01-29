using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Localization;

[assembly: RegisterObjectType(typeof(ResourceStringListInfo), "cms.resourcestringlist")]

namespace CMS.Localization
{
    internal class ResourceStringListInfo : AbstractInfo<ResourceStringListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.resourcestringlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ResourceStringInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ResourceStringListInfo object.
        /// </summary>
        public ResourceStringListInfo()
            : base(TYPEINFO)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            if (mCombinedColumnNames == null)
            {
                mCombinedColumnNames = new List<string>
                {
                    "StringKey",
                    "StringIsCustom",
                    "DefaultText",
                    "CultureText"
                };
            }

            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("cms.resourcestring", "selectforunigrid");
        }

        #endregion
    }
}