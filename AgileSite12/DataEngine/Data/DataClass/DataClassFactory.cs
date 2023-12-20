using System.Data;

using CMS.Core;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// DataClass factory.
    /// </summary>
    public static class DataClassFactory
    {
        #region "Methods"

        /// <summary>
        /// Creates new DataClass of the given type
        /// </summary>
        /// <param name="structureInfo">Class structure info</param>
        public static IDataClass NewDataClass(ClassStructureInfo structureInfo)
        {
            var dc = ObjectFactory<IDataClass>.New();
            dc.Init(structureInfo);

            return dc;
        }


        /// <summary>
        /// Creates new DataClass of the given type
        /// </summary>
        /// <param name="className">Class name</param>
        public static IDataClass NewDataClass(string className)
        {
            var classInfo = ClassStructureInfo.GetClassInfo(className);
            return NewDataClass(classInfo);
        }


        /// <summary>
        /// Constructor. Gets a class name and parameters for selecting the item. Use it to load an existing item.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="primaryKeyValue">Primary key value</param>
        public static IDataClass NewDataClass(string className, int primaryKeyValue)
        {
            var dc = NewDataClass(className);
            dc.LoadData(primaryKeyValue);

            return dc;
        }


        /// <summary>
        /// Constructor. Gets a class name and data row. Use it to load an existing item.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="dataRow">Data row representing the current item</param>
        public static IDataClass NewDataClass(string className, DataRow dataRow)
        {
            var dc = NewDataClass(className);
            dc.LoadData(dataRow.AsDataContainer());

            return dc;
        }


        /// <summary>
        /// Constructor. Gets a class name and data row. Use it to load an existing item.
        /// </summary>
        /// <param name="className">Class name in format application.class</param>
        /// <param name="data">Data row representing the current item</param>
        public static IDataClass NewDataClass(string className, IDataContainer data)
        {
            var dc = NewDataClass(className);
            dc.LoadData(data);

            return dc;
        }


        /// <summary>
        /// Changes the default data class type to specific class
        /// </summary>
        public static void ChangeDefaultDataClassTypeTo<ClassType>()
            where ClassType : IDataClass, new()
        {
            ObjectFactory<IDataClass>.SetObjectTypeTo<ClassType>(true);
        }

        #endregion
    }
}