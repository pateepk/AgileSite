using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Linq;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// List of prefixes as a source for document path properties update.
    /// </summary>
    /// <remarks>This class is used to serialize list of prefixes to XML to be used for paths update.</remarks>
    [CollectionDataContract(Name = "Prefixes", ItemName = "Prefix", Namespace = "")]
    internal class DocumentPathPrefixes : List<DocumentPathPrefixes.Prefix>
    {
        /// <summary>
        /// Represents pair of DocumentNamePath and DocumentUrlPath for a document culture version.
        /// </summary>
        /// <remarks>This class is used for serialization of the prefixes information as an input for paths update.</remarks>
        [DataContract(Namespace = "")]
        internal class Prefix
        {
            [DataMember]
            public string Culture { get; internal set; }


            [DataMember]
            public Path NamePath { get; internal set; }


            [DataMember]
            public Path UrlPath { get; internal set; }


            /// <summary>
            /// Indicates if the original and current prefixes are different
            /// </summary>
            internal protected bool Changed()
            {
                return (NamePath.Original != NamePath.Current) || (UrlPath.Original != UrlPath.Current);
            }
        }


        /// <summary>
        /// Represents pair of original path and current path of a document.
        /// </summary>
        /// <remarks>This class is used for serialization of the prefixes information as an input for paths update.</remarks>
        [DataContract(Namespace = "")]
        internal class Path
        {
            /// <summary>
            /// Creates new instance of <see cref="Path"/>.
            /// </summary>
            /// <remarks>Ensures slashes for provided paths.</remarks>
            /// <param name="original">Original path</param>
            /// <param name="current">Current path</param>
            public Path(string original, string current)
            {
                Original = EnsureSlash(original);
                Current = EnsureSlash(current);
            }

            
            [DataMember]
            public string Original { get; private set; }


            [DataMember]
            public string Current { get; private set; }


            private string EnsureSlash(string path)
            {
                if (path == null)
                {
                    return null;
                }

                return path.TrimEnd('/') + "/";
            }
        }


        /// <summary>
        /// Gets XML representation of the object
        /// </summary>
        public string Serialize()
        {
            var data = new StringBuilder();
            using (var writer = XmlWriter.Create(data, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                var serializer = new DataContractSerializer(GetType());
                serializer.WriteObject(writer, this);
            }

            return data.ToString();
        }


        /// <summary>
        /// Indicates if the original and current prefixes are different for at least one prefix
        /// </summary>
        public bool Changed()
        {
            return Exists(p => p.Changed());
        }
    }
}
