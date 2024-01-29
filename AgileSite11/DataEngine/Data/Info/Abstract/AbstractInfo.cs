using System;
using System.Data;
using System.Runtime.Serialization;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Abstract object info class.
    /// </summary>
    [Serializable]
    public abstract class AbstractInfo<TInfo> : AbstractInfoBase<TInfo>
        where TInfo : AbstractInfo<TInfo>, new()
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractInfo()
        {
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        protected AbstractInfo(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="createData">If true, data structure of the object is created</param>
        protected AbstractInfo(ObjectTypeInfo typeInfo, bool createData)
            : base(typeInfo, createData)
        {
        }


        /// <summary>
        /// Constructor - Initializes the type dependent values.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="dr">DataRow with the source data</param>
        protected AbstractInfo(ObjectTypeInfo typeInfo, DataRow dr)
            : base(typeInfo, dr)
        {
        }


        /// <summary>
        /// Constructor - Initializes the object with source data.
        /// </summary>
        /// <param name="sourceData">Source data</param>
        protected AbstractInfo(IDataClass sourceData)
            : base(sourceData)
        {
        }


        /// <summary>
        /// Constructor - Initializes the object with source data.
        /// </summary>
        /// <param name="typeInfo">Type information</param>
        /// <param name="sourceData">Source data</param>
        /// <param name="keepSourceData">If true, source data are kept</param>
        protected AbstractInfo(ObjectTypeInfo typeInfo, IDataClass sourceData, bool keepSourceData)
            : base(typeInfo, sourceData, keepSourceData)
        {
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        protected AbstractInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected AbstractInfo(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
            : base(info, context, typeInfos)
        {
        }

        #endregion


        #region "New methods"

        /// <summary>
        /// Creates new object of the given class based on the given settings
        /// </summary>
        /// <param name="settings">Data settings</param>
        protected override BaseInfo NewObject(LoadDataSettings settings)
        {
            return New(settings);
        }


        /// <summary>
        /// Creates new object of the given class based on the given object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static TInfo New(string objectType)
        {
            return NewInternal(objectType);
        }
        

        /// <summary>
        /// Creates new object of the given class
        /// </summary>
        /// <param name="initializer">Optional initializer to set additional properties to the object</param>
        /// <param name="objectType">Object type</param>
        public static TInfo New(Action<TInfo> initializer = null, string objectType = null)
        {
            var obj = New(objectType);

            // Apply initializer
            initializer?.Invoke(obj);

            return obj;
        }


        /// <summary>
        /// Creates new object of the given class with the ability to specify more details about how the object should be initialized.
        /// Use other New methods for default loading of data.
        /// </summary>
        /// <param name="settings">Data settings</param>
        public static TInfo New(LoadDataSettings settings)
        {
            if (settings.Data == null)
            {
                return New(settings.ObjectType);
            }

            // Create using DataRow
            var result = mGenerator.CreateNewObject(settings.Data);
            result?.LoadData(settings);

            return result;
        }


        /// <summary>
        /// Creates new object of the given class
        /// </summary>
        /// <param name="dr">Data row with the data</param>
        /// <param name="objectType">Object type</param>
        public static TInfo New(DataRow dr, string objectType = null)
        {
            return New(new LoadDataSettings(dr, objectType));
        }


        /// <summary>
        /// Creates new object of the given class
        /// </summary>
        /// <param name="dc">Container with the source data</param>
        /// <param name="objectType">Object type</param>
        public static TInfo New(IDataContainer dc, string objectType = null)
        {
            return New(new LoadDataSettings(dc, objectType));
        }

        #endregion
    }
}
