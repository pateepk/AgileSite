using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Helpers;
using CMS.Base;

using Newtonsoft.Json;

namespace CMS.DataEngine
{
    /// <summary>
    /// Extensions for the SettingsProvider classes
    /// </summary>
    public static class DataExtensions
    {
        #region "Collections and objects"

        /// <summary>
        /// Returns true if given list of columns do not have null values in the object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="columns">List of columns to check</param>
        public static bool CheckRequiredColumns(this ISimpleDataContainer obj, params string[] columns)
        {
            // Check all columns
            foreach (string col in columns)
            {
                if (obj.GetValue(col) == null)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Name enumerator over a collection
        /// </summary>
        /// <param name="collection">Collection</param>
        public static IEnumerator<T> GetNamedEnumerator<T>(this INamedEnumerable collection)
        {
            if (!collection.ItemsHaveNames)
            {
                throw new Exception("[INamedEnumerable.GetNamedEnumerator]: This collection cannot be enumerated by names, because its items aren't able to provide the valid names.");
            }

            if (!collection.SortNames)
            {
                // Enumerate without sorting
                foreach (var item in collection)
                {
                    yield return (T)item;
                }
            }
            else
            {
                // Get all items and sort
                var items = new ArrayList();

                // Fill in the list
                foreach (var item in collection)
                {
                    items.Add(item);
                }

                // Sort and output
                items.Sort();

                foreach (var item in items)
                {
                    yield return (T)item;
                }
            }
        }


        /// <summary>
        /// Converts the DataSet to a strongly typed one
        /// </summary>
        /// <param name="ds">Source data set</param>
        public static InfoDataSet<InfoType> As<InfoType>(this DataSet ds)
            where InfoType : BaseInfo
        {
            // Check info dataset directly
            var infoDs = ds as InfoDataSet<InfoType>;
            if (infoDs != null)
            {
                return infoDs;
            }

            // Create new typed DataSet from the given data
            return new InfoDataSet<InfoType>(ds);
        }


        /// <summary>
        /// Ensures that the given DataSet is not cached, copies the data if it is
        /// </summary>
        /// <param name="ds">Source data</param>
        public static DataSet AsModifyable(this DataSet ds)
        {
            // Make sure that cached DataSet is not destroyed
            var objDs = ds as IReadOnlyFlag;
            if ((objDs != null) && objDs.IsReadOnly)
            {
                ds = ds.Copy();
            }

            return ds;
        }


        /// <summary>
        /// Converts the list of objects to a hash set of distinct values
        /// </summary>
        /// <param name="objects">List of objects to convert</param>
        /// <param name="comparer">Comparer</param>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> objects, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(objects, comparer);
        }


        /// <summary>
        /// Adds range of items to hashset.
        /// </summary>
        /// <param name="instance">Hashset to add to.</param>
        /// <param name="range">List of items to add.</param>
        public static void AddRange<T>(this ISet<T> instance, IEnumerable<T> range)
        {
            range.ToList().ForEach(x => instance.Add(x));
        }


        /// <summary>
        /// Converts the list of objects to a dictionary indexed by object ID
        /// </summary>
        /// <param name="objects">List of objects to convert</param>
        public static SafeDictionary<int, BaseInfo> ToDictionaryById(this IEnumerable<BaseInfo> objects)
        {
            var result = new SafeDictionary<int, BaseInfo>();

            // Process all objects
            foreach (var obj in objects)
            {
                result[obj.Generalized.ObjectID] = obj;
            }

            return result;
        }


        /// <summary>
        /// Converts the DataSet to a dictionary indexed by object ID
        /// </summary>
        /// <param name="ds">DataSet with the data</param>
        /// <param name="idColumn">ID column name</param>
        public static SafeDictionary<int, IDataContainer> ToDictionaryById(this DataSet ds, string idColumn)
        {
            var result = new SafeDictionary<int, IDataContainer>();

            // Process all objects
            foreach (DataTable dt in ds.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var dc = new DataRowContainer(dr);
                    var id = ValidationHelper.GetInteger(dc.GetValue(idColumn), 0);

                    result[id] = dc;
                }
            }

            return result;
        }


        /// <summary>
        /// Implicit conversion to indexable type with a specific values
        /// </summary>
        /// <param name="obj">Object to convert</param>
        public static IGeneralIndexable<TKey, TValue> AsIndexable<TKey, TValue>(this IGeneralIndexable obj)
        {
            return new GeneralIndexableWrapper<TKey, TValue>(obj);
        }


