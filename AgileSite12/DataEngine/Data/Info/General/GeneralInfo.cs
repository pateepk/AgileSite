using System;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// General info class to work with any object type
    /// </summary>
    public class GeneralInfo : AbstractInfo<GeneralInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.general";

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(GeneralInfoProvider), OBJECT_TYPE, null, null, null, null, null, null, null, null, null, null);

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public GeneralInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Loads the object data from given data container.
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            // Get the correct type
            var type = ValidationHelper.GetString(settings.Data.GetValue(SystemColumns.SOURCE_TYPE), "");
            if (string.IsNullOrEmpty(type))
            {
                throw new Exception("Cannot detect object type from the given DataRow, missing the " + SystemColumns.SOURCE_TYPE + " column value");
            }

            // Get the correct object type
            var obj = ModuleManager.GetReadOnlyObject(type);
            TypeInfo = obj.TypeInfo;

            base.LoadData(settings);
        }

        #endregion
    }
}
