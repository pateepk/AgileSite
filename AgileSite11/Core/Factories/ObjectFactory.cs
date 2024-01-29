using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace CMS.Core
{
    /// <summary>
    /// Object factory based on the given type
    /// </summary>
    public class ObjectFactory : ObjectFactory<object>
    {
        #region "Variables"

        /// <summary>
        /// Dictionary of cached methods for creating objects by the given type
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> mGetFactoryMethods = new Dictionary<Type, MethodInfo>();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Object type</param>
        public ObjectFactory(Type type)
        {
            UseCompiledLambda = false;
            CreatedType = type;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates an object of the given type
        /// </summary>
        /// <param name="type">Type of the created object</param>
        public static object New(Type type)
        {
            return GetFactory(type).CreateNewObject();
        }

        /// <summary>
        /// Gets the factory for the given type
        /// </summary>
        /// <param name="type">Type of the requested object factory</param>
        public static IObjectFactory GetFactory(Type type)
        {
            MethodInfo genericMethodInfo;

            // Cache the method to create object
            if (!mGetFactoryMethods.TryGetValue(type, out genericMethodInfo))
            {
                lock (mGetFactoryMethods)
                {
                    if (!mGetFactoryMethods.TryGetValue(type, out genericMethodInfo))
                    {
                        var methodInfo = typeof(ObjectFactory).GetMethod("GetFactoryGeneric", BindingFlags.Static | BindingFlags.NonPublic);

                        genericMethodInfo = methodInfo.MakeGenericMethod(type);

                        mGetFactoryMethods.Add(type, genericMethodInfo);
                    }
                }
            }

            var fact = (IObjectFactory)genericMethodInfo.Invoke(null, null);

            return fact;
        }


        /// <summary>
        /// Creates an object by an external factory of the given type
        /// </summary>
        protected static IObjectFactory GetFactoryGeneric<TObject>()
        {
            return ObjectFactory<TObject>.CurrentFactory;
        }

        #endregion
    }


    /// <summary>
    /// Class that provides construction of the objects of given class
    /// </summary>
    public class ObjectFactory<T> : IObjectFactory
    {
        #region "Variables"

        private static IObjectFactory mCurrentFactory;

        private static bool mIsFactoryInitialized;

        private static bool mCanReplaceCurrent;

        private object mSingleton;

        private Type mCreatedType = typeof(T);

        // Type that is truly created for T. This member must be accessed from lock context of lockObject
        private static Type mCurrentRegisteredType;

        private static readonly object lockObject = new object();

        // Flag for preventing loops in constructor resolution. To be set from context of mConstructorResolutionLock only
        private bool mConstructorResolutionInProgress;

        // Lock ensuring mutual exclusion when resolving constructor
        private readonly object mConstructorResolutionLock = new object();

        /// <summary>
        /// Activator delegate
        /// </summary>
        protected delegate T ObjectActivator();

        private ObjectActivator mActivator;
        private ConstructorInfo mConstructor;

        #endregion


        #region "Static properties"

        /// <summary>
        /// If true, the default object factory can be replaced by other factory
        /// </summary>
        public static bool CanReplaceCurrent
        {
            get
            {
                Initialize();

                return mCanReplaceCurrent;
            }
        }


        /// <summary>
        /// Returns true if the default factory is initialized
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return CurrentFactory.CanCreateObject(null);
            }
        }


        /// <summary>
        /// Current factory to create objects.
        /// </summary>
        public static IObjectFactory CurrentFactory
        {
            get
            {
                Initialize();
                
                return mCurrentFactory;
            }
        }
        

        /// <summary>
        /// Returns true if the factory is capable of instantiating an object
        /// </summary>
        /// <param name="factory">Factory</param>
        private static bool CanInstantiateObject(IObjectFactory factory)
        {
            var createdType = factory.CreatedType;

            return !createdType.IsInterface && !createdType.IsAbstract;
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the type created by this factory
        /// </summary>
        public Type CreatedType
        {
            get
            {
                return mCreatedType;
            }
            protected set
            {
                mCreatedType = value;
            }
        }


        /// <summary>
        /// Object initialization function
        /// </summary>
        public Func<T, object> Initializer
        {
            get;
            set;
        }


        /// <summary>
        /// Singleton instance of the created object type
        /// </summary>
        public object Singleton
        {
            get
            {
                if (mSingleton == null)
                {
                    lock (this)
                    {
                        if (mSingleton == null)
                        {
                            mSingleton = CreateNewObject();
                        }
                    }
                }

                return mSingleton;
            }
        }


        /// <summary>
        /// Compiled object activator
        /// </summary>
        private ObjectActivator ActivatorDelegate
        {
            get
            {
                return mActivator ?? (mActivator = CreateActivator());
            }
        }


        /// <summary>
        /// If true, the compiled lambda expression is used to create new objects. Set to true if the factory will be creating a lot of objects
        /// </summary>
        protected bool UseCompiledLambda
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor used for dependency injection
        /// </summary>
        protected ConstructorInfo Constructor
        {
            get
            {
                if (mConstructor == null)
                {
                    lock (mConstructorResolutionLock)
                    {
                        if (mConstructor == null)
                        {
                            mConstructor = GetConstructor();
                        }
                    }
                }

                return mConstructor;
            }
            set
            {
                mConstructor = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates and returns a new object
        /// </summary>
        public virtual object CreateNewObject()
        {
            if (!CanInstantiateObject(this))
            {
                throw new NotSupportedException(String.Format("[ObjectFactory.CurrentObjectFactory]: Implementation for type '{0}' was not found, make sure that assembly with implementation is present in the bin folder of your application. If the assembly is present, the implementation must be registered by call to ObjectFactory<{0}>.SetObjectTypeTo<SomeType>() to initialize the type for this interface.", CreatedType.Name));
            }

            // Create the object
            T result = (T)CreateInstance();

            if (Initializer != null)
            {
                Initializer(result);
            }

            return result;
        }


        /// <summary>
        /// Creates a new instance of the object
        /// </summary>
        protected object CreateInstance()
        {
            try
            {
                // Use activator delegate
                if (UseCompiledLambda)
                {
                    return ActivatorDelegate();
                }

                // Create value type using activator
                if (CreatedType.IsValueType)
                {
                    return Activator.CreateInstance(CreatedType);
                }

                // Create other types using constructor
                return CreateInstance(Constructor);
            }
            catch (TargetInvocationException ex)
            {
                // Throw inner exception to pass exception further
                throw ex.InnerException;
            }
        }


        /// <summary>
        /// Creates a new instance using dependency injection with automatic parameters
        /// </summary>
        /// <param name="ctor">Constructor</param>
        private object CreateInstance(ConstructorInfo ctor)
        {
            if (ctor == null)
            {
                throw new MissingMethodException("[ObjectFactory.CreateInstance]: Could not find proper constructor for type '" + CreatedType.FullName + "'. The type must have an empty constructor or a constructor that can be used for dependency injection. For DI, there must be a single constructor with the largest number of parameters that can be resolved by the object factory.");
            }

            // Get the parameters
            var parameterInfos = ctor.GetParameters();
            if (parameterInfos.Length == 0)
            {
                return ctor.Invoke(null);
            }

            // Prepare parameters for constructor
            var parameters = new object[parameterInfos.Length];
            var index = 0;

            foreach (var par in parameterInfos)
            {
                // Use default value for optional value type parameter
                var useDefaultValue = (par.IsOptional && par.ParameterType.IsValueType);

                object parValue = useDefaultValue ? par.DefaultValue : ObjectFactory.New(par.ParameterType);

                parameters[index] = parValue;

                index++;
            }

            return ctor.Invoke(parameters);
        }


        /// <summary>
        /// Gets the dependency injection constructor for the created type.
        /// Prevents loops when resolving parameters for constructor.
        /// Must be called from synchronzation context of <see cref="mConstructorResolutionLock"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a circular dependency in constructors parameters is detected.</exception>
        private ConstructorInfo GetConstructor()
        {
            // If default constructor is available, get it primarily. No protection from loops is necessary at this point.
            var defaultCtor = CreatedType.GetConstructor(Type.EmptyTypes);
            if (defaultCtor != null)
            {
                return defaultCtor;
            }

            if (mConstructorResolutionInProgress)
            {
                throw new InvalidOperationException(String.Format("A loop was detected when resolving constructor for type '{0}'. This is typically a result of circular dependency in types' constructors, which is unsupported.", mCreatedType));
            }

            try
            {
                mConstructorResolutionInProgress = true;

                return GetConstructorCore();
            }
            finally
            {
                mConstructorResolutionInProgress = false;
            }

        }


        /// <summary>
        /// Gets the dependency injection constructor for the created type.
        /// </summary>
        private ConstructorInfo GetConstructorCore()
        {
            // Try other constructors for which DI can be provided, starting from the largest ones
            var topCtors = CreatedType
                    .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(GetParametersCount)
                    // CanProvideAllParameters is a possible source of loops
                    // A loop occurs when there's a circular dependency in parameter types in constructors
                    .Where(CanProvideAllParameters)
                    .Take(2)
                    .ToList();

            // For only one result, return it
            switch (topCtors.Count)
            {
                case 1:
                    // Only single match - Right constructor found
                    return topCtors[0];

                case 2:
                    // If more than one matches, the top one has to have the largest number of parameters
                    if (GetParametersCount(topCtors[0]) > GetParametersCount(topCtors[1]))
                    {
                        return topCtors[0];
                    }
                    break;
            }

            return null;
        }


        /// <summary>
        /// Gets the number of parameters of the given constructor
        /// </summary>
        /// <param name="ctor">Constructor</param>
        private int GetParametersCount(ConstructorInfo ctor)
        {
            return ctor.GetParameters().Length;
        }


        /// <summary>
        /// Checks if the given constructor can fake its parameters
        /// </summary>
        /// <param name="ctor">Constructor to check</param>
        private bool CanProvideAllParameters(ConstructorInfo ctor)
        {
            var parameters = ctor.GetParameters();

            return parameters.All(ParameterCanBeProvided);
        }


        /// <summary>
        /// Returns true if the given parameter can be provided
        /// </summary>
        /// <param name="parameterInfo">Parameter info</param>
        private bool ParameterCanBeProvided(ParameterInfo parameterInfo)
        {
            // Value type parameters that are not optional are not allowed, optional value types use their default values
            var type = parameterInfo.ParameterType;
            if (type.IsValueType)
            {
                return parameterInfo.IsOptional;
            }

            // Following types cannot be used as generic arguments
            if (type.IsPointer || type.IsByRef || (type == typeof(void)))
            {
                return false;
            }

            // Get the factory to provide the value for parameter
            var fact = ObjectFactory.GetFactory(type);
            if (fact == null)
            {
                return false;
            }

            return fact.CanCreateObject(null);
        }


        /// <summary>
        /// Creates an object activator for the created type
        /// </summary>
        protected virtual ObjectActivator CreateActivator()
        {
            // Get the type constructor
            var ctor = CreatedType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new MissingMethodException("[ObjectFactory.CreateActivator]: Cannot create activator for the type " + CreatedType.FullName + ", empty constructor wasn't found.");
            }

            var newExp = Expression.New(ctor);

            // Create a lambda with the New expression as body
            var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, null);

            // Compile it
            return (ObjectActivator)lambda.Compile();
        }


        /// <summary>
        /// Returns true if the factory is able to create the object based on the given parameter
        /// </summary>
        /// <param name="parameter">Object parameter</param>
        public virtual bool CanCreateObject(object parameter)
        {
            if (UseCompiledLambda)
            {
                return (ActivatorDelegate != null);
            }

            var type = typeof(T);

            // Value type can be always created
            if (type.IsValueType)
            {
                return true;
            }

            // Interface / abstract type can never be created
            if (type.IsInterface || type.IsAbstract)
            {
                return false;
            }

            return (Constructor != null);
        }


        /// <summary>
        /// Creates and returns a new object
        /// </summary>
        public T CreateNewTypedObject()
        {
            return (T)CreateNewObject();
        }

        #endregion


        #region "Static methods"
        
        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectFactory()
        {
            TypeManager.RegisterGenericType(typeof(ObjectFactory<T>));
        }
        

        /// <summary>
        /// Creates a new object of the given type
        /// </summary>
        public static T New()
        {
            return (T)CurrentFactory.CreateNewObject();
        }


        /// <summary>
        /// Initializes factory so that it uses plain object factory instance.
        /// The initializer is not responsible for extensions (decorators) initialization.
        /// </summary>
        /// <remarks>
        /// The factory can be initialized explicitly by calling <see cref="SetObjectTypeTo{NewType}(bool, bool)"/> or <see cref="SetObjectTypeTo(Type, bool, bool)"/>.
        /// </remarks>
        private static void Initialize()
        {
            // This method is a complement to SetObjectTypeTo. Keep that in mind when making changes.
            if (mIsFactoryInitialized)
            {
                return;
            }

            lock (lockObject)
            {
                // Acquisition of any locks within this critical section should be avoided (especially lock for AppCore.PreInit) as it may cause deadlock

                if (!mIsFactoryInitialized)
                {
                    // Do not try re-initialize default implementation 
                    if (!TypeManager.PreInitialized && mCurrentFactory != null)
                    {
                        return;
                    }

                    // Ensure that the assembly resolver is initialized. The rest of the application core is not to be initialized in this critical section (as it may result in a deadlock).
                    AppCore.InitializeAssemblyResolver();

                    mCurrentFactory = new ObjectFactory<T>();
                    mCurrentRegisteredType = typeof(T);

                    Thread.MemoryBarrier();

                    // Default implementation is not considered as final implementation until type pre-ilization is complete
                    if (TypeManager.PreInitialized || CanInstantiateObject(mCurrentFactory))
                    {
                        mIsFactoryInitialized = true;
                    }
                }
            }
        }


        /// <summary>
        /// Sets the object type created by the factory to the given type.
        /// </summary>
        /// <param name="canBeReplaced">If true, the given object type can be replaced by other object type</param>
        /// <param name="replaceExisting">If true, the service replaces existing if service is already initialized</param>
        public static void SetObjectTypeTo<NewType>(bool canBeReplaced = false, bool replaceExisting = true)
            where NewType : T
        {
            SetObjectTypeTo(typeof(NewType), canBeReplaced, replaceExisting);
        }


        /// <summary>
        /// Sets the object type created by the factory to the given type.
        /// </summary>
        /// <param name="newType">Type to be crated by the factory.</param>
        /// <param name="canBeReplaced">If true, the given object type can be replaced by other object type</param>
        /// <param name="replaceExisting">If true, the service replaces existing if service is already initialized</param>
        public static void SetObjectTypeTo(Type newType, bool canBeReplaced = false, bool replaceExisting = true)
        {
            // The method is a complement to Initialize when it comes to setting object type of uninitialized factory. Keep that in mind when making changes.
            lock (lockObject)
            {
                if (mIsFactoryInitialized)
                {
                    if (!mCanReplaceCurrent)
                    {
                        throw new Exception("Default implementation '" + mCurrentRegisteredType.FullName + "' for type '" + typeof(T).FullName + "' does not allow replacement. Implementation '" + newType.FullName + "' can not be registered.");
                    }
                    if (!replaceExisting)
                    {
                        return;
                    }
                }

                SetObjectTypeToInternal(newType);

                mCanReplaceCurrent = canBeReplaced;

                if (!mIsFactoryInitialized)
                {
                    Thread.MemoryBarrier();

                    mIsFactoryInitialized = true;
                }
            }
        }


        /// <summary>
        /// Replaces the creation of the given original type by the new type
        /// </summary>
        /// <param name="newType">New type</param>
        private static void SetObjectTypeToInternal(Type newType)
        {
            if (!typeof(T).IsAssignableFrom(newType))
            {
                throw new NotSupportedException("Implementation '" + newType.FullName + "' can not be registered for type '" + typeof(T).FullName + "' as it is of incompatible type.");
            }

            lock (lockObject)
            {
                var newFactory = new ObjectFactory(newType);

                mCurrentFactory = newFactory;
                mCurrentRegisteredType = newType;
            }
        }


        /// <summary>
        /// Provides a static singleton instance of the object implementing this interface
        /// </summary>
        public static T StaticSingleton()
        {
            return (T)CurrentFactory.Singleton;
        }


        /// <summary>
        /// Returns static singleton for the given object type
        /// </summary>
        public static T StaticSingleton<TParent>()
        {
            if (SingletonStorage<TParent, T>.Singleton == null)
            {
                SingletonStorage<TParent, T>.Singleton = New();
            }

            return SingletonStorage<TParent, T>.Singleton;
        }

        #endregion
    }
}