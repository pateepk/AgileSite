using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    using ProcessorCreator = Func<AbstractFileSystemJob, ICustomProcessor>;


    /// <summary>
    /// Factory allowing <see cref="ICustomProcessor"/> registration.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// <para>By default, (only) separated fields are processed for each object type.</para>
    /// </remarks>
    public sealed class CustomProcessorFactory
    {
        #region "Fields"

        private static CustomProcessorFactory mInstance;

        // Factory methods organized by object type
        private readonly ConcurrentDictionary<string, List<ProcessorCreator>> mCustomProcessors = new ConcurrentDictionary<string, List<ProcessorCreator>>(StringComparer.InvariantCultureIgnoreCase);


        // Default processor(s)
        private readonly List<ProcessorCreator> mDefaultProcessors = new List<ProcessorCreator>
        {
            job => job.SeparatedFieldProcessor
        };


        /// <summary>
        /// The only instance of the class.
        /// </summary>
        internal static CustomProcessorFactory Instance
        {
            get
            {
                return mInstance ?? (mInstance = new CustomProcessorFactory());
            }
            set
            {
                mInstance = value;
            }
        } 

        #endregion


        #region "Static methods"


        /// <summary>
        /// Registers custom processor with parameterless constructor to <paramref name="objectType"/>.
        /// Processor will be instantiated by the parameterless constructor's call.
        /// </summary>
        /// <typeparam name="TCustomProcessor">A parameterless implementation of <see cref="ICustomProcessor"/>.</typeparam>
        /// <param name="objectType">Object type to associate the <typeparamref name="TCustomProcessor"/> with.</param>
        internal static void RegisterProcessor<TCustomProcessor>(string objectType)
            where TCustomProcessor : ICustomProcessor, new()
        {
            Instance.RegisterProcessorInternal(objectType, (job) => new TCustomProcessor());
        }


        /// <summary>
        /// Registers given <paramref name="processorMethod"/> to <paramref name="objectType"/>.
        /// Processor will be instantiated by calling the  <paramref name="processorMethod"/>.
        /// </summary>
        /// <param name="objectType">Object type to associate the <paramref name="processorMethod"/> with.</param>
        /// <param name="processorMethod">Method returning implementation of the <see cref="ICustomProcessor"/> interface that should handle given <paramref name="objectType"/>.</param>
        public static void RegisterProcessor(string objectType, ProcessorCreator processorMethod)
        {
            Instance.RegisterProcessorInternal(objectType, processorMethod);
        }


        /// <summary>
        /// Returns collection of all <see cref="ICustomProcessor"/>s associated with provided <paramref name="typeInfo"/> 
        /// and (eventually) its <see cref="ObjectTypeInfo.OriginalObjectType"/>. Also default processor(s) are yielded.
        /// </summary>
        /// <param name="job">Instance of <see cref="AbstractFileSystemJob"/> that requests the processors.</param>
        /// <param name="typeInfo">Type of object the processors should be gathered for.</param>
        /// <returns>Collection ordered for serialization – default are last, most specialized first (opposite to <see cref="GetDeserilizationProcessors"/>).</returns>
        /// <remarks><see cref="SeparatedFieldProcessor"/> is returned as the very last one.</remarks>
        internal static IEnumerable<ICustomProcessor> GetSerializationProcessors(AbstractFileSystemJob job, ObjectTypeInfo typeInfo)
        {
            return Instance.GetSerializationProcessorsInternal(job, typeInfo);
        }


        /// <summary>
        /// Returns collection of all <see cref="ICustomProcessor"/>s associated with provided <paramref name="typeInfo"/> 
        /// and (eventually) its <see cref="ObjectTypeInfo.OriginalObjectType"/>. Also default processor(s) are yielded.
        /// </summary>
        /// <param name="job">Instance of <see cref="AbstractFileSystemJob"/> that requests the processors.</param>
        /// <param name="typeInfo">Type of object the processors should be gathered for.</param>
        /// <returns>Collection ordered for deserialization – default are first, most specialized last (opposite to <see cref="GetSerializationProcessors"/>.</returns>
        /// <remarks><see cref="SeparatedFieldProcessor"/> is returned as the very first one.</remarks>
        internal static IEnumerable<ICustomProcessor> GetDeserilizationProcessors(AbstractFileSystemJob job, ObjectTypeInfo typeInfo)
        {
            return Instance.GetDeserializationProcessorsInternal(job, typeInfo);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers given <paramref name="processorMethod"/> to <paramref name="objectType"/>.
        /// Processor will be instantiated by calling the  <paramref name="processorMethod"/>.
        /// </summary>
        /// <param name="objectType">Object type to associate the <paramref name="processorMethod"/> with.</param>
        /// <param name="processorMethod">Method returning implementation of the <see cref="ICustomProcessor"/> interface that should handle given <paramref name="objectType"/>.</param>
        private void RegisterProcessorInternal(string objectType, ProcessorCreator processorMethod)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType", "Object type can not be null.");
            }
            if (processorMethod == null)
            {
                throw new ArgumentNullException("processorMethod", "Processor method can not be null.");
            }

            var processorMethods = mCustomProcessors.GetOrAdd(objectType, (type) => new List<ProcessorCreator>());
            if (!processorMethods.Contains(processorMethod))
            {
                processorMethods.Add(processorMethod);
            }
        }


        /// <summary>
        /// Returns collection of all <see cref="ICustomProcessor"/>s associated with provided <paramref name="typeInfo"/> 
        /// and (eventually) its <see cref="ObjectTypeInfo.OriginalObjectType"/>. Also default processor(s) are yielded.
        /// </summary>
        /// <param name="job">Instance of <see cref="AbstractFileSystemJob"/> that requests the processors.</param>
        /// <param name="typeInfo">Type of object the processors should be gathered for.</param>
        /// <returns>Collection ordered for serialization – default are last, most specialized first (opposite to <see cref="GetDeserilizationProcessors"/>).</returns>
        private IEnumerable<ICustomProcessor> GetSerializationProcessorsInternal(AbstractFileSystemJob job, ObjectTypeInfo typeInfo)
        {
            CheckArguments(job, typeInfo);

            // Processors registered for the type first
            List<ProcessorCreator> processorList;
            if (mCustomProcessors.TryGetValue(typeInfo.ObjectType, out processorList))
            {
                foreach (var processorMethod in processorList)
                {
                    yield return processorMethod(job);
                }
            }

            // Processors registered for the original type second
            if ((typeInfo.OriginalTypeInfo != null) && mCustomProcessors.TryGetValue(typeInfo.OriginalTypeInfo.ObjectType, out processorList))
            {
                foreach (var processorMethod in processorList)
                {
                    yield return processorMethod(job);
                }
            }

            // Default processors third
            foreach (var processorMethod in mDefaultProcessors)
            {
                yield return processorMethod(job);
            }
        }


        /// <summary>
        /// Returns collection of all <see cref="ICustomProcessor"/>s associated with provided <paramref name="typeInfo"/> 
        /// and (eventually) its <see cref="ObjectTypeInfo.OriginalObjectType"/>. Also default processor(s) are yielded.
        /// </summary>
        /// <param name="job">Instance of <see cref="AbstractFileSystemJob"/> that requests the processors.</param>
        /// <param name="typeInfo">Type of object the processors should be gathered for.</param>
        /// <returns>Collection ordered for deserialization – default are first, most specialized last (opposite to <see cref="GetSerializationProcessors"/>.</returns>
        private IEnumerable<ICustomProcessor> GetDeserializationProcessorsInternal(AbstractFileSystemJob job, ObjectTypeInfo typeInfo)
        {
            CheckArguments(job, typeInfo);

            // Default processors first
            foreach (var processorMethod in ((IEnumerable<ProcessorCreator>)mDefaultProcessors).Reverse())
            {
                yield return processorMethod(job);
            }

            // Processors registered for the original type second
            List<ProcessorCreator> processorList;
            if ((typeInfo.OriginalTypeInfo != null) && mCustomProcessors.TryGetValue(typeInfo.OriginalTypeInfo.ObjectType, out processorList))
            {
                processorList.Reverse();
                foreach (var processorMethod in processorList)
                {
                    yield return processorMethod(job);
                }
            }

            // Processors registered for the type third
            if (mCustomProcessors.TryGetValue(typeInfo.ObjectType, out processorList))
            {
                processorList.Reverse();
                foreach (var processorMethod in processorList)
                {
                    yield return processorMethod(job);
                }
            }
        }


        /// <summary>
        /// Validates given arguments for not-being <see langword="null"/>.
        /// </summary>
        /// <param name="job">Job instance the processor(s) are called from.</param>
        /// <param name="typeInfo">Type of object the processors should be gathered for.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="job"/> or <paramref name="typeInfo"/> is not provided.</exception>
        private static void CheckArguments(AbstractFileSystemJob job, ObjectTypeInfo typeInfo)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job", "Job cannot be null.");
            }
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo", "Type info cannot be null.");
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new job factory with default factory method.
        /// </summary>
        internal CustomProcessorFactory()
        {
        }

        #endregion    
    }
}
