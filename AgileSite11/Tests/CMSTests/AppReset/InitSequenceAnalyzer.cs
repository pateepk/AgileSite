using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace CMS.Tests
{
    /// <summary>
    /// Analyzes the init sequence of the given list of types
    /// </summary>
    internal class InitSequenceAnalyzer
    {
        private readonly HashSet<Type> mProcessedTypes;
        private readonly HashSet<string> mProcessedAssemblies;

        private HashSet<MemberInfo> mVisitedMembers;
        private HashSet<Type> mInitializedTypes;

        private HashSet<Type> mInstantiatedTypes;

        private Stack<string> mMethods;
        
        private readonly List<Type> typeList;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="types">List of types to analyze</param>
        /// <param name="assemblies">Assemblies</param>
        public InitSequenceAnalyzer(List<Type> types, IEnumerable<AssemblyName> assemblies)
        {
            typeList = types;

            mProcessedTypes = new HashSet<Type>(typeList);
            mProcessedAssemblies = new HashSet<string>(assemblies.Select(asm => asm.FullName));
        }


        /// <summary>
        /// Prepares the necessary collections for analysis of the init sequence
        /// </summary>
        private void PrepareCollections()
        {
            // Mark types that need initialization
            mVisitedMembers = new HashSet<MemberInfo>();
            mInstantiatedTypes = new HashSet<Type>();
            mInitializedTypes = new HashSet<Type>();
            mMethods = new Stack<string>();
        }


        /// <summary>
        /// Analyzes the init sequence and returns the ordered list of types to initialize
        /// </summary>
        public List<Type> GetInitSequence()
        {
            PrepareCollections();

            return GetInitSequence(typeList).ToList();
        }


        /// <summary>
        /// Gets the init sequence for the given type list
        /// </summary>
        /// <param name="types">Type list</param>
        private IEnumerable<Type> GetInitSequence(IEnumerable<Type> types)
        {
            // Process all types
            foreach (var type in types)
            {
                // Gets the underlying init sequence for the given type
                var initSequence = GetInitSequence(type);

                foreach (var initType in initSequence)
                {
                    yield return initType;
                }
            }
        }


        /// <summary>
        /// Runs the static constructor of the given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="indent">Call indentation</param>
        private IEnumerable<Type> GetInitSequence(Type type, int indent = 0)
        {
            // Remove type from types that need initialization
            if (!IsProcessedType(type) || !mInitializedTypes.Add(type))
            {
                yield break;
            }

            var ctr = type.TypeInitializer;
            if (ctr != null)
            {
                var initSequence = GetInitSequence(ctr, false, indent + 1);
                foreach (var initType in initSequence)
                {
                    yield return initType;
                }

                // Call static constructor
                yield return type;
            }
        }


        /// <summary>
        /// Returns true if the given type is allowed to be processed
        /// </summary>
        /// <param name="type">Type to check</param>
        private bool IsProcessedType(Type type)
        {
            return mProcessedAssemblies.Contains(type.Assembly.FullName);
        }


        /// <summary>
        /// Gets the init sequence for the given method
        /// </summary>
        /// <param name="method">Method to analyze</param>
        /// <param name="processInherited">If true, processing of the inherited (overridden) methods is allowed</param>
        /// <param name="indent">Log indentation</param>
        private IEnumerable<Type> GetInitSequence(MethodBase method, bool processInherited, int indent)
        {
            var methodDeclaringType = method.DeclaringType;

            // Skip methods without declaring type, and methods of types which are not processed at all
            if ((methodDeclaringType == null) || !IsProcessedType(methodDeclaringType))
            {
                yield break;
            }

            // Add visited member
            if (mVisitedMembers.Add(method))
            {
                var methodFullName = methodDeclaringType.FullName + "." + method.Name;

                mMethods.Push(methodFullName);

                // Mark instantiated types when instance constructor is called (these types might occur under their base classes when called within the tree)
                var newInstance = method.IsConstructor && !method.IsStatic;
                if (newInstance)
                {
                    mInstantiatedTypes.Add(methodDeclaringType);
                }
                else if (method == methodDeclaringType.TypeInitializer)
                {
                    // Do not log type initializer call
                    indent--;
                }
                else
                {
                    
                    // Check call to AppCore.PreInit, initialization is not allowed to be raised by static constructors
                    if (methodFullName == "CMS.Core.AppCore.PreInit")
                    {
                        throw new Exception(
@"Application pre-initialization is not allowed during initialization of static fields. Unwanted initialization may be caused by accessing API which needs core initialization from static field initialization or static class constructor. Such code should be converted to lazy initialization.

Method stack causing pre-initialization:

" +
                            String.Join("\r\n", mMethods.ToArray())
                        );
                    }
                }

                // For interfaces and abstract/virtual methods cascade through all implementing methods of inherited types
                if (processInherited &&
                    !method.IsConstructor &&
                    !method.IsStatic &&
                    (method.IsAbstract || method.IsVirtual))
                {
                    var initSequence = GetInitSequenceFromInheritedMethods(method, indent);

                    foreach (var initType in initSequence)
                    {
                        yield return initType;
                    }
                }

                // Cascade through method body
                if (method.GetMethodBody() != null)
                {
                    var initSequence = GetInitSequenceFromMethodBody(method, indent);

                    foreach (var initType in initSequence)
                    {
                        yield return initType;
                    }
                }

                mMethods.Pop();
            }
        }


        /// <summary>
        /// Gets the initialization sequence from the given method body
        /// </summary>
        /// <param name="method">Method to examine</param>
        /// <param name="indent">Log indent</param>
        private IEnumerable<Type> GetInitSequenceFromMethodBody(MethodBase method, int indent)
        {
            var members = GetReferencedMembers(method);

            foreach (var member in members)
            {
                var declaringType = member.DeclaringType;
                if (declaringType != null)
                {
                    var fi = member as FieldInfo;
                    if ((fi != null) && fi.IsStatic)
                    {
                        // Reference to other type, initialize if not initialized yet
                        var initSequence = GetInitSequence(declaringType, indent + 1);
                        foreach (var initType in initSequence)
                        {
                            yield return initType;
                        }
                    }

                    var mi = member as MethodBase;
                    if (mi != null)
                    {
                        if ( // Process constructors and methods that are not auto-generated by compiler
                            (mi.IsConstructor || !AppStateReset.IsCompilerGenerated(declaringType)) &&
                            // Process methods in explicitly processed types or constructed generic types (types that may not be registered)
                            (mProcessedTypes.Contains(declaringType) || (declaringType.IsGenericType && !declaringType.ContainsGenericParameters)))
                        {
                            // Reference to other type, initialize if not initialized yet
                            var initSequence = GetInitSequence(mi, true, indent + 1);

                            foreach (var initType in initSequence)
                            {
                                yield return initType;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the list of members directly referenced by the given method. Does not examine the members recursively.
        /// </summary>
        /// <param name="method">Method to check</param>
        private static IEnumerable<MemberInfo> GetReferencedMembers(MethodBase method)
        {
            var instructions = MethodBodyReader.GetInstructions(method);

            foreach (var instruction in instructions)
            {
                // Skip instructions that are method references (delegates)
                if (instruction.OpCode == OpCodes.Ldftn)
                {
                    continue;
                }

                // Get the member info
                var mi = instruction.Operand as MemberInfo;
                if (mi != null)
                {
                    yield return mi;
                }
            }
        }


        /// <summary>
        /// Gets the init sequence for the method call from inherited types
        /// </summary>
        /// <param name="method">Base method</param>
        /// <param name="indent">Log indent</param>
        private IEnumerable<Type> GetInitSequenceFromInheritedMethods(MethodBase method, int indent)
        {
            var methodDeclaringType = method.DeclaringType;

            // Only process types that were instantiated in the processed chain of instantiation
            var inheritedTypes = mInstantiatedTypes.Where(type => (type != methodDeclaringType) && methodDeclaringType.IsAssignableFrom(type)).ToList();

            foreach (var inheritedType in inheritedTypes)
            {
                var parameters = method.GetParameters();

                Type type = inheritedType;

                var inheritedMethods =
                    inheritedType
                        .GetMethods()
                        .Where(m =>
                            (m.DeclaringType == type) &&
                            (m.Name == method.Name) &&
                            ParametersMatch(m.GetParameters(), parameters)
                        );

                foreach (var inheritedMethod in inheritedMethods)
                {
                    var initSequence = GetInitSequence(inheritedMethod, false, indent + 1);

                    foreach (var initType in initSequence)
                    {
                        yield return initType;
                    }
                }
            }
        }


        /// <summary>
        /// Checks if two given arrays of parameters match
        /// </summary>
        /// <param name="p1">First parameters</param>
        /// <param name="p2">Second parameters</param>
        private bool ParametersMatch(ParameterInfo[] p1, ParameterInfo[] p2)
        {
            if (p1.Length != p2.Length)
            {
                return false;
            }

            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i].ParameterType != p2[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if the static constructor of the given type calls the given method
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="registerMethod">Method to check</param>
        public static bool StaticConstructorCallsMethod(Type type, MethodInfo registerMethod)
        {
            return ((type.TypeInitializer != null) && GetReferencedMembers(type.TypeInitializer).Any(member => (member == registerMethod)));
        }
    }
}