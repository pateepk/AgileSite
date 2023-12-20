using System;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class providing DataClassInfo management.
    /// </summary>
    public abstract class DataClassInfoProviderBase<TProvider> : AbstractInfoProvider<DataClassInfo, TProvider>
        where TProvider : DataClassInfoProviderBase<TProvider>, new()
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        protected DataClassInfoProviderBase()
            : base(DataClassInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    Name = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the DataClassInfo objects.
        /// </summary>
        public static ObjectQuery<DataClassInfo> GetClasses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns DataClassInfo with specified ID.
        /// </summary>
        /// <param name="id">DataClassInfo ID</param>
        public static DataClassInfo GetDataClassInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns DataClassInfo with specified name.
        /// </summary>
        /// <param name="name">DataClassInfo name</param>
        public static DataClassInfo GetDataClassInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }
        

        /// <summary>
        /// Returns DataClassInfo with specified GUID.
        /// </summary>
        /// <param name="guid">DataClassInfo GUID</param>                
        public static DataClassInfo GetDataClassInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified DataClassInfo.
        /// </summary>
        /// <param name="infoObj">DataClassInfo to be set</param>
        public static void SetDataClassInfo(DataClassInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified DataClassInfo.
        /// </summary>
        /// <param name="infoObj">DataClassInfo to be deleted</param>
        public static void DeleteDataClassInfo(DataClassInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes DataClassInfo with specified ID.
        /// </summary>
        /// <param name="id">DataClassInfo ID</param>
        public static void DeleteDataClassInfo(int id)
        {
            DataClassInfo infoObj = GetDataClassInfo(id);
            DeleteDataClassInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DataClassInfo info)
        {
            if (info != null)
            {
                ValidateClassName(info.ClassName);
            }

            base.SetInfo(info);
        }


        private static void ValidateClassName(string className)
        {
            if (String.IsNullOrEmpty(className))
            {
                throw new InvalidOperationException("Class name cannot be null or empty.");
            }

            bool separatorFound = false;
            for (int i = 0; i < className.Length; i++)
            {
                // Find namespace separator
                if (className[i] == '.')
                {
                    if ((i == 0) || ((i + 1) >= className.Length) || !ValidationHelper.IsIdentifier(className.Substring(i + 1)) || !ValidationHelper.IsIdentifier(className.Substring(0, i - 1)))
                    {
                        throw new InvalidOperationException(String.Format("Class name '{0}' does not meet restrictions for identifier format.", className));
                    }
                    separatorFound = true;
                    break;
                }
            }

            if (!separatorFound)
            {
                throw new InvalidOperationException(String.Format("Class name '{0}' does not consist of namespace and code name.", className));
            }
        }


        /// <summary>
        /// Updates the object instance in the hashtables. Updates is different than <see cref="AbstractInfoProvider{TInfo,TProvider,TQuery}.RegisterObjectInHashtables(TInfo)"/>, because it logs task about changing object.
        /// </summary>
        /// <param name="info">Object to update</param>
        protected override void UpdateObjectInHashtables(DataClassInfo info)
        {
            base.UpdateObjectInHashtables(info);

            RemoveObjectFromCustomHashtables(info);
        }


        /// <summary>
        /// Deletes the object instance from the hashtables.
        /// </summary>
        /// <param name="info">Object to delete</param>
        /// <exception cref="ArgumentNullException">When info is null</exception>
        protected override void DeleteObjectFromHashtables(DataClassInfo info)
        {
            base.DeleteObjectFromHashtables(info);

            RemoveObjectFromCustomHashtables(info);
        }


        internal override DataClassInfo GetInfoByColumn<T>(string columnName, T value)
        {
            var dataClass = default(DataClassInfo);

            // Base implementation uses the InfoDataSet implementation which causes stack overflow exception for DataClassInfo
            var infoDataRow = GetObjectQuery().WhereEquals(columnName, value).BinaryData(true).TopN(1).Result;
            if (!DataHelper.DataSourceIsEmpty(infoDataRow))
            {
                dataClass = CreateInfo(infoDataRow.Tables[0].Rows[0]);
            }

            return dataClass;
        }


        /// <summary>
        /// Removes the object instance from custom cache storages.
        /// </summary>
        /// <param name="info">Object to remove</param>
        private void RemoveObjectFromCustomHashtables(DataClassInfo info)
        {
            var originalClassName = ValidationHelper.GetString(info.GetOriginalValue("ClassName"), info.ClassName);

            // Remove original ClassInfo object from cache
            ClassStructureInfo.Remove(originalClassName, true);
        }

        #endregion
    }
}