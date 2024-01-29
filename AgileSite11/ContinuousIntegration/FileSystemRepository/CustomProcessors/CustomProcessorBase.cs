using System;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Provides <see cref="TranslationHelper"/> object from a job the processor will be run from to ease
    /// customization of <see cref="XmlDocument"/> after it is processed by <see cref="InfoSerializer"/>
    /// and before it is process by <see cref="InfoDeserializer"/>.
    /// </summary>
    /// <remarks>This class is intended for internal usage only.</remarks>
    public abstract class CustomProcessorBase : ICustomProcessor
    {
        /// <summary>
        /// Translation helper object used to optimize database calls.
        /// </summary>
        protected TranslationHelper TranslationHelper
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a loader object used to instantiate <see cref="TranslationReference"/> from various sources.
        /// </summary>
        protected TranslationReferenceLoader TranslationReferenceLoader
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of the <see cref="CustomProcessorBase"/> class.
        /// </summary>
        /// <param name="job">Job the processor will be run from.</param>
        protected CustomProcessorBase(AbstractFileSystemJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            TranslationHelper = job.TranslationHelper;
            TranslationReferenceLoader = TranslationHelper.TranslationReferenceLoader;
        }


        /// <summary>
        /// Amends <paramref name="document"/> loaded from file system before it is processed by <see cref="InfoDeserializer"/>.
        /// </summary>
        /// <param name="processorResult">Object carrying information of ongoing deserialization.</param>
        /// <param name="document">XML file loaded from the file system.</param>
        /// <param name="structuredLocation">Location of object's main file and its additional parts in repository.</param>
        /// <remarks>
        /// In case the <paramref name="document"/> is in invalid state, <see cref="DeserializationResultBase.FailedFields"/>
        /// and/or <see cref="DeserializationResultBase.FailedMappings"/> should be updated in order to stop object from being
        /// set into DB in invalid state. 
        /// <para>The method should never throw an exception.</para>
        /// </remarks>
        public abstract void PreprocessDeserializedDocument(CustomProcessorResult processorResult, XmlDocument document, StructuredLocation structuredLocation);


        /// <summary>
        /// Amends <paramref name="serializedObject"/> after it was processed by <see cref="InfoSerializer"/>.
        /// </summary>
        /// <param name="infoObject">Object that was serialized into <paramref name="serializedObject"/>.</param>
        /// <param name="infoRelativePath">(Main) Path the <paramref name="serializedObject"/> will be stored in.</param>
        /// <param name="serializedObject">Serialization form of the <paramref name="infoObject"/>.</param>
        /// <remarks>The method should never throw an exception.</remarks>
        public abstract void PostprocessSerializedObject(BaseInfo infoObject, string infoRelativePath, XmlElement serializedObject);
    }
}
