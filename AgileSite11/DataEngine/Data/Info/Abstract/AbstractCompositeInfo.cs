using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

using CMS.Base;

namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Base class for info object which represents a composite info object made of multiple internal info components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is intended for internal usage only.
    /// </para>
    /// <para>
    /// <see cref="AbstractCompositeInfo{T}"/> does not have its own internal data class for storing data. Data management is ensured by each internal component.
    /// </para>
    /// </remarks>
    public abstract class AbstractCompositeInfo<TInfo> : AbstractInfoBase<TInfo>
        where TInfo : AbstractCompositeInfo<TInfo>, new()
    {
        #region "Properties"

        /// <summary>
        /// Data class is not used within composite instance. 
        /// Data is managed by the partial classes of each data part.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when property is used. The <see cref="DataClass"/> property is not internally used within composite instance.</exception>
        protected override sealed IDataClass DataClass
        {
            get
            {
                throw new NotSupportedException("[AbstractCompositeInfo.DataClass]: The DataClass property is not internally used within composite instance and is not initialized. Depending code needs to be overridden and partial classes need to be used.");
            }
        }
        

        /// <summary>
        /// Indicates whether the object changed.
        /// </summary>   
        public override bool HasChanged
        {
            get
            {
                return GetComponents().Any(c => c.HasChanged);
            }
        }


        /// <summary>
        /// Indicates whether the object has loaded values in all columns.
        /// </summary>       
        public override bool IsComplete
        {
            get
            {
                return GetComponents().All(c => c.IsComplete);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of <see cref="AbstractCompositeInfo{T}"/>.
        /// </summary>
        protected AbstractCompositeInfo()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="AbstractCompositeInfo{T}"/> with type dependent values initialization.
        /// </summary>
        /// <param name="typeInfo">Type information.</param>
        protected AbstractCompositeInfo(ObjectTypeInfo typeInfo)
            : base(typeInfo)
        {
        }


        /// <summary>
        /// Special constructor is used to deserialize values.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <exception cref = "NotSupportedException" >Thrown when constructor is used. Serialization is not support within the scope of abstract class.</exception>
        protected AbstractCompositeInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SerializationNotSupported();
        }


        /// <summary>
        /// Special constructor is used to deserialize values.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        /// <param name="typeInfos">Type infos that the object may need</param>
        protected AbstractCompositeInfo(SerializationInfo info, StreamingContext context, params ObjectTypeInfo[] typeInfos)
            : base(info, context, typeInfos)
        {
            DeserializeComponents(info);
        }


        /// <summary>
        /// Deserializes the internal components.
        /// </summary>
        /// <param name="info">Serialization info</param>
        private void DeserializeComponents(SerializationInfo info)
        {
            var components = (List<IInfo>)info.GetValue("Components", typeof(List<IInfo>));

            SetComponents(components);
        }


        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">  The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Components", GetComponents().ToList());
        }

        #endregion


        #region "Components methods"

        /// <summary>
        /// Loads internal components data from the given data source.
        /// </summary>
        /// <param name="data">Source data to load.</param>
        /// <remarks>Method is used when <see cref="LoadData(LoadDataSettings)"/> is called.</remarks>
        protected abstract void LoadComponentsData(IDataContainer data);


        /// <summary>
        /// Returns collection of internal components.
        /// </summary>
        /// <remarks>The order of components must be the same as in <see cref="SetComponents"/>.</remarks>
        protected abstract IEnumerable<IInfo> GetComponents();


        /// <summary>
        /// Sets the collection of internal components from external source.
        /// </summary>
        /// <param name="components">Components collection.</param>
        /// <remarks>
        /// <para>
        /// The order of components must be the same as in <see cref="GetComponents"/>.
        /// </para>
        /// <para>
        /// Method is used in cases where already existing internal components should be used for new instance of <see cref="AbstractCompositeInfo{TInfo}"/> (cloning, deserialization etc.)
        /// </para>
        /// </remarks>
        protected abstract void SetComponents(IEnumerable<IInfo> components);


        /// <summary>
        /// Clones the internal components from the given <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Source of data</param>
        /// <param name="clear">If true, the object is cleared to be able to create new object.</param>
        private void CloneComponentsFrom(TInfo source, bool clear = false)
        {
            var newComponents = source.GetComponents().Select(c => c.CloneObject(clear));

            SetComponents(newComponents);
        }


        /// <summary>
        /// Runs the given action for each internal component.
        /// </summary>
        /// <param name="action">Action to make</param>
        private void ForEachComponent(Action<IInfo> action)
        {
            foreach (var component in GetComponents())
            {
                action(component);
            }
        }

        #endregion


        #region "Clone methods"

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object.</param>
        public override TInfo Clone(bool clear)
        {
            TInfo clone = base.Clone(clear);

            clone.CloneComponentsFrom((TInfo)this, clear);

            return clone;
        }


        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        public override TInfo Clone()
        {
            TInfo clone = base.Clone();

            clone.CloneComponentsFrom((TInfo)this);

            return clone;
        }

        #endregion


        #region "CRUD methods"

        /// <summary>
        /// Updates the object data to the database.
        /// </summary>
        protected override void UpdateDataInternal()
        {
            ForEachComponent(c => c.Update());
        }


        /// <summary>
        /// Deletes the object data from the database.
        /// </summary>
        protected override void DeleteDataInternal()
        {
            ForEachComponent(c => c.Delete());
        }


        /// <summary>
        /// Inserts the object data to the database.
        /// </summary>
        protected override void InsertDataInternal()
        {
            using (CMSActionContext ctx = new CMSActionContext())
            {
                // Do not touch parent since it is being updated
                ctx.TouchParent = false;

                ForEachComponent(c => c.Insert());
            }
        }


        /// <summary>
        /// Updates or inserts the object to the database.
        /// </summary>
        /// <exception cref="NotSupportedException">Upsert is not allowed on a composite object.</exception>
        protected override void UpsertData()
        {
            throw new NotSupportedException("[AbstractCompositeInfo.UpsertData]: Upsert is not allowed on a composite object.");
        }


        /// <summary>
        /// Upserts the data to the database.
        /// </summary>
        /// <param name="existingWhere">Existing data where condition</param>
        /// <exception cref="NotSupportedException">Upsert is not allowed on a composite object.</exception>
        protected override void UpsertDataInternal(WhereCondition existingWhere)
        {
            throw new NotSupportedException("[AbstractCompositeInfo.UpsertDataInternal]: Upsert is not allowed on a composite object.");
        }

        #endregion


        #region "IAdvancedDataContainer methods"

        /// <summary>
        /// Obtains value of given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Result value</param>
        /// <returns>Returns <c>true</c> if the operation was successful (the value was present).</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            value = null;
            
            foreach (var component in GetComponents())
            {
                if (component.TryGetValue(columnName, out value))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Sets value of the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <returns>Returns <c>true</c> if the operation was successful</returns>
        public override bool SetValue(string columnName, object value)
        {
            bool result = false;

            foreach (var component in GetComponents())
            {
                if (component.SetValue(columnName, value))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns true if the object contains specific column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            return GetComponents().Any(c => c.ContainsColumn(columnName));
        }


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ItemChanged(string columnName)
        {
            return GetComponents().Any(c => c.ItemChanged(columnName));
        }


        /// <summary>
        /// Returns the original value of given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetOriginalValue(string columnName)
        {
            foreach (var component in GetComponents())
            {
                if (component.ContainsColumn(columnName))
                {
                    return component.GetOriginalValue(columnName);
                }
            }

            return null;
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            ForEachComponent(c => c.ResetChanges());
        }


        /// <summary>
        /// Reverts the object changes and keeps the original values.
        /// </summary>
        public override void RevertChanges()
        {
            ForEachComponent(c => c.RevertChanges());
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database.</param>
        public override void MakeComplete(bool loadFromDb)
        {
            ForEachComponent(c => c.MakeComplete(loadFromDb));
        }


        /// <summary>
        /// Indicates whether object was changed.
        /// </summary>
        /// <param name="excludedColumns">Collection of columns excluded from change (separated by ';').</param>
        public override bool DataChanged(string excludedColumns)
        {
            return GetComponents().Any(c => c.DataChanged(excludedColumns));
        }


        /// <summary>
        /// Executes the given action using original data of the object.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public override void ExecuteWithOriginalData(Action action)
        {
            var enumerator = GetComponents().GetEnumerator();
            ExecuteWithOriginalDataRecursive(enumerator, action);
        }


        /// <summary>
        /// Executes the given action using original data of the object. Recursively wraps all objects from the given enumerator and calls their <see cref="IInfo.ExecuteWithOriginalData"/>.
        /// </summary>
        /// <param name="enumerator">Components enumerator.</param>
        /// <param name="action">Action to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumerator"/> is not set.</exception>
        private void ExecuteWithOriginalDataRecursive(IEnumerator<IInfo> enumerator, Action action)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException("enumerator");
            }

            if (enumerator.MoveNext())
            {
                enumerator.Current.ExecuteWithOriginalData(() =>
                {
                    ExecuteWithOriginalDataRecursive(enumerator, action);
                });
            }
            else
            {
                action();
            }
        }

        #endregion


        #region "IHierarchicalDataContainer methods"

        /// <summary>
        /// Returns the type of given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override Type GetPropertyType(string propertyName)
        {
            foreach (var component in GetComponents())
            {
                var type = component.Generalized.GetPropertyType(propertyName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns collection of column names which values were changed.
        /// </summary>
        public override List<string> ChangedColumns()
        {
            return GetComponents().SelectMany(c => c.ChangedColumns()).ToList();
        }

        #endregion


        #region "Load methods"

        /// <summary>
        /// Loads the object data from given data container defined in <see cref="LoadDataSettings.Data"/>.
        /// </summary>
        /// <param name="settings">Data settings</param>
        /// <remarks><see cref="LoadComponentsData(IDataContainer)"/> is used for data load into the internal components.</remarks>
        protected internal override void LoadData(LoadDataSettings settings)
        {
            if ((settings == null) || (settings.Data == null))
            {
                return;
            }

            LoadComponentsData(settings.Data);
        }


        /// <summary>
        /// Sets the object default values.
        /// </summary>
        /// <remarks>
        /// This method is not called for objects inherited from <see cref="AbstractCompositeInfo{TInfo}"/>.
        /// </remarks>
        protected override sealed void LoadDefaultData()
        {
            base.LoadDefaultData();
        }

        #endregion
    }
}
