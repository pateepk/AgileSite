using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Deletes objects of given object type missing in the repository but present in the database.
    /// </summary>
    public class FileSystemDeleteObjectsByTypeJob : AbstractFileSystemTypeWideJob
    {
        #region "Nested classes"

        /// <summary>
        /// Class represents the object that could not be deleted because another object depends on it.
        /// </summary>
        private class NotDeletedObject
        {
            /// <summary>
            /// Object that could not be deleted.
            /// </summary>
            public BaseInfo Object
            {
                get;
                set;
            }


            /// <summary>
            /// Object's file locations in the repository.
            /// </summary>
            public RepositoryLocationsCollection RepositoryLocations
            {
                get;
                set;
            }


            /// <summary>
            /// Error message (contains information why the object could not be deleted).
            /// </summary>
            public string ErrorMessage
            {
                get;
                set;
            }


            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            public override string ToString()
            {
                return String.Format("ErrorMessage: {0}, ObjectType: {1}, MainLocations: {2}",
                    ErrorMessage,
                    Object.TypeInfo.ObjectType,
                    String.Join(", ", RepositoryLocations.MainLocations));
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates a new instance of <see cref="FileSystemDeleteObjectsByTypeJob"/> that deletes all objects of given object type missing in the file system repository from the database.
        /// </summary>
        /// <param name="configuration">Repository configuration.</param>
        public FileSystemDeleteObjectsByTypeJob(FileSystemRepositoryConfiguration configuration)
            : base(configuration)
        {
        }


        /// <summary>
        /// Process all deleted objects of given object type.
        /// </summary>
        /// <param name="objectType">Processed object type.</param>
        /// <param name="repositoryLocations">Set of given object type's repository locations.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <para>
        /// This member is internal for the purpose of testing only.
        /// </para>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        internal void ProcessDeletedObjects(string objectType, ISet<RepositoryLocationsCollection> repositoryLocations, CancellationToken cancellationToken)
        {
            var mainFilesLocations = new HashSet<RepositoryLocationsCollection>(repositoryLocations.Select(x => new RepositoryLocationsCollection(x.MainLocations)));

            string logMessageFormat = GetObjectInfoMessageFormat(objectType);
            var notDeletedObjects = new Queue<NotDeletedObject>();

            CancellableForEach(cancellationToken, GetObjectsInDatabase(objectType), info =>
            {
                DeleteObjectBase(objectType, info, mainFilesLocations, logMessageFormat, notDeletedObjects);
            });

            ProcessNotDeletedObjectsQueue(notDeletedObjects, mainFilesLocations, logMessageFormat, cancellationToken);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Deletes all objects of given object type that are not present in the file system repository from the database. 
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
            ProcessDeletedObjects(objectType, fileLocations, cancellationToken);
        }


        /// <summary>
        /// Gets enumeration of all objects of given type. Only objects that are covered by continuous integration solution are returned.
        /// </summary>
        /// <param name="objectType">Returned objects' type.</param>
        /// <returns>Enumeration of all objects of given type.</returns>
        protected virtual IEnumerable<BaseInfo> GetObjectsInDatabase(string objectType)
        {
            return DatabaseObjectsEnumeratorFactory.GetObjectEnumerator(objectType, RepositoryConfiguration);
        }


        /// <summary>
        /// Deletes object that is missing in the repository from the database.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        protected virtual void DeleteMissingObject(BaseInfo obj)
        {
            ContentStagingTaskCollection.AddTaskForDeleteObject(obj);

            obj.Delete();
        }


        /// <summary>
        /// Gets collection of all file locations that are used for serialization of given object.
        /// </summary>
        /// <param name="obj">File locations will be returned for given object.</param>
        /// <returns>Collection of all file locations that are used for serialization of given object.</returns>
        protected virtual RepositoryLocationsCollection GetObjectFilePaths(BaseInfo obj)
        {
            return new RepositoryLocationsCollection(RepositoryPathHelper.GetFilePath(obj));
        }

        #endregion


        #region "Private methods"

        private void DeleteObjectBase(string objectType, BaseInfo info, ISet<RepositoryLocationsCollection> mainFilesLocations, string logMessageFormat, Queue<NotDeletedObject> notDeletedObjects)
        {
            if (!RepositoryConfigurationEvaluator.IsObjectIncluded(info, RepositoryConfiguration, TranslationHelper))
            {
                // Skip objects not to be included in the repository
                return;
            }

            var locations = GetObjectFilePaths(info);
            if (mainFilesLocations.Contains(locations))
            {
                // If the object is binding, check the content since multiple bindings may share a file
                if (!info.TypeInfo.IsBinding || locations.Any(l => BindingsProcessor.ContainsBinding(info, l)))
                {
                    return;
                }
            }

            try
            {
                DeleteObject(info, locations, mainFilesLocations, logMessageFormat);
            }
            catch (CheckDependenciesException ex)
            {
                notDeletedObjects.Enqueue(new NotDeletedObject
                {
                    Object = info,
                    RepositoryLocations = locations,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception exception)
            {
                var typeInfo = info.TypeInfo;
                var typeName = typeInfo.ObjectType;
                var infoId = info.Generalized.ObjectID;
                throw new ObjectTypeSerializationException(
                    String.Format(
                        "Deletion of object type \"{0}\" ({1}) failed for object \"{2}\" ({3}: {4}). See inner exception for further details.",
                        TypeHelper.GetNiceObjectTypeName(objectType),
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
        /// Process queue of not deleted objects.
        /// </summary>
        /// <param name="queue">Processed queue of not deleted objects.</param>
        /// <param name="repositoryLocations">Set of processed object type's repository locations.</param>
        /// <param name="logMessageFormat">Message format to be logged for each object.</param>
        /// <param name="cancellationToken">Operation can be canceled at any time using given cancellation token. This method's operation terminates as soon as cancellation request is detected.</param>
        /// <exception cref="OperationCanceledException">Thrown when operation was canceled.</exception>
        private void ProcessNotDeletedObjectsQueue(Queue<NotDeletedObject> queue, ISet<RepositoryLocationsCollection> repositoryLocations, string logMessageFormat, CancellationToken cancellationToken)
        {
            CancellablyProcessQueue(cancellationToken, queue, processedObject =>
            {
                try
                {
                    DeleteObject(processedObject.Object, processedObject.RepositoryLocations, repositoryLocations, logMessageFormat);
                }
                catch (CheckDependenciesException ex)
                {
                    processedObject.ErrorMessage = ex.Message;

                    return processedObject;
                }
                catch (Exception exception)
                {
                    var info = processedObject.Object;
                    var typeInfo = info.TypeInfo;
                    var typeName = typeInfo.ObjectType;
                    var infoId = info.Generalized.ObjectID;
                    throw new ObjectTypeSerializationException(
                        String.Format(
                            "Deletion of object type \"{0}\" ({1}) failed for object \"{2}\" ({3}: {4}). See inner exception for further details.",
                            TypeHelper.GetNiceObjectTypeName(typeInfo.ObjectType),
                            typeName,
                            GetLogObjectName(typeInfo, info),
                            typeInfo.IDColumn,
                            infoId),
                        exception,
                        typeName,
                        infoId);
                }

                return null;
            });

            // Irremovable objects left in the queue
            foreach (var notDeletedObject in queue)
            {
                RaiseLogProgress(String.Format(ResHelper.GetAPIString("ci.deserialization.deleteobjectfailedformat", "Object {0} cannot be deleted due to this error: {1}"), GetLogObjectName(notDeletedObject.Object), notDeletedObject.ErrorMessage));
            }
        }


        /// <summary>
        /// Deletes given object from the database.
        /// </summary>
        /// <param name="obj">Object to delete.</param>
        /// <param name="objectLocations">Repository locations of given object.</param>
        /// <param name="repositoryLocations">Set of processed object type's repository locations.</param>
        /// <param name="logMessageFormat">Message format to be logged.</param>
        private void DeleteObject(BaseInfo obj, RepositoryLocationsCollection objectLocations, ISet<RepositoryLocationsCollection> repositoryLocations, string logMessageFormat)
        {
            var deletingMessage = String.Format(logMessageFormat, ResHelper.GetAPIString("ci.deserialization.objectdeleting", "deleting"), GetLogObjectName(obj));

            RaiseLogProgress(deletingMessage, LogItemTypeEnum.Info, LogItemActionTypeEnum.Delete);

            try
            {
                using (new CMSActionContext { CreateVersion = false })
                {
                    DeleteMissingObject(obj);
                }
            }
            catch (CheckDependenciesException)
            {
                var errorMessage = String.Format(logMessageFormat, GetLogObjectName(obj), ResHelper.GetAPIString("ci.deserialization.objectcannotbedeleted", "cannot be deleted due to its dependencies"));
                RaiseLogProgress(errorMessage);

                throw;
            }

            if (UseFileMetadata)
            {
                foreach (var objectLocation in objectLocations)
                {
                    if (repositoryLocations.SelectMany(locations => locations).Any(location => location == objectLocation))
                    {
                        // Meta-data of locations shared with another object won't be deleted
                        continue;
                    }

                    var metaDataCollection = FileMetadataInfoProvider.GetFileMetadataInfos()
                                                            .WhereEquals("FileLocation", objectLocation)
                                                            .Or()
                                                            .WhereStartsWith("FileLocation", RepositoryPathHelper.GetPathWithoutExtension(objectLocation) + RepositoryPathHelper.GROUP_SUFFIX_DELIMITER);

                    foreach (var metaData in metaDataCollection)
                    {
                        FileSystemWriter.RepositoryHashManager.RemoveHash(metaData.FileLocation);
                    }
                }
            }

            var deletedMessage = String.Format(logMessageFormat, GetLogObjectName(obj), ResHelper.GetString("ci.deserialization.objectdeleted"));

            RaiseLogProgress(deletedMessage, LogItemTypeEnum.Info, LogItemActionTypeEnum.Delete);
        }

        #endregion
    }
}