        /// <summary>
        /// Returns true if the given table contains some external data (data not from CMS database) 
        /// </summary>
        /// <param name="dt">Data table</param>
        internal static bool ContainsExternalData(this DataTable dt)
        {
            return
                !ValidationHelper.GetBoolean(dt.ExtendedProperties[SqlHelper.TABLE_IS_FROM_CMS_DB], false) ||
                ValidationHelper.GetBoolean(dt.ExtendedProperties[SqlHelper.TABLE_CONTAINS_EXTERNAL_DATA], false);
        }


        /// <summary>
        /// Marks the given table as originated from the CMS database
        /// </summary>
        /// <param name="dt">Data table</param>
        internal static void TrackExternalData(this DataTable dt)
        {
            Action markExternal = () => { dt.ExtendedProperties[SqlHelper.TABLE_CONTAINS_EXTERNAL_DATA] = true; };

            dt.TableNewRow += (sender, args) => markExternal();
            dt.RowChanged += (sender, args) => markExternal();
            dt.ColumnChanged += (sender, args) => markExternal();
        }


        /// <summary>
        /// Marks the given table as originated from the CMS database
        /// </summary>
        /// <param name="dt">Data table</param>
        /// <param name="isFromCmsDatabase">Sets the flag</param>
        internal static void IsFromCMSDatabase(this DataTable dt, bool isFromCmsDatabase)
        {
            dt.ExtendedProperties[SqlHelper.TABLE_IS_FROM_CMS_DB] = isFromCmsDatabase;
        }

        #endregion


        #region "XML/JSON export methods"

        /// <summary>
        /// Returns XML representation of current instance of IEnumerable.
        /// </summary>
        /// <param name="collection">Collection to serialize</param>
        /// <param name="rootName">Name of the root element</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static string ToXML(this IEnumerable collection, string rootName, bool binary)
        {
            var sb = new StringBuilder();

            // Get the list of the objects
            var enumerator = collection.GetEnumerator();

            while (enumerator.MoveNext())
            {
                // Data container
                var dc = enumerator.Current as IDataContainer;
                if (dc != null)
                {
                    sb.Append(dc.ToXML((string)null, binary));
                }
                else
                {
                    // Collection
                    var col = enumerator.Current as IEnumerable;
                    if (col != null)
                    {
                        sb.Append(col.ToXML(rootName, binary));
                    }
                }
            }

            string retval = sb.ToString();

            rootName = !string.IsNullOrEmpty(rootName) ? rootName.Replace(".", "_") : "collection";

            retval = String.Format("<{0}>{1}</{0}>", rootName, retval);

            return retval;
        }


        /// <summary>
        /// Writes XML representation of selected columns of current instance of <see cref="IDataContainer"/> to provided <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="data">IDataContainer to serialize</param>
        /// <param name="xml"><see cref="XmlWriter"/> instance for outputting data</param>
        /// <param name="columns">Columns which should be written</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static void ToXML(this IDataContainer data, XmlWriter xml, List<string> columns, bool binary)
        {
            foreach (string col in columns)
            {
                object val = data[col];
                if (val != null)
                {
                    Type type = val.GetType();

                    if (type == typeof(byte[]))
                    {
                        xml.WriteStartElement(col);
                        if (binary)
                        {
                            byte[] bin = (byte[])val;
                            xml.WriteBase64(bin, 0, bin.Length);
                        }
                        xml.WriteEndElement();
                    }
                    else
                    {
                        xml.WriteElementString(col, XmlHelper.ConvertToString(val, type));
                    }
                }
            }
        }


        /// <summary>
        /// Writes XML representation of current instance of <see cref="IDataContainer"/> to provided <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="data">IDataContainer to serialize</param>
        /// <param name="xml"><see cref="XmlWriter"/> instance for outputting data</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static void ToXML(this IDataContainer data, XmlWriter xml, bool binary)
        {
            data.ToXML(xml, data.ColumnNames, binary);
        }


        /// <summary>
        /// Returns XML representation of current instance of IDataContainer.
        /// </summary>
        /// <param name="data">IDataContainer to serialize</param>
        /// <param name="rootName">Name of the root element</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static string ToXML(this IDataContainer data, string rootName, bool binary)
        {
            if (data != null)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.CheckCharacters = false;

                StringBuilder sb = new StringBuilder();
                using (XmlWriter xml = XmlWriter.Create(sb, settings))
                {
                    string safeObjType = rootName.Replace(".", "_");

                    xml.WriteStartElement(safeObjType);
                    data.ToXML(xml, binary);
                    xml.WriteEndElement();
                }

                return sb.ToString();
            }
            return "";
        }


