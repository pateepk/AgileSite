using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace WTE.Communication
{
    public delegate object TemplateFieldAccessor(string fieldName);

    /// <summary>
    /// Provides common accessor patterns
    /// </summary>
    public static class TemplateFieldAccessors
    {
        public static TemplateFieldAccessor DictionaryAccessor(IDictionary<string, object> dictionary)
        {
            return DictionaryAccessor(dictionary, false);
        }

        public static TemplateFieldAccessor DictionaryAccessor(IDictionary<string, object> dictionary, bool throwIfNotFound)
        {
            return delegate(string fieldName)
            {
                object value;
                if (!dictionary.TryGetValue(fieldName, out value) && throwIfNotFound)
                    throw new Exception(string.Format("{0} not found in dictionary.", fieldName));
                return value;
            };
        }

        public static TemplateFieldAccessor IDataRecordAccessor<T>(T record) where T : IDataRecord
        {
            return delegate(string fieldName)
            {
                int index = record.GetOrdinal(fieldName);
                if (!record.IsDBNull(index))
                    return record.GetValue(index);
                return null;
            };
        }

        public static TemplateFieldAccessor DataRowAccessor(DataRow row)
        {
            return delegate(string fieldName)
            {
                if (!row.IsNull(fieldName))
                    return row[fieldName];
                return null;
            };
        }

        public static TemplateFieldAccessor PropertyAccessor(object instance)
        {
            Type type = instance.GetType();
            return delegate(string fieldName)
            {
                PropertyInfo info = type.GetProperty(fieldName);
                if (info == null)
                    throw new InvalidOperationException(string.Format("Object {0} does not have property '{1}'", type, fieldName));
                return info.GetValue(instance, null);
            };
        }
    }
}