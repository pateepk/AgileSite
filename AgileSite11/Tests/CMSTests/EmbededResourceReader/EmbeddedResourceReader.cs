using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources.Tools;

namespace CMS.Tests
{
    /// <summary>
    /// Reads content of an embedded resource which name is provided in the way of relative path within provided assembly (project).
    /// </summary>
    /// <remarks>
    /// Class is meant to be injected into test class.
    /// <para>For single class, it should be instantiated only once per assembly (as the embedded resources do not change during runtime).</para>
    /// </remarks>
    public class EmbeddedResourceReader
    {
        // Separates resource's naming spaces. In this case, effectively replaces "\" with ".".
        private const string RESOURCE_NAMESPACE_SEPARATOR = ".";

        // Separates file's name from it's extension.
        private const string PATH_EXTENSION_SEPARATOR = ".";


        private IEnumerable<string> mManifestEmbeddedResourceFileNames;
        private CodeDomProvider mCodeDomProvider;


        /// <summary>
        /// Assembly the resources are located in (not the assembly the <see cref="EmbeddedResourceReader{TAnySourceAssemblyType}"/> is in).
        /// </summary>
        /// <remarks>Value is read only once per instance.</remarks>
        protected Assembly EmbeddedResourceAssembly
        {
            get;
            private set;
        }


        /// <summary>
        /// Collections of all embedded resource file names in the <see cref="EmbeddedResourceAssembly"/>.
        /// </summary>
        /// <remarks>Value is read only once per instance.</remarks>
        protected IEnumerable<string> ManifestEmbeddedResourceFileNames
        {
            get
            {
                return mManifestEmbeddedResourceFileNames ?? (mManifestEmbeddedResourceFileNames = EmbeddedResourceAssembly.GetManifestResourceNames());
            }
        }


        /// <summary>
        /// Code domain provider used for obtaining correct name of the embedded resource from give file path (see <see cref="ReadResourceFile(string)"/>).
        /// </summary>
        protected CodeDomProvider CodeDomProvider
        {
            get
            {
                return mCodeDomProvider ?? (mCodeDomProvider = new Microsoft.CSharp.CSharpCodeProvider());
            }
        }


        /// <summary>
        /// Create new instance of the embedded resource reader for given assembly.
        /// </summary>
        /// <param name="embeddedResourceAssembly">Assembly the embedded resources should be read from.</param>
        /// <exception cref="ArgumentNullException">Thrown when no <paramref name="embeddedResourceAssembly"/> provided.</exception>
        public EmbeddedResourceReader(Assembly embeddedResourceAssembly)
        {
            if (embeddedResourceAssembly == null)
            {
                throw new ArgumentNullException("embeddedResourceAssembly");
            }

            EmbeddedResourceAssembly = embeddedResourceAssembly;
        }


        /// <summary>
        /// Create new instance of the embedded resource reader for calling assembly.
        /// </summary>
        public EmbeddedResourceReader()
        {
            EmbeddedResourceAssembly = Assembly.GetCallingAssembly();
        }


        /// <summary>
        /// Returns content of embedded resource specified by <paramref name="fileName"/> that is stored in the <see cref="EmbeddedResourceAssembly"/> under <paramref name="rootFolder"/>.
        /// </summary>
        /// <param name="rootFolder">Relative path to the folder where various embedded resources are stored in.</param>
        /// <param name="fileName">Name of a single embedded resource stored within <paramref name="rootFolder"/>.</param>
        /// <remarks>
        /// <paramref name="rootFolder"/> is supposed to be relative against (executing) project root.
        /// <para><paramref name="fileName"/> has to contain extension (but must not contain <paramref name="rootFolder"/>).</para>
        /// <para>
        /// If there are more resources which name ends with combination of provided <paramref name="rootFolder"/> and <paramref name="fileName"/>,
        /// very first found is returned (see <see  cref="Assembly.GetManifestResourceNames()"/> for further insights).
        /// </para>
        /// <para>
        /// In order to obtain correct (partial) embedded resource name, symbols "\" and other invalid characters are replaced with "." and "_" respectively
        /// in the combination of provided <paramref name="rootFolder"/> and <paramref name="fileName"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="rootFolder"/> or  <paramref name="fileName"/> is null or empty.</exception>
        /// <exception cref="EmbeddedResourceNotFoundException">Thrown when either embedded resource full name or resource stream was not found.</exception>
        public string ReadResourceFile(string rootFolder, string fileName)
        {
            if (String.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentException("Provided root folder cannot be null nor empty.", "rootFolder");
            }

            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Provided file name cannot be null nor empty.", "fileName");
            }

            var relativeFilePath = Path.Combine(rootFolder, fileName);
            return ReadResourceFile(relativeFilePath);
        }


