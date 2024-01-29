using System;
using System.Collections.Generic;
using System.Threading;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Generator class for the various type of objects
    /// </summary>
    public class ObjectGenerator<TBase>
        where TBase : class
    {
        // Dictionary of the object factories [objectType -> IObjectFactory]
        private readonly Dictionary<string, IObjectFactory> mObjectFactories = new Dictionary<string, IObjectFactory>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<IObjectFactory> mDefaultFactories = new List<IObjectFactory>();
        private readonly ReaderWriterLockSlim generatorLock = new ReaderWriterLockSlim();
        private bool generatorUsed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultFactory">Default factory that will be used if no object type matches</param>
        public ObjectGenerator(IObjectFactory defaultFactory)
        {
            RegisterDefaultFactory(defaultFactory, false);
        }


        /// <summary>
        /// Creates new object of the given type
        /// </summary>
        /// <param name="parameter">Parameter used in <see cref="IObjectFactory.CanCreateObject(object)"/> method.</param>
        public TBase CreateNewObject(object parameter = null)
        {
            bool lockTaken = false;

            try
            {
                lockTaken = generatorLock.TryEnterReadLock(Timeout.Infinite);
                
                generatorUsed = true;

                if (mDefaultFactories.Count > 0)
                {
                    // Try default factories
                    foreach (IObjectFactory f in mDefaultFactories)
                    {
                        TBase result = TryFactory(f, parameter);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }

                return null;
            }
            finally
            {
                if (lockTaken)
                {
                    generatorLock.ExitReadLock();
                }
            }
        }


        /// <summary>
        /// Creates new object of the given type
        /// </summary>
        /// <param name="objectType">Object type</param>
        public TBase CreateNewObject(string objectType)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = generatorLock.TryEnterReadLock(Timeout.Infinite);

                generatorUsed = true;

                // Try to get registered object
                if (objectType != null)
                {
                    IObjectFactory factory;
                    if (mObjectFactories.TryGetValue(objectType, out factory))
                    {
                        var result = TryFactory(factory, objectType);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    generatorLock.ExitReadLock();
                }
            }

            // Create the object using the given parameter
            return CreateNewObject((object)objectType);
        }


        /// <summary>
        /// Tries to create an object using given factory
        /// </summary>
        /// <param name="factory">Factory to try</param>
        /// <param name="parameter">Object parameter</param>
        private static TBase TryFactory(IObjectFactory factory, object parameter)
        {
            if ((factory == null) || !factory.CanCreateObject(parameter))
            {
                return null;
            }

            // Create new object
            return (TBase)factory.CreateNewObject();
        }


        /// <summary>
        /// Registers the default object factory
        /// </summary>
        /// <param name="topPriority">If true, this object type is prioritized against other default factories</param>
        [Obsolete("Use RegisterDefaultFactory instead.")]
        public void RegisterDefaultObjectType<TObject>(bool topPriority)
            where TObject : class, TBase, new()
        {
            var newFac = new ObjectFactory<TObject>();
            RegisterDefaultFactory(newFac, topPriority);
        }


        /// <summary>
        /// Registers the default object factory
        /// </summary>
        /// <param name="factory">Object factory to register</param>
        /// <param name="topPriority">If true, this factory is prioritized against </param>
        public void RegisterDefaultFactory(IObjectFactory factory, bool topPriority)
        {
            if (factory == null)
            {
                return;
            }

            bool lockTaken = false;
            try
            {
                lockTaken = generatorLock.TryEnterWriteLock(Timeout.Infinite);

                ThrowExceptionIfGeneratorUsed();

                if (topPriority)
                {
                    // Register to the beginning
                    mDefaultFactories.Insert(0, factory);
                }
                else
                {
                    // Register to the end
                    mDefaultFactories.Add(factory);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    generatorLock.ExitWriteLock();
                }
            }
        }


        /// <summary>
        /// Registers the given object type
        /// </summary>
        /// <param name="objectType">Object type string</param>
        /// <param name="factory">Object factory</param>
        public void RegisterObjectType(string objectType, IObjectFactory factory)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = generatorLock.TryEnterWriteLock(Timeout.Infinite);

                ThrowExceptionIfGeneratorUsed(() => mObjectFactories.ContainsKey(objectType));

                mObjectFactories[objectType] = factory;
            }
            finally
            {
                if (lockTaken)
                {
                    generatorLock.ExitWriteLock();
                }
            }
        }


        /// <summary>
        /// Registers the given object type
        /// </summary>
        /// <param name="objectType">Object type string</param>
        public void RegisterObjectType<TObject>(string objectType)
            where TObject : class, TBase, new()
        {
            var factory = new ObjectFactory<TObject>();
            RegisterObjectType(objectType, factory);
        }


        private void ThrowExceptionIfGeneratorUsed(Func<bool> throwIfUsedCondition = null)
        {
            if (generatorUsed && (throwIfUsedCondition?.Invoke() ?? true))
            {
                throw new Exception("You cannot change the type of the generated objects after some objects were created by the generator.");
            }
        }
    }
}