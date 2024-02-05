using System;
using System.Linq;
using System.Text;

using CMS.DataEngine.Serialization;
using CMS.DataEngine;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Provides information on custom process result.
    /// </summary>
    public class CustomProcessorResult : DeserializationResultBase
    {
        /// <summary>
        /// Type info the processor processes.
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of <see cref="CustomProcessorResult"/>
        /// </summary>
        /// <param name="typeInfo">Type info the process processes.</param>
        /// <exception cref="NullReferenceException">Thrown when <paramref name="typeInfo"/> is <see langword="null"/>.</exception>
        public CustomProcessorResult(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }

            TypeInfo = typeInfo;
        }
    }
}
