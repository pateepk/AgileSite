using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Container for the extra metadata for the given type
    /// </summary>
    internal class MetadataContainer
    {
        /// <summary>
        /// Type that defines the extra metadata
        /// </summary>
        public Type MetadataType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Empty constructor
        /// </summary>
        public MetadataContainer()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataType">Type that defines the extra metadata</param>
        public MetadataContainer(Type metadataType)
        {
            MetadataType = metadataType;
        }
    }
}