        /// <summary>
        /// Returns JSON representation of current instance of IEnumerable.
        /// </summary>
        /// <param name="collection">Collection to serialize</param>
        /// <param name="rootName">Name of the root element</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static string ToJSON(this IEnumerable collection, string rootName, bool binary)
        {
            var sb = new StringBuilder();

            // Get the list of the objects
            var enumerator = collection.GetEnumerator();

            bool first = true;
            while (enumerator.MoveNext())
            {
                sb.Append(first ? "[" : ",");

                // Data container
                var dc = enumerator.Current as IDataContainer;
                if (dc != null)
                {
                    sb.Append(dc.ToJSON(null, binary));
                }
                else
                {
                    // General collection
                    var col = enumerator.Current as IEnumerable;
                    if (col != null)
                    {
                        sb.Append(col.ToJSON(rootName, binary));
                    }
                }
                first = false;
            }

            string retval = sb + (sb.Length == 0 ? "" : "]");

            rootName = !string.IsNullOrEmpty(rootName) ? rootName.Replace(".", "_") : "collection";

            retval = String.Format("\"{0}\": {1}", rootName, retval);

            return retval;
        }


        /// <summary>
        /// Returns JSON representation of current instance of IDataContainer.
        /// </summary>
        /// <param name="data">IDataContainer to serialize</param>
        /// <param name="rootName">Name of the root element</param>
        /// <param name="binary">If true, binary data is exported, if false, binary columns remain empty</param>
        public static string ToJSON(this IDataContainer data, string rootName, bool binary)
        {
            if (data != null)
            {
                // Put the columns to a hashtable (Hashtable is supported type for JavaScriptSerializer)
                Hashtable table = new Hashtable();

                foreach (string col in data.ColumnNames)
                {
                    object val = data[col];
                    if (!binary && (val is byte[]))
                    {
                        table[col] = "";
                    }
                    else
                    {
                        table[col] = data[col];
                    }
                }

                string retval = JsonConvert.SerializeObject(table, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Local });

                if (!string.IsNullOrEmpty(rootName))
                {
                    retval = String.Format("{{ \"{0}\": {1} }}", rootName.Replace(".", "_"), retval);
                }

                return retval;
            }
            return "";
        }


        /// <summary>
        /// Exports DataSet to JSON string.
        /// </summary>
        /// <param name="dt">DataTable to export</param>
        /// <param name="includeName">If true, name of the table is included in result</param>
        public static string ToJSON(this DataTable dt, bool includeName)
        {
            StringBuilder sb = new StringBuilder();

            if (dt != null)
            {
                if (includeName)
                {
                    sb.AppendLine("{\"" + dt.TableName + "\": ");
                }
                sb.AppendLine("[");
                bool first = true;
                foreach (DataRow dr in dt.Rows)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }
                    DataRowContainer drContainer = new DataRowContainer(dr);
                    drContainer.AddTableProperty = false;
                    sb.AppendLine(drContainer.ToJSON(null, true));
                    first = false;
                }
                sb.AppendLine("]");
                if (includeName)
                {
                    sb.AppendLine("}");
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Exports DataSet to JSON string.
        /// </summary>
        /// <param name="ds">DataSet to export</param>
        /// <param name="includeName">If true, name of the dataset and names of the tables are included in the result</param>
        public static string ToJSON(this DataSet ds, bool includeName)
        {
            StringBuilder sb = new StringBuilder();

            if (ds != null)
            {
                if (includeName)
                {
                    sb.AppendLine("{\"" + ds.DataSetName + "\": ");
                }
                sb.AppendLine("[");
                bool first = true;
                foreach (DataTable dt in ds.Tables)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }
                    sb.Append(dt.ToJSON(true));
                    first = false;
                }
                sb.AppendLine("]");
                if (includeName)
                {
                    sb.AppendLine("}");
                }
            }

            return sb.ToString();
        }

        #endregion


        #region "Factories"

        /// <summary>
        /// Adds the condition for the column value to the factory
        /// </summary>
        /// <param name="fact">Factory to extend</param>
        /// <param name="className">Object class name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="condition">Condition that must be matched, if null, the value converted to bool must match</param>
        public static IConditionalObjectFactory WhenColumnValue(this IConditionalObjectFactory fact, string className, string columnName, Func<object, bool> condition)
        {
            // Set default condition
            if (condition == null)
            {
                condition = (v => ValidationHelper.GetBoolean(v, false));
            }

            fact.WhenParameter<IDataContainer>(dc => condition(dc.GetValue(columnName)));
            fact.WhenParameter<DataRow>(dr => condition(DataHelper.GetDataRowValue(dr, columnName)));
            fact.WhenParameter<object[]>(data => condition(GetValueFromArray(data, className, columnName)));
            fact.WhenParameter<string>(objectType => condition(GetTypeConditionValue(objectType, columnName)));

            return fact;
        }


        /// <summary>
        /// Gets the column value from the data array
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="className">Class name</param>
        /// <param name="columnName">Column name</param>
        private static object GetValueFromArray(object[] data, string className, string columnName)
        {
            return data[GetColumnIndex(className, columnName)];
        }


        /// <summary>
        /// Gets the column value from the type condition
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="columnName">Column name</param>
        private static object GetTypeConditionValue(string objectType, string columnName)
        {
            // Get the type info
            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if (typeInfo == null)
            {
                return null;
            }

            var tc = typeInfo.TypeCondition;

            // Check if there is a type condition 
            return tc == null ? null : tc.GetFieldValue(columnName);
        }


        /// <summary>
        /// Gets the column index for the given class
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="colName">Column name</param>
        private static int GetColumnIndex(string className, string colName)
        {
            ClassStructureInfo csi = ClassStructureInfo.GetClassInfo(className);

            return csi.GetColumnIndex(colName);
        }


        /// <summary>
        /// Sets up the object using the given lambda expression
        /// </summary>
        /// <param name="obj">Object to set up</param>
        /// <param name="setup">Set up action</param>
        public static T With<T>(this T obj, Action<T> setup)
        {
            setup(obj);

            return obj;
        }

        #endregion


        #region "Connections"

        /// <summary>
        /// Ensures that the event uses transaction
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="tr">Transaction to use</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static void UseTransaction(this CMSEventArgs e, CMSTransactionScope tr = null)
        {
            // Use transaction
            if (tr == null)
            {
                tr = new CMSTransactionScope();
            }

            e.Using(tr);
            e.CallWhenFinished(() => tr.Commit());
        }

        #endregion


        #region "Security"

        /// <summary>
        /// Combines the authorization result enum with a bool result
        /// </summary>
        /// <param name="result">Original result</param>
        /// <param name="combineWith">Result to combine with</param>
        public static AuthorizationResultEnum CombineWith(this AuthorizationResultEnum result, bool combineWith)
        {
            // Deny is always stronger
            if (result == AuthorizationResultEnum.Denied)
            {
                return result;
            }

            result = combineWith ? AuthorizationResultEnum.Allowed : AuthorizationResultEnum.Denied;

            return result;
        }


        /// <summary>
        /// Combines the authorization result enum with a bool result
        /// </summary>
        /// <param name="result">Original result</param>
        /// <param name="combineWith">Result to combine with</param>
        public static AuthorizationResultEnum CombineWith(this AuthorizationResultEnum result, AuthorizationResultEnum combineWith)
        {
            // Deny is always stronger
            if (result == AuthorizationResultEnum.Denied)
            {
                return result;
            }
            else if (result == AuthorizationResultEnum.Insignificant)
            {
                // Use only the other
                return combineWith;
            }

            // Deny by other
            if (combineWith == AuthorizationResultEnum.Denied)
            {
                return AuthorizationResultEnum.Denied;
            }

            return result;
        }


        /// <summary>
        /// Convert the authorization result to a boolean value representing allow / deny. Insignificant means deny.
        /// </summary>
        /// <param name="result">Result</param>
        public static bool ToBoolean(this AuthorizationResultEnum result)
        {
            return (result == AuthorizationResultEnum.Allowed);
        }


        /// <summary>
        /// Convert the boolean value to a AuthorizationResultEnum, outputs Allowed or Denied based on the bool value.
        /// </summary>
        /// <param name="result">Result</param>
        public static AuthorizationResultEnum ToAuthorizationResultEnum(this bool result)
        {
            return (result ? AuthorizationResultEnum.Allowed : AuthorizationResultEnum.Denied);
        }

        #endregion
    }
}