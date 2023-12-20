using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Serialization;
using CMS.DataEngine.CollectionExtensions;
using CMS.Helpers;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Inserts all objects of given object type present in repository to the database. If object already exists, it is updated. 
    /// </summary>
    public class FileSystemUpsertObjectsByTypeJob : AbstractFileSystemTypeWideJob
    {
        #region "Nested classes"

        /// <summary>
        /// Class represents result of object deserialization.
        /// </summary>
        private class DeserializedObject
        {
            /// <summary>
            /// Deserialization result.
            /// </summary>
            public DeserializationResult Result
            {
                get;
                set;
            }


            /// <summary>
            /// Locations of files used for object deserialization.
            /// </summary>
            public RepositoryLocationsCollection RepositoryLocations
            {
                get;
                set;
            }


            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            public override string ToString()
            {
                return String.Format("IsValid: {0}, ObjectType: {1}, MainLocations: {2}",
                    Result.IsValid,
                    Result.DeserializedInfo.TypeInfo.ObjectType,
                    String.Join(", ", RepositoryLocations.MainLocations));
            }
        }

        #endregion


        #region "Constants"

        // Separates two field names.
        private const string FIELD_SEPARATOR = ", ";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates a new instance of <see cref="FileSystemUpsertObjectsByTypeJob"/> that restores all objects of given object type to the database.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        public FileSystemUpsertObjectsByTypeJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Inserts all objects of given object type that are present in the file system repository to the database. If object already exists, it is updated.
        /// </summary>
        /// <param name="objectType">Objects of given object type will be restored.</param>
        /// <param name="fileLocations">Set of all locations' of objects stored in the repository that are of given <paramref name="objectType"/>.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <remarks>
        /// <para>
        /// This operation can be canceled using given <paramref name="cancellationToken"/> at any time.
        /// This method's operations throws <see cref="OperationCanceledException"/> as soon as cancellation request is detected.
        /// </para>
        /// </remarks>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        protected override void RunInternal(string objectType, ISet<RepositoryLocationsCollection> fileLocations, CancellationToken cancellationToken)
        {
            var changedObjects = fileLocations.SelectMany(ChangedObjectSelector).Where(it => it != null);
            ProcessChangedObjects(objectType, changedObjects, cancellationToken);
        }


        /// <summary>
        /// Returns deserialized object(s) in its final form that might be composed of multiple partial sub-objects.
        /// All object part's file locations are provided in <paramref name="fileLocations"/>.
        /// </summary>
        /// <remarks>
        /// This method can handle single location. For multiple location must be registered custom job.
        /// <para>For binding object types, single file location is provided, yet all binding objects with same parent are returned.</para>
        /// </remarks>
        /// <param name="fileLocations">Serialized object locations in repository.</param>
        /// <returns>Collection of deserialized main objects.</returns>
        protected virtual IEnumerable<DeserializationResult> GetDeserializedMainObjects(RepositoryLocationsCollection fileLocations)
        {
            IDictionary<string, IEnumerable<DeserializationResult>> deserializedObjects = new Dictionary<string, IEnumerable<DeserializationResult>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var structuredLocation in fileLocations.StructuredLocations)
            {
                deserializedObjects.Add(structuredLocation.MainLocation, DeserializeObjectFromFiles(structuredLocation));
            }

            return GetMainObjectsWrapped(deserializedObjects);
        }


        /// <summary>
        /// Gets main objects directly or composes multiple partial sub-objects into one or more main objects
        /// while encapsulating uncaught exceptions to allow for easier repository state debugging.
        /// </summary>
        /// <param name="deserializedObjects">Deserialized objects indexed by their location.</param>
        /// <returns>Main object or objects.</returns>
        private IEnumerable<DeserializationResult> GetMainObjectsWrapped(IDictionary<string, IEnumerable<DeserializationResult>> deserializedObjects)
        {
            try
            {
                return GetMainObjects(deserializedObjects);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    String.Format("Reconstructing object from deserialized part(s) failed:{0}{1}{0}{0}This is typically caused by deserialized part not containing expected information or by missing part(s) in the repository.",
                    Environment.NewLine, String.Join(Environment.NewLine, deserializedObjects.Keys)), ex);
            }
        }


        /// <summary>
        /// Gets main objects directly or composes multiple partial sub-objects into one or more main objects.
        /// </summary>
        /// <param name="deserializedObjects">Deserialized objects indexed by their location.</param>
        /// <returns>Main object or objects.</returns>
        protected virtual IEnumerable<DeserializationResult> GetMainObjects(IDictionary<string, IEnumerable<DeserializationResult>> deserializedObjects)
        {
            return deserializedObjects.First().Value;
        }


        /// <summary>
        /// Sets changed object to the database.
        /// </summary>
        /// <param name="baseInfo">Changed object</param>
        protected virtual void SetChangedObject(BaseInfo baseInfo)
        {
            ContentStagingTaskCollection.AddTaskForChangeObject(baseInfo);

            baseInfo.Generalized.SetObject();

            ReplaceTranslationRecord(baseInfo);
        }


        /// <summary>
        /// Deserializes object(s) from files' content based on <paramref name="structuredLocation"/>.
        /// <para>Files' content is eventually composed from separated fields stored in separated files.</para>
        /// <para>If file's content belongs to a bindings object type, all binding objects within the file (i.e. objects with same parent) are returned.</para>
        /// </summary>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        /// <returns>Collection of deserialized objects.</returns>
        protected IEnumerable<DeserializationResult> DeserializeObjectFromFiles(StructuredLocation structuredLocation)
        {
            var document = new XmlDocument();
            XmlElement root;
            try
            {
                var fileContent = FileSystemReader.ReadString(structuredLocation.MainLocation);
                document.LoadXml(fileContent);
                root = document.DocumentElement;
            }
            catch (Exception ex)
            {
                throw new ObjectSerializationException("The file \"" + structuredLocation.MainLocation + "\" is corrupted. See inner exception for further details.", ex);
            }

            if (root == null)
            {
                // No root, no inner elements, no results
                yield break;
            }

            var typeInfo = ObjectTypeManager.GetTypeInfo(root.Name, true);
            if (typeInfo.IsBinding)
            {
                // Read bindings from XML altogether
                var deserializedBindings = BindingsProcessor.ReadBindings(document, typeInfo);
                foreach (var binding in deserializedBindings)
                {
                    yield return binding;
                }
            }
            else
            {
                DeserializationResult deserializationResult;
                try
                {
                    // Pre-processes XML document, first of all with SeparatedFieldProcessor
                    var customProcessorsResult = ExecuteCustomProcessors(typeInfo, document, structuredLocation);

                    // Read single object from XML
                    deserializationResult = root.Deserialize(TranslationHelper);

                    // Merges custom processors' results into deserialization results
                    deserializationResult.MergeWith(customProcessorsResult);

                    // Handles binary fields
                    SeparatedFieldProcessor.PostprocessDeserializedDocument(deserializationResult, structuredLocation);
                }
                catch (Exception ex)
                {
                    throw new ObjectSerializationException("Deserialization of object stored in file \"" + structuredLocation.MainLocation + "\" failed. See inner exception for further details.", ex);
                }
                yield return deserializationResult;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Process all changed objects.
        /// </summary>
        /// <param name="objectType">Processed object type.</param>
        /// <param name="objects">Enumeration of changed objects to be processed.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <returns>FileMetadataInfo enumeration of objects inserted to the database.</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        private void ProcessChangedObjects(string objectType, IEnumerable<DeserializedObject> objects, CancellationToken cancellationToken)
        {
            string logMessageFormat = GetObjectInfoMessageFormat(objectType);
            var invalidObjectsQueue = new Queue<DeserializedObject>();

            CancellableForEach(cancellationToken, objects, deserializedObject =>
            {
                if (deserializedObject.Result.IsValid)
                {
                    // Insert object to the database immediately
                    ProcessChangedObject(deserializedObject, logMessageFormat);
                }
                else
                {
                    invalidObjectsQueue.Enqueue(deserializedObject);
                }
            });

            ProcessInvalidObjectsQueue(invalidObjectsQueue, logMessageFormat, cancellationToken);
        }


        /// <summary>
        /// Process changed object and returns its FileMetadata info.
        /// </summary>
        /// <param name="deserializedObject">Deserialized object to be processed.</param>
        /// <param name="logMessageFormat">Message format to be logged after inserting object to the database.</param>
        /// <returns>FileMetadataInfo of processed object.</returns>
        /// <exception cref="ObjectTypeSerializationException">Thrown when operation fails.</exception>
        private void ProcessChangedObject(DeserializedObject deserializedObject, string logMessageFormat)
        {
            var baseInfo = deserializedObject.Result.DeserializedInfo;

            if (!RepositoryConfigurationEvaluator.IsObjectIncluded(baseInfo, RepositoryConfiguration, TranslationHelper))
            {
                return;
            }

            var updatingMessage = String.Format(logMessageFormat, ResHelper.GetAPIString("ci.deserialization.objectupdating", "updating"), GetLogObjectName(baseInfo));

            RaiseLogProgress(updatingMessage, LogItemTypeEnum.Info, LogItemActionTypeEnum.Update);

            try
            {
                using (new CMSActionContext { CreateVersion = false, UpdateSystemFields = false })
                {
                    SetChangedObject(baseInfo);
                }

                var updatedMessage = String.Format(logMessageFormat, GetLogObjectName(baseInfo), ResHelper.GetAPIString("ci.deserialization.objectupdated", "updated"));

                RaiseLogProgress(updatedMessage, LogItemTypeEnum.Info, LogItemActionTypeEnum.Update);

                foreach (var location in deserializedObject.RepositoryLocations)
                {
                    FileSystemWriter.RepositoryHashManager.SaveHash(FileSystemReader.GetFileHash(location), location);
                }
            }
            catch (Exception exception)
            {
                var info = deserializedObject.Result.DeserializedInfo;
                var typeInfo = info.TypeInfo;
                var typeName = typeInfo.ObjectType;
                var infoId = info.Generalized.ObjectID;
                throw new ObjectTypeSerializationException(
                    String.Format(
                        "Restoration of object type \"{0}\" ({1}) failed for object \"{2}\" ({3}: {4}). See inner exception for further details.",
                        TypeHelper.GetNiceObjectTypeName(typeInfo.ObjectType),
                        typeName,
                        GetLogObjectName(typeInfo, info),
                        typeInfo.IDColumn,
                        infoId),
                    exception,
                    typeName,
                    infoId);
            }
        }


        /// <summary>
        /// Process invalid deserialized objects queue. All objects that can be restored are inserted in to the database.
        /// </summary>
        /// <param name="queue">Processed queue of invalid objects.</param>
        /// <param name="logMessageFormat">Message format to be logged for each object.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        private void ProcessInvalidObjectsQueue(Queue<DeserializedObject> queue, string logMessageFormat, CancellationToken cancellationToken)
        {
            CancellablyProcessQueue(cancellationToken, queue, processedObject =>
            {
                // Try to translate failed references again
                processedObject.Result = TranslationHelper.TranslateFailedReferences(processedObject.Result);
                if (!processedObject.Result.IsValid)
                {
                    return processedObject;
                }
                ProcessChangedObject(processedObject, logMessageFormat);

                return null;
            });

            ProcessObjectsWithNotRequiredMappings(queue, logMessageFormat, cancellationToken);

            // Unrestorable objects left in the queue
            LogInvalidObjects(queue, logMessageFormat);
        }


        /// <summary>
        /// Process invalid deserialized objects queue. All objects with failed mappings that are not required can be restored and are inserted in to the database and removed from the queue.
        /// </summary>
        /// <param name="queue">Processed queue of invalid objects.</param>
        /// <param name="logMessageFormat">Message format to be logged for each object.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        private void ProcessObjectsWithNotRequiredMappings(Queue<DeserializedObject> queue, string logMessageFormat, CancellationToken cancellationToken)
        {
            var queueCount = queue.Count;
            for (var i = 0; i < queueCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var processedObject = queue.Dequeue();
                var deserializationResult = processedObject.Result;
                var typeDependsOn = deserializationResult.DeserializedInfo.TypeInfo.DependsOn;

                if (!deserializationResult.FailedFields.Any() && (typeDependsOn != null))
                {
                    var notRequiredDependencies = typeDependsOn.Where(x => x.DependencyType == ObjectDependencyEnum.NotRequired)
                        .Select(x => x.DependencyColumn).ToHashSetCollection(StringComparer.OrdinalIgnoreCase);
                    if (deserializationResult.FailedMappings.All(x => notRequiredDependencies.Contains(x.FieldName)))
                    {
                        try
                        {
                            ProcessChangedObject(processedObject, logMessageFormat);
                            RaiseLogProgress(GetIncompleteObjectRestoredMessage(processedObject, RepositoryConfiguration), LogItemTypeEnum.Warning, LogItemActionTypeEnum.Update);

                            // Object was successfully updated, do not enqueue it back to the queue
                            continue;
                        }
                        catch (ObjectTypeSerializationException ex)
                        {
                            RaiseLogProgress(ex.Message, LogItemTypeEnum.Error, LogItemActionTypeEnum.Update);
                        }
                    }
                }
                
                queue.Enqueue(processedObject);
            }
        }


        /// <summary>
        /// Gets deserialized object if its file was changed.
        /// </summary>
        /// <param name="fileLocations">Serialized object locations in repository.</param>
        /// <returns>Deserialized object if its file was changed in repository, null otherwise.</returns>
        /// <remarks>
        /// Provided <paramref name="fileLocations"/> contains all location single (serialized info) object is stored in
        /// (typically single location is present in the collection).
        /// </remarks>
        private IEnumerable<DeserializedObject> ChangedObjectSelector(RepositoryLocationsCollection fileLocations)
        {
            bool fileChanged = fileLocations.Any(fileLocation => FileSystemWriter.RepositoryHashManager.HasHashChanged(FileSystemReader.GetFileHash(fileLocation), fileLocation));

            if (!fileChanged)
            {
                // The file has not been changed (hash in database is the same as computed hash), throw away cached files' content
                FileSystemReader.RemoveFromCache(fileLocations);
                yield break;
            }

            foreach (var result in GetDeserializedMainObjects(fileLocations))
            {
                yield return new DeserializedObject
                {
                    Result = result,
                    RepositoryLocations = fileLocations,
                };
            }
        }


        /// <summary>
        /// Returns text message containing all failed fields.
        /// </summary>
        /// <param name="invalidObject">Object that has failed field(s).</param>
        private static string GetInvalidObjectFieldMessage(DeserializedObject invalidObject)
        {
            return String.Format(
                ResHelper.GetAPIString("ci.deserialization.invalidobjectfields", "Deserialization of the following fields failed: {0}."),
                String.Join(FIELD_SEPARATOR, invalidObject.Result.FailedFields));
        }


        /// <summary>
        /// Returns text message containing all failed mappings.
        /// </summary>
        /// <param name="invalidObject">Object that has failed mapping(s).</param>
        /// <param name="repositoryConfiguration">Configuration of a file system repository</param>
        private static string GetInvalidObjectMappingMessage(DeserializedObject invalidObject, FileSystemRepositoryConfiguration repositoryConfiguration)
        {
            var failedMappings = invalidObject.Result.FailedMappings;
            var failedFieldNames = failedMappings.Select(mapping => mapping.FieldName);
            var failedObjects = failedMappings.Select(t => new
            {
                t.TranslationReference.ObjectType,
                t.TranslationReference.CodeName
            }).ToHashSetCollection();
            
            var message = String.Format(
                ResHelper.GetAPIString("ci.deserialization.invalidobjectmapping", "Mapping of the following fields failed: {0}. The following required objects do not exist: {1}. The objects are missing in the target database and/or excluded from continuous integration in repository.config."),
                String.Join(FIELD_SEPARATOR, failedFieldNames),
                String.Join(FIELD_SEPARATOR, failedObjects.Select(obj => String.Format("{0}({1})", obj.CodeName, obj.ObjectType))));
            
            // Get failed object types which are not included in repository config
            var excludedObjectTypes = failedObjects.Select(obj => obj.ObjectType).Except(repositoryConfiguration.ObjectTypes).ToHashSetCollection(StringComparer.Ordinal);
            if (excludedObjectTypes.Count > 0)
            {
                var excludedMessage = String.Format(ResHelper.GetAPIString("ci.deserialization.notincludedobjecttypes", "The following related object types are currently not included: {0}."), 
                    String.Join(FIELD_SEPARATOR, excludedObjectTypes));

                message += " " + excludedMessage;
            }
            
            return message;
        }


        /// <summary>
        /// Returns single message for all failed fields and all failed mappings.
        /// </summary>
        /// <param name="invalidObject">Object that has failed field(s) and/or failed mapping(s).</param>
        /// <param name="repositoryConfiguration">Configuration of a file system repository</param>
        private static string GetInvalidObjectMessage(DeserializedObject invalidObject, FileSystemRepositoryConfiguration repositoryConfiguration)
        {
            var message = new StringBuilder().AppendFormat(ResHelper.GetAPIString("ci.deserialization.objectcannotbedeserialized", "The object stored in file {0} cannot be deserialized."),
                String.Join(FIELD_SEPARATOR, invalidObject.RepositoryLocations));

            if (invalidObject.Result.FailedFields.Any())
            {
                message.Append(" ").Append(GetInvalidObjectFieldMessage(invalidObject));
            }

            if (invalidObject.Result.FailedMappings.Any())
            {
                message.Append(" ").Append(GetInvalidObjectMappingMessage(invalidObject, repositoryConfiguration));
            }

            return message.ToString();
        }


        /// <summary>
        /// Returns message informing about incomplete object restore and its failed mappings.
        /// </summary>
        /// <param name="incompleteObject">Object that has failed mapping(s).</param>
        /// <param name="repositoryConfiguration">Configuration of a file system repository</param>
        private static string GetIncompleteObjectRestoredMessage(DeserializedObject incompleteObject, FileSystemRepositoryConfiguration repositoryConfiguration)
        {
            var message = String.Format(ResHelper.GetAPIString("ci.deserialization.incompleteobjectrestored", "The object stored in file {0} was not restored completely due to missing optional dependencies."),
                String.Join(FIELD_SEPARATOR, incompleteObject.RepositoryLocations)) 
                + " "
                + GetInvalidObjectMappingMessage(incompleteObject, repositoryConfiguration);
            
            return message;
        }


        /// <summary>
        /// Logs failed objects to progress log.
        /// </summary>
        /// <param name="invalidObjects">Enumeration of failed objects.</param>
        /// <param name="logMessageFormat">Message format.</param>
        private void LogInvalidObjects(IEnumerable<DeserializedObject> invalidObjects, string logMessageFormat)
        {
            invalidObjects
                .Select(obj => GetInvalidObjectMessage(obj, RepositoryConfiguration))
                .ToList()
                .ForEach(failedMessage => RaiseLogProgress(String.Format(logMessageFormat, failedMessage, String.Empty), LogItemTypeEnum.Error));
        }


        /// <summary>
        /// Instantiates and executes all <see cref="ICustomProcessor"/>s registered for given <see cref="BaseInfo.TypeInfo"/>
        /// or its <see cref="ObjectTypeInfo.OriginalObjectType"/> (if any), along with <see cref="SeparatedFieldProcessor"/>.
        /// </summary>
        /// <param name="typeInfo">Type info of object that is stored in <paramref name="serializedInfo"/>.</param>
        /// <param name="serializedInfo">XML file loaded from the file system.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        /// <returns>Collections that all processors could written in if they were unable to process the <paramref name="serializedInfo"/>.</returns>
        /// <remarks>Execution's order is given and reverse to the order in <see cref="FileSystemStoreJob"/>.</remarks>
        private CustomProcessorResult ExecuteCustomProcessors(ObjectTypeInfo typeInfo, XmlDocument serializedInfo, StructuredLocation structuredLocation)
        {
            var deserializationResult = new CustomProcessorResult(typeInfo);

            var customProcessors = CustomProcessorFactory.GetDeserilizationProcessors(this, typeInfo);
            foreach (var customProcessor in customProcessors)
            {
                customProcessor.PreprocessDeserializedDocument(deserializationResult, serializedInfo, structuredLocation);
            }

            return deserializationResult;
        }


        /// <summary>
        /// Registers record for given <paramref name="info"/> into <see cref="TranslationHelper"/>.
        /// </summary>
        /// <remarks>
        /// Deletes old record of same object when found.
        /// Required only by objects with tree hierarchy, yet without any order (e.g documents).
        /// </remarks>
        protected void ReplaceTranslationRecord(BaseInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var typeInfo = info.TypeInfo;
            var objectId = info.Generalized.ObjectID;

            // Delete old record
            var record = TranslationHelper.GetRecord(typeInfo.ObjectType, objectId);
            if (record != null)
            {
                TranslationHelper.DeleteRecord(record);
            }

            TranslationHelper.RegisterRecord(info);
        }

        #endregion
    }
}