        /// <summary>
        /// Returns content of embedded resource specified by <paramref name="relativeFilePath"/> that is stored in the <see cref="EmbeddedResourceAssembly"/>.
        /// </summary>
        /// <param name="relativeFilePath">Relative path to a single embedded resource (including folders).</param>
        /// <remarks>
        /// <paramref name="relativeFilePath"/> is supposed to be relative against (executing) project root.
        /// <para><paramref name="relativeFilePath"/> has to contain extension.</para>
        /// <para>
        /// If there are more resource which name ends with <paramref name="relativeFilePath"/>,
        /// very first found is returned (see <see  cref="Assembly.GetManifestResourceNames()"/> for further insights).
        /// </para>
        /// <para>
        /// In order to obtain correct (partial) embedded resource name, symbols "\" and other invalid characters are replaced with "." and "_" respectively
        /// in the <paramref name="relativeFilePath"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when null or empty <paramref name="relativeFilePath"/> provided.</exception>
        /// <exception cref="EmbeddedResourceNotFoundException">Thrown when either embedded resource full name or resource stream was not found.</exception>
        public string ReadResourceFile(string relativeFilePath)
        {
            var resourceStream = GetResourceStream(relativeFilePath);

            // Read embedded resource's content
            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }


        /// <summary>
        /// Returns content of embedded resource specified by <paramref name="relativeFilePath"/> that is stored in the <see cref="EmbeddedResourceAssembly"/>.
        /// </summary>
        /// <param name="relativeFilePath">Relative path to a single embedded resource (including folders).</param>
        /// <remarks>
        /// <paramref name="relativeFilePath"/> is supposed to be relative against (executing) project root.
        /// <para><paramref name="relativeFilePath"/> has to contain extension.</para>
        /// <para>
        /// If there are more resource which name ends with <paramref name="relativeFilePath"/>,
        /// very first found is returned (see <see  cref="Assembly.GetManifestResourceNames()"/> for further insights).
        /// </para>
        /// <para>
        /// In order to obtain correct (partial) embedded resource name, symbols "\" and other invalid characters are replaced with "." and "_" respectively
        /// in the <paramref name="relativeFilePath"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when null or empty <paramref name="relativeFilePath"/> provided.</exception>
        /// <exception cref="EmbeddedResourceNotFoundException">Thrown when either embedded resource full name or resource stream was not found.</exception>
        public Stream GetResourceStream(string relativeFilePath)
        {
            if (String.IsNullOrEmpty(relativeFilePath))
            {
                throw new ArgumentException("Provided relative file name cannot be null nor empty.", "relativeFilePath");
            }

            // Get resource name out of the relativeFilePath
            var partialResourceName = GetResourceNameFromPath(relativeFilePath);

            // Try match the partial resource name with existing resource names
            var fullResourceName = ManifestEmbeddedResourceFileNames.FirstOrDefault(resourceFullName => resourceFullName.EndsWith(partialResourceName));
            if (String.IsNullOrEmpty(fullResourceName))
            {
                throw new EmbeddedResourceNotFoundException(partialResourceName);
            }

            // Get embedded resource's stream from assembly
            var resourceStream = EmbeddedResourceAssembly.GetManifestResourceStream(fullResourceName);
            if (resourceStream == null)
            {
                throw new EmbeddedResourceNotFoundException(fullResourceName);
            }
            return resourceStream;
        }


        /// <summary>
        /// Copies content of embedded resource specified by <paramref name="resourceRelativeFilePath"/> that is stored in the <see cref="EmbeddedResourceAssembly"/> to file system.
        /// </summary>
        /// <param name="resourceRelativeFilePath">Relative path to a single embedded resource (including folders).</param>
        /// <param name="targetFullPath">Full path to file being saved in file system.</param>
        public void CopyBytesToFileSystem(string resourceRelativeFilePath, string targetFullPath)
        {
            if (String.IsNullOrEmpty(resourceRelativeFilePath))
            {
                throw new ArgumentException("Provided relative file name cannot be null nor empty.", "resourceRelativeFilePath");
            }

            if (String.IsNullOrEmpty(targetFullPath))
            {
                throw new ArgumentException("Provided full path cannot be null nor empty.", "targetFullPath");
            }

            using (var stream = GetResourceStream(resourceRelativeFilePath))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(targetFullPath, buffer);
            }
        }


        /// <summary>
        /// Transforms given path into its embedded resource name equivalent.
        /// </summary>
        /// <param name="path">Path to the embedded resource.</param>
        /// <remarks>
        /// Path should never be absolute. Relativity root might differ, however, project root is the deepest one supported.
        /// <para>Method expects the <paramref name="path"/> not to be null nor empty.</para>
        /// </remarks>
        private String GetResourceNameFromPath(string path)
        {
            var resourceNameSpaces = Enumerable
                .Empty<string>()
                .Union(Path.GetDirectoryName(path).Split(Path.DirectorySeparatorChar))
                .Select(resourceNamespace => StronglyTypedResourceBuilder.VerifyResourceName(resourceNamespace, CodeDomProvider))
                .Concat(new[] { Path.GetFileNameWithoutExtension(path) })
                .Concat(new[] { Path.GetExtension(path).Replace(PATH_EXTENSION_SEPARATOR, String.Empty) });

            return String.Join(RESOURCE_NAMESPACE_SEPARATOR, resourceNameSpaces);
        }
    }
}
