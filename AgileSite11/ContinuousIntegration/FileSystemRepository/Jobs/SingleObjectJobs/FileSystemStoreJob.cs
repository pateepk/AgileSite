using System;
using System.Xml;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class designated for serialization of BaseInfo objects to the file system.
    /// </summary>
    public class FileSystemStoreJob : AbstractSingleObjectJob
    {
        /// <summary>
        /// Creates a new file system store job with given repository configuration.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public FileSystemStoreJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Returns text that is shown in <see cref="ObjectSerializationException"/> thrown when execution of <see cref="RunInternal(BaseInfo)"/> fails with an exception.
        /// </summary>
        /// <param name="baseInfo">Object that was passed to the <see cref="RunInternal(BaseInfo)"/> method.</param>
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected override string ObjectSerializationExceptionMessage(BaseInfo baseInfo)
        {
            return "Serialization of the object " + baseInfo + " has failed.";
        }


        /// <summary>
        /// Stores given <paramref name="baseInfo"/> to the repository by serializing it to proper repository location.
        /// </summary>
        /// <param name="baseInfo">Base info which will be stored.</param>
        /// <remarks>Provided <paramref name="baseInfo"/> is never <see langword="null"/>.</remarks>
        protected override void RunInternal(BaseInfo baseInfo)
        {
            // For current operation there is no benefit in storing current object in translation helper.
            // However translation helper might be shared on global level and subsequent operations might benefit from it.
            RegisterTranslationRecord(baseInfo);

            var relativePath = RepositoryPathHelper.GetFilePath(baseInfo);
            StoreBaseInfo(baseInfo, relativePath);
        }


        /// <summary>
        /// Stores given <paramref name="baseInfo"/> to a repository.
        /// </summary>
        /// <param name="baseInfo">Base info which will be stored.</param>
        /// <param name="relativePath">Relative path where the base info will be stored</param>
        protected void StoreBaseInfo(BaseInfo baseInfo, string relativePath)
        {
            EnsureCompleteInfo(baseInfo);

            if (baseInfo.TypeInfo.IsBinding)
            {
                var document = BindingsProcessor.AppendBinding(baseInfo, relativePath);

                if (document == FileSystemBindingsProcessor.NO_BINDINGS)
                {
                    throw new InvalidOperationException($"File \"{RepositoryConfiguration.GetAbsolutePath(relativePath)}\" contains invalid data and cannot be updated!");
                }

                BindingsProcessor.WriteBindings(relativePath, document);
            }
            else
            {
                var document = GetPartialDocumentStoringSeparatedFieldsSideways(baseInfo, relativePath);
                FileSystemWriter.WriteToFile(relativePath, document);
            }
        }


        /// <summary>
        /// Stores content of separated fields to extra files and removes them from serialized object's XML,
        /// also executes all <see cref="ICustomProcessor"/>s registered for object type of given <paramref name="baseInfo"/>.
        /// </summary>
        /// <param name="baseInfo">Base info which will be stored.</param>
        /// <param name="relativePath">Relative path to file in which the given base info is to be stored.</param>
        /// <returns>Document the content of serialized object's XML is stored in.</returns>
        private XmlDocument GetPartialDocumentStoringSeparatedFieldsSideways(BaseInfo baseInfo, string relativePath)
        {
            var serializedObject = baseInfo.Serialize(TranslationHelper);

            // Post-processes XML document, last of all with SeparatedFieldProcessor
            ExecuteCustomProcessors(baseInfo, relativePath, serializedObject);

            return serializedObject.OwnerDocument;
        }


        /// <summary>
        /// Instantiates and executes all <see cref="ICustomProcessor"/>s registered for given <see cref="BaseInfo.TypeInfo"/>
        /// or its <see cref="ObjectTypeInfo.OriginalObjectType"/> (if any), along with <see cref="SeparatedFieldProcessor"/>.
        /// </summary>
        /// <param name="baseInfo">Object that was serialized into <paramref name="serializedObject"/>.</param>
        /// <param name="relativePath">(Main) Path the <paramref name="serializedObject"/> will be stored in.</param>
        /// <param name="serializedObject">Serialization form of the <paramref name="baseInfo"/>.</param>
        /// <remarks>Execution's order is given and reverse to the order in <see cref="FileSystemUpsertObjectsByTypeJob"/>.</remarks>
        private void ExecuteCustomProcessors(BaseInfo baseInfo, string relativePath, XmlElement serializedObject)
        {
            var customProcessors = CustomProcessorFactory.GetSerializationProcessors(this, baseInfo.TypeInfo);
            foreach (var customProcessor in customProcessors)
            {
                customProcessor.PostprocessSerializedObject(baseInfo, relativePath, serializedObject);
            }
        }


        /// <summary>
        /// Ensures that given info object has all data loaded.
        /// </summary>
        private static void EnsureCompleteInfo(IInfo baseInfo)
        {
            // New object should be complete
            if (baseInfo.Generalized.ObjectID == 0)
            {
                return;
            }

            // Existing object can be completed
            if (!baseInfo.IsComplete)
            {
                baseInfo.MakeComplete(true);
            }
        }
    }
}
