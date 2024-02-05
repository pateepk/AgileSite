using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Azure.Search.Models;

namespace CMS.Search.Azure
{
    /// <summary>
    /// <para>
    /// Provides mapping of data types and their values from .NET types to Azure Search types.
    /// </para>
    /// <para>
    /// The implementation can map <see cref="int"/>, <see cref="long"/>, <see cref="bool"/>, <see cref="double"/>, <see cref="DateTime"/>, <see cref="string"/> and <see cref="IEnumerable{T}"/> of <see cref="string"/> types directly
    /// to their Azure counterparts. Types <see cref="Guid"/> and <see cref="decimal"/> are mapped as <see cref="string"/> and <see cref="double"/> respectively.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Certain mappings can cause loss of precision. Azure Search does not support sub-millisecond precision of date time.
    /// The <see cref="decimal"/> type is mapped to <see cref="double"/>, which can lead to round-off errors.
    /// </remarks>
    public class DataMapper
    {
        private volatile DataMapperState state;
        private static DataMapper mInstance;


        /// <summary>
        /// Gets the <see cref="DataMapper"/> instance.
        /// </summary>
        public static DataMapper Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new DataMapper(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// Initializes a new data mapper instance with default mappings.
        /// </summary>
        internal DataMapper()
        {
            var initialMappings = new Dictionary<Type, DataMapping>
            {
                { typeof(int), new DataMapping(DataType.Int32, AsIs) },
                { typeof(long), new DataMapping(DataType.Int64, AsIs) },

                { typeof(bool), new DataMapping(DataType.Boolean, AsIs) },

                { typeof(Guid), new DataMapping(DataType.String, GuidToString) },

                { typeof(DateTime), new DataMapping(DataType.DateTimeOffset, AsIs) },

                { typeof(double), new DataMapping(DataType.Double, AsIs) },
                { typeof(decimal), new DataMapping(DataType.Double, DecimalToDouble) },

                { typeof(string), new DataMapping(DataType.String, AsIs) },
                { typeof(IEnumerable<string>), new DataMapping(DataType.Collection(DataType.String), AsIs) }
            };

            state = new DataMapperState(initialMappings);
        }


        /// <summary>
        /// Registers a new mapping of <paramref name="type"/> to Azure Search <see cref="DataType"/> along with a function for conversion
        /// of source values to target values directly assignable to Azure Search API.
        /// </summary>
        /// <param name="type">Type for which to register the mapping.</param>
        /// <param name="mappedDataType">Azure Search data type to be used for storing values of type <paramref name="type"/>.</param>
        /// <param name="conversion">Function converting source value to value of type which is directly assignable to Azure Search API. Leave null to keep the value as is.</param>
        /// <remarks>
        /// <para>
        /// Some types like <see cref="int"/> or <see cref="string"/> are directly assignable to Azure Search.
        /// On the contrary <see cref="Guid"/> is explicitly converted to a string as the default mapping stores <see cref="Guid"/> as a <see cref="DataType.String"/>.
        /// </para>
        /// <para>
        /// In order to avoid unexpected behavior while performing implicit conversions by the Azure Search API, it is recommended to always provide a conversion function.
        /// </para>
        /// </remarks>
        public void RegisterMapping(Type type, DataType mappedDataType, Func<object, object> conversion = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (mappedDataType == null)
            {
                throw new ArgumentNullException(nameof(mappedDataType));
            }

            var mapping = new DataMapping(mappedDataType, conversion ?? AsIs);

            RegisterMappingCore(type, mapping);
        }


        /// <summary>
        /// Registers a new mapping and atomically publishes it.
        /// </summary>
        private void RegisterMappingCore(Type type, DataMapping mapping)
        {
            DataMapperState initialState;
            DataMapperState newState;

            do
            {
                initialState = state;

                Dictionary<Type, DataMapping> mappings = new Dictionary<Type, DataMapping>(initialState.Mappings);
                mappings[type] = mapping;

                newState = new DataMapperState(mappings);
            }
            while (Interlocked.CompareExchange(ref state, newState, state) != initialState);
        }




        /// <summary>
        /// Gets Azure Search <see cref="DataType"/> suitable for given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type for which to return corresponding Azure Search data type.</param>
        /// <returns>Azure Search data type suitable for <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="type"/> has no suitable mapping.</exception>
        public DataType MapType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var currentState = state;

            DataMapping result;
            if (currentState.Mappings.TryGetValue(type, out result))
            {
                return result.DataType;
            }

            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Azure data types are nullable, except for collection
                return MapType(type.GenericTypeArguments[0]);
            }

            foreach (var netType in currentState.KnownTypes)
            {
                if (netType.IsAssignableFrom(type))
                {
                    return currentState.Mappings[netType].DataType;
                }
            }

            throw new InvalidOperationException($"The type '{type.FullName}' has no mapping to Azure Search data type.");
        }


        /// <summary>
        /// Coverts <paramref name="value"/> of type supported by this mapper to result of type suitable for Azure Search.
        /// </summary>
        /// <param name="value">Value of type supported by this mapper.</param>
        /// <returns>Result of type suitable for Azure Search.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="value"/> has no conversion defined (i.e. corresponding mapping in <see cref="MapType"/> does not exist).</exception>
        public object ConvertValue(object value)
        {
            var currentState = state;

            if (value == null)
            {
                return null;
            }

            var valueType = value.GetType();

            DataMapping result;
            if (currentState.Mappings.TryGetValue(valueType, out result))
            {
                return result.Conversion(value);
            }

            foreach (var netType in currentState.KnownTypes)
            {
                if (netType.IsAssignableFrom(valueType))
                {
                    return currentState.Mappings[netType].Conversion(value);
                }
            }

            throw new InvalidOperationException($"Value of type '{valueType.FullName}' has no conversion defined.");
        }


        private static string GuidToString(object guid)
        {
            return ((Guid)guid).ToString();
        }


        private static object DecimalToDouble(object d)
        {
            return (double)(decimal)d;
        }


        private static object AsIs(object o)
        {
            return o;
        }


        /// <summary>
        /// Defines a mapping result containing Azure data type and corresponding conversion function.
        /// </summary>
        private class DataMapping
        {
            /// <summary>
            /// Gets Azure Search <see cref="DataType"/>.
            /// </summary>
            public DataType DataType
            {
                get;
            }


            /// <summary>
            /// Gets function whose result is directly assignable to index field of type <see cref="Microsoft.Azure.Search.Models.DataType"/> in Azure Search.
            /// </summary>
            public Func<object, object> Conversion
            {
                get;
            }


            /// <summary>
            /// Initializes a mapping result with specified data type and conversion function.
            /// </summary>
            /// <param name="dataType"></param>
            /// <param name="conversion"></param>
            public DataMapping(DataType dataType, Func<object, object> conversion)
            {
                DataType = dataType;
                Conversion = conversion;
            }
        }


        /// <summary>
        /// Encapsulates the state of this <see cref="DataMapper"/>. Allows for atomic publishing of newly registered mappings.
        /// </summary>
        private class DataMapperState
        {
            /// <summary>
            /// Maps source .NET types to Azure Search data types and corresponding conversion functions. The conversion function
            /// converts to an object of type which is directly assignable to Azure Search field (i.e. the function is to perform an explicit
            /// conversion to avoid implicit conversions in Azure Search.
            /// </summary>
            public Dictionary<Type, DataMapping> Mappings
            {
                get;
            }


            /// <summary>
            /// List of known types in order of preferred type matching.
            /// </summary>
            public List<Type> KnownTypes
            {
                get;
            }


            /// <summary>
            /// Initializes a new data mapper state from given <paramref name="mappings"/>.
            /// </summary>
            /// <param name="mappings">Mappings to initialize the state with.</param>
            public DataMapperState(Dictionary<Type, DataMapping> mappings)
            {
                Mappings = mappings;
                KnownTypes = BuildTypeInheritanceList(mappings.Keys);
            }


            private List<Type> BuildTypeInheritanceList(IEnumerable<Type> types)
            {
                List<Type> result = new List<Type>();

                foreach (var type in types)
                {
                    TypeInheritanceSortedInsert(type, result);
                }

                return result;
            }


            /// <summary>
            /// Inserts <paramref name="type"/> in a list so that more specific types precede the less specific ones.
            /// Order of types which are not in inheritance hierarchy is undefined.
            /// </summary>
            private void TypeInheritanceSortedInsert(Type type, List<Type> list)
            {
                var insertAt = 0;
                foreach (var item in list)
                {
                    if (item.IsAssignableFrom(type))
                    {
                        break;
                    }
                    ++insertAt;
                }

                list.Insert(insertAt, type);
            }
        }
    }
}
