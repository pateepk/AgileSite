using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Modules;

using CMS.ModuleLicenses.Web.UI;

[assembly: RegisterObjectType(typeof(ModuleLicenseKeyListInfo), ModuleLicenseKeyListInfo.OBJECT_TYPE)]

namespace CMS.ModuleLicenses.Web.UI
{
    /// <summary>
    /// Info for loading data to list of module licenses.
    /// </summary>
    internal class ModuleLicenseKeyListInfo : AbstractInfo<ModuleLicenseKeyListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.modulelicensekeylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ModuleLicenseKeyInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ModuleLicenseKeyListInfo object.
        /// </summary>
        public ModuleLicenseKeyListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ModuleLicenseKeyListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ModuleLicenseKeyListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for list of custom module licenses.
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return mCombinedColumnNames ?? (mCombinedColumnNames = new List<string> { "ModuleLicenseKeyID", "ModuleLicenseKeyResourceID", "ResourceDisplayName", "ModuleLicenseKeyLicense" });
        }


        /// <summary>
        /// Gets the data query for list of custom module licenses.
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            var query = ModuleLicenseKeyInfoProvider.GetModuleLicenseKeys()
                .Source(s => s.LeftJoin(new QuerySourceTable(ResourceInfo.TYPEINFO.ClassStructureInfo.TableName, "resource"), "ModuleLicenseKeyResourceID", "ResourceID"))
                .AsNested();

            return query;
        }

        #endregion
    }
}