using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using CMS.Core;

namespace CMS.Tests
{
    /// <summary>
    /// Provides support for reset of application state
    /// </summary>
    public class AppStateReset
    {
        #region "Field state class"

        /// <summary>
        /// Container for holding the specific field state
        /// </summary>
        public class FieldState
        {
            /// <summary>
            /// Field info
            /// </summary>
            public FieldInfo Field
            {
                get;
            }


            /// <summary>
            /// Field value
            /// </summary>
            public object Value
            {
                get;
            }


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="field"></param>
            /// <param name="value"></param>
            public FieldState(FieldInfo field, object value)
            {
                Field = field;
                Value = value;
            }
        }

        #endregion


        #region "Field excluded class"

        private class ExcludedField
        {
            public string FieldName { get; }

            public string TypeName { get; }


            /// <summary>
            /// This constructor should not be used outside the class, so it more remains error-prone.
            /// </summary>
            private ExcludedField(string fieldName, string typeName)
            {
                if (String.IsNullOrEmpty(fieldName))
                {
                    throw new ArgumentException("Field's name cannot be null nor empty.", nameof(fieldName));
                }
                if (String.IsNullOrEmpty(typeName))
                {
                    throw new ArgumentException("Type's name cannot be null nor empty.", nameof(typeName));
                }

                FieldName = fieldName;
                TypeName = typeName;
            }


            /// <summary>
            /// Creates new instance of <see cref="ExcludedField"/> that refers to a static member
            /// (should be property in most cases) of a type.
            /// </summary>
            /// <example>
            /// This example shows typical usage with a static class
            /// <code>
            /// ExcludedField excludedField = ExcludedField.Create(() => AssemblyDiscoveryHelper.AssemblyDiscovery);
            /// </code>
            /// </example>
            internal static ExcludedField Create<TResult>(Expression<Func<TResult>> memberSelector)
            {
                return Create(memberSelector.Body as MemberExpression);
            }


            private static ExcludedField Create(MemberExpression memberExpression)
            {
                if (memberExpression == null)
                {
                    throw new ArgumentException("Only simple type member selection are supported.", nameof(memberExpression));
                }

                return new ExcludedField(memberExpression.Member.Name, memberExpression.Member.DeclaringType.Name);
            }
        }

        #endregion


        #region "Variables"

        // Black list of fields that cannot be reset
        private static readonly Lazy<IEnumerable<ExcludedField>> EXCLUDED_FIELDS = new Lazy<IEnumerable<ExcludedField>>(() => new[]
            {
                ExcludedField.Create(() => AssemblyDiscoveryHelper.AssemblyDiscovery)
            });

        private HashSet<string> mAssembliesToInitialize;
        private HashSet<string> mInitializedAssemblies;

        private readonly HashSet<Type> mKnownGenericTypes = new HashSet<Type>();

        private List<FieldInfo> mFields;
        private List<Type> mInitSequence;

        private static readonly Dictionary<Type, bool> mCompilerGenerated = new Dictionary<Type, bool>();
        private static readonly Dictionary<FieldInfo, bool> mHasThreadStaticFields = new Dictionary<FieldInfo, bool>();

        private bool mLoaded;

        #endregion


        #region "Properties"

        /// <summary>
        /// Assembly condition for resetting state. Default if prefix "CMS."
        /// </summary>
        internal Func<Assembly, bool> AssemblyCondition
        {
            get;
            set;
        } = StartsWithCMS;

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the assembly name starts with CMS
        /// </summary>
        /// <param name="asm">Assembly</param>
        internal static bool StartsWithCMS(Assembly asm)
        {
            return asm.GetName().Name.StartsWith("CMS.");
        }


        /// <summary>
        /// Resets the 
        /// </summary>
        internal void Reset()
        {
            // Ensure collections for the reset
            EnsureSequence();

            // Read first to ensure dependent initialization to happen, otherwise 
            ReadFields(mFields);

            try
            {
                // Reset fields to type default values
                ResetFields(mFields);
            }
            catch (Exception)
            {
                // Do not throw exception if reset failed, we need to re-init static constructors
            }

            foreach (var type in mInitSequence)
            {
                // Run static constructors
                type.TypeInitializer.Invoke(null, null);
            }
            
            // Re-register known generic types
            foreach (var knownGenericType in mKnownGenericTypes)
            {
                TypeManager.RegisterGenericType(knownGenericType);
            }
        }


        /// <summary>
        /// Resets a specific type
        /// </summary>
        /// <param name="type">Type to reset</param>
        internal void ResetType(Type type)
        {
            // Reset fields to their default values
            var fields = GetFieldsToReset(type).ToList();

            ReadFields(fields);
            ResetFields(fields);

            // Re-initialize the fields
            RunStaticConstructor(type);
        }


        /// <summary>
        /// Runs a type static constructor to re-initialize its fields
        /// </summary>
        /// <param name="type">Type</param>
        private void RunStaticConstructor(Type type)
        {
            var typeInitializer = type.TypeInitializer;
            if (typeInitializer != null)
            {
                typeInitializer.Invoke(null, null);
            }
        }


        /// <summary>
        /// Gets the list of types to reset
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        private IEnumerable<Type> GetAssemblyTypes(IEnumerable<AssemblyName> assemblies)
        {
            // Filter supported types
            var types =
                assemblies
                    .Select(Assembly.Load)
                    .SelectMany(GetTypes)
                    .Where(TypeIsSupported);

            return types;
        }


        /// <summary>
        /// Gets types from the given assembly
        /// </summary>
        /// <param name="asm">Assembly</param>
        private IEnumerable<Type> GetTypes(Assembly asm)
        {
            try
            {
                // Get all types
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Get types which could be loaded. Filter by null, because the last type is null.
                return ex.Types.Where(type => (type != null));
            }
        }


        /// <summary>
        /// Gets the list of covered assemblies
        /// </summary>
        private IEnumerable<AssemblyName> GetCoveredAssemblies()
        {
            // Get assemblies from discovery 
            var assemblies = AssemblyDiscoveryHelper.GetAssemblies(false);

            // Always skip the Tests support assembly
            assemblies = assemblies.Where(asm => asm != typeof(AppStateReset).Assembly);

            // Filter to supported assemblies only, exclude the Tests assembly (current assembly)
            if (AssemblyCondition != null)
            {
                assemblies = assemblies.Where(AssemblyCondition);
            }

            var names = GetInitSequence(assemblies.ToList());

            return names;
        }


        /// <summary>
        /// Gets the initialization sequence for the given assemblies
        /// </summary>
        /// <param name="asmList">Assembly list</param>
        private IEnumerable<AssemblyName> GetInitSequence(List<Assembly> asmList)
        {
            mAssembliesToInitialize = new HashSet<string>(asmList.Select(asm => asm.FullName));
            mInitializedAssemblies = new HashSet<string>();

            foreach (var assembly in asmList)
            {
                var initSequence = GetInitSequence(assembly);

                foreach (var initAsm in initSequence)
                {
                    yield return initAsm;
                }
            }
        }


        /// <summary>
        /// Gets the initialization sequence for the given assembly
        /// </summary>
        /// <param name="asm">Assembly</param>
        private IEnumerable<AssemblyName> GetInitSequence(Assembly asm)
        {
            var fullName = asm.FullName;

            if (!mAssembliesToInitialize.Contains(fullName) || !mInitializedAssemblies.Add(fullName))
            {
                yield break;
            }

            var referenced = asm.GetReferencedAssemblies();

            foreach (var refAsmName in referenced)
            {
                Assembly refAsm;

                try
                {
                    refAsm = Assembly.Load(refAsmName);
                }
                catch
                {
                    continue;
                }

                // Load inner references
                var initSequence = GetInitSequence(refAsm);

                foreach (var initAsm in initSequence)
                {
                    yield return initAsm;
                }
            }

            yield return asm.GetName();
        }


        /// <summary>
        /// Checks if the given type is supported for reset
        /// </summary>
        /// <param name="type">Type to check</param>
        private bool TypeIsSupported(Type type)
        {
            return
                !type.IsInterface &&
                !type.ContainsGenericParameters &&
                !IsCompilerGenerated(type);
        }


        /// <summary>
        /// Checks if the given type if compiler generated type
        /// </summary>
        /// <param name="type">Type to check</param>
        internal static bool IsCompilerGenerated(Type type)
        {
            bool result;
            if (mCompilerGenerated.TryGetValue(type, out result))
            {
                return result;
            }

            result = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            mCompilerGenerated[type] = result;

            return result;
        }


        private void EnsureSequence()
        {
            // Get the registered generic types
            var genericTypes = TypeManager.GetRegisteredGenericTypes().ToList();
            var newGenericTypes = new List<Type>();

            // Reload the data if there are some new generic types (may influence init sequence)
            foreach (var genericType in genericTypes)
            {
                if (mKnownGenericTypes.Add(genericType))
                {
                    newGenericTypes.Add(genericType);
                    // Cause reload if the registered generic type was not found
                    mLoaded = false;
                }
            }

            if (mLoaded)
            {
                return;
            }

            // Get the types to reset. Merge assembly types with generic types
            if (mInitSequence == null)
            {
                var assemblyList = GetCoveredAssemblies().ToList();
                var types = GetFilteredTypes(GetAssemblyTypes(assemblyList));

                var sequence = new InitSequenceAnalyzer(types, assemblyList);

                // Create default list of types
                mFields = GetFieldsToReset(types).ToList();
                mInitSequence = sequence.GetInitSequence();
            }

            if (newGenericTypes.Count > 0)
            {
                var types = GetFilteredTypes(newGenericTypes);
                var assemblyList = GetGenericTypes(newGenericTypes);

                var newSequence = new InitSequenceAnalyzer(types, assemblyList);

                // Append new types
                mFields = mFields.Union(GetFieldsToReset(types)).ToList();
                mInitSequence = mInitSequence.Union(newSequence.GetInitSequence()).ToList();
            }

            mLoaded = true;
        }


        private static List<Type> GetFilteredTypes(IEnumerable<Type> types)
        {
            return types
                .Where(type => type != null)
                .Where(type => EXCLUDED_FIELDS.Value.All(excludedField => excludedField.TypeName != type.Name))
                .ToList();
        }


        private static IEnumerable<AssemblyName> GetGenericTypes(IEnumerable<Type> types)
        {
            return types
                .Where(type => type != null)
                .Select(type => type.Assembly.GetName())
                .ToList();
        }


        /// <summary>
        /// Reads all the given fields to cause default initialization of types
        /// </summary>
        /// <param name="fields">Fields to read</param>
        [HandleProcessCorruptedStateExceptions]
        private void ReadFields(IEnumerable<FieldInfo> fields)
        {
            foreach (var field in fields)
            {
                try
                {
                    field.GetValue(null);
                }
                catch
                {
                    // Suppress error on read
                }
            }
        }


        /// <summary>
        /// Resets the given list of fields to their type defaults
        /// </summary>
        /// <param name="fields">Fields to reset</param>
        [HandleProcessCorruptedStateExceptions]
        private void ResetFields(IEnumerable<FieldInfo> fields)
        {
            var fieldsList = fields.ToList();

            // Process all not-thread-static fields in parallel
            Parallel.ForEach(fieldsList.Where(field => !HasThreadStaticFields(field)), ResetField);

            // Process all thread-static fields in sequence
            foreach (var field in fieldsList.Where(HasThreadStaticFields))
            {
                ResetField(field);
            }
        }


        private bool HasThreadStaticFields(FieldInfo fi)
        {
            bool result;
            if (mHasThreadStaticFields.TryGetValue(fi, out result))
            {
                return result;
            }

            result = fi.GetCustomAttributes(typeof(ThreadStaticAttribute), false).Any();
            mHasThreadStaticFields[fi] = result;

            return result;
        }


        private void ResetField(FieldInfo field)
        {
            var declaringType = field.DeclaringType;
            if (declaringType == null)
            {
                return;
            }

            try
            {
                var fieldType = field.FieldType;
                var defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;

                field.SetValue(null, defaultValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to set static field '" + declaringType.FullName + "." + field.Name + "'.", ex);
            }
        }


        /// <summary>
        /// Clears the class static fields of the given type to default type values
        /// </summary>
        /// <param name="types">Types</param>
        private IEnumerable<FieldInfo> GetFieldsToReset(IEnumerable<Type> types)
        {
            // Reset all fields to their original values
            foreach (var type in types)
            {
                var fields = GetFieldsToReset(type);

                foreach (var fieldInfo in fields)
                {
                    yield return fieldInfo;
                }
            }
        }


        /// <summary>
        /// Clears the class static fields of the given type to default type values
        /// </summary>
        /// <param name="type">Type</param>
        private IEnumerable<FieldInfo> GetFieldsToReset(Type type)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (CannotResetField(field))
                {
                    continue;
                }

                yield return field;
            }
        }


        /// <summary>
        /// Returns <c>true</c> if the application reset is able to reset the given field
        /// </summary>
        /// <param name="field">Field to check</param>
        internal static bool CannotResetField(FieldInfo field)
        {
            return field.IsLiteral || // Do not reset constants
                                      //field.IsInitOnly || // Do not reset init only fields
                   IsExcludedField(field) ||
                   IsSystemField(field); // Do not reset special system fields
        }



        /// <summary>
        /// Returns <c>true</c> if the given field is not explicitly excluded in <see cref="EXCLUDED_FIELDS"/>.
        /// </summary>
        /// <param name="field">Field to check</param>
        internal static bool IsExcludedField(FieldInfo field)
        {
            return EXCLUDED_FIELDS
                .Value
                .Any(excludedField => field.Name.Contains(excludedField.FieldName) && field.DeclaringType.Name == excludedField.TypeName);
        }


        /// <summary>
        /// Returns <c>true</c> if the given field is a special system field
        /// </summary>
        /// <param name="field">Field to check</param>
        internal static bool IsSystemField(FieldInfo field)
        {
            return field.Name.Contains("__CachedAnonymousMethodDelegate");
        }


        /// <summary>
        /// Gets the current field state of the given type and object instance
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="instance">Instance</param>
        public static List<FieldState> GetFieldState(Type type, object instance)
        {
            return
                type.GetFields().Select(
                        f => new FieldState(f, f.GetValue((f.IsStatic ? null : instance)))
                ).ToList();
        }


        /// <summary>
        /// Restores the field state for the given object
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="fieldStates">Field states returned earlier by <see cref="GetFieldState" /></param>
        public static void RestoreFieldState(object instance, IEnumerable<FieldState> fieldStates)
        {
            if (fieldStates == null)
            {
                return;
            }

            foreach (var state in fieldStates)
            {
                var field = state.Field;

                field.SetValue(field.IsStatic ? null : instance, state.Value);
            }
        }

        #endregion
    }
}
