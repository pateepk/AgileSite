using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using CMS.Modules.NuGetPackages;

using NuGet;

using SystemIO = System.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Builder class for a NuSpec manifest. Allows for configuration of a NuSpec manifest which can be used for NuGet package creation.
    /// </summary>
    /// <remarks>http://docs.nuget.org/docs/reference/nuspec-reference</remarks>
    public class NuSpecBuilder
    {
        #region "Constants"

        private const string NUSPEC_LIB_DIR = "lib";
        private const string NUSPEC_CONTENT_DIR = "content";
        private const string NUSPEC_TOOLS_DIR = "tools";

        #endregion


        #region "Fields"

        private int mMinimumManifestVersion = 1;
        private readonly List<ManifestFile> mFiles = new List<ManifestFile>();
        private readonly ModulePackageMetadata mModulePackageMetadata;

        #endregion


        #region "Properties"


        /// <summary>
        /// Gets or sets a relative path to the package's readme file
        /// If null, no readme is included
        /// </summary>
        public string ReadmeFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum NuSpec manifest version. The actual version is determined
        /// at the time of build.
        /// Default version is 1.
        /// </summary>
        public int MinimumManifestVersion
        {
            get
            {
                return mMinimumManifestVersion;
            }
            set
            {
                mMinimumManifestVersion = value;
            }
        }


        /// <summary>
        /// Gets or sets the minimum version of NuGet required when installing the package. Optional.
        /// </summary>
        public string MinimumClientVersion
        {
            get;
            set;
        }


        /// <summary>
        /// Gets module's metadata.
        /// </summary>
        public ModulePackageMetadata ModulePackageMetadata
        {
            get
            {
                return mModulePackageMetadata;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes new NuSpecBuilder.
        /// </summary>
        /// <param name="modulePackageMetadata">Module's metadata.</param>
        public NuSpecBuilder(ModulePackageMetadata modulePackageMetadata)
        {
            mModulePackageMetadata = modulePackageMetadata;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds a library to the manifest.
        /// The library can be either framework specific, or not.
        /// </summary>
        /// <param name="src">Source path to the library, folder with libraries, or pattern.</param>
        /// <param name="dst">Relative destination path of the library within the package's lib folder. Null or empty string means root.</param>
        /// <param name="exclude">Excluded files from src (when src is not a single library)</param>
        /// <param name="targetFramework">Target framework for the library (i.e. "net40", "net45").</param>
        /// <remarks>http://docs.nuget.org/docs/reference/nuspec-reference#Set_Of_DLLs</remarks>
        public void AddLibrary(string src, string dst, string exclude = null, string targetFramework = null)
        {
            AddManifestFile(src, dst, NUSPEC_LIB_DIR, exclude, targetFramework);
        }


        /// <summary>
        /// Adds a content to the manifest.
        /// </summary>
        /// <param name="src">Source path to the content file, folder with content files, or pattern.</param>
        /// <param name="dst">Relative destination path of the content within the package's content folder. Null or empty string means root.</param>
        /// <param name="exclude">Excluded files from src (when src is not a single file)</param>
        /// <param name="targetFramework">Target framework for the content (i.e. "net40", "net45"). Supported in NuGet version 2.0 and above.</param>
        /// <remarks>http://docs.nuget.org/docs/reference/nuspec-reference#Content_Files</remarks>
        public void AddContent(string src, string dst, string exclude = null, string targetFramework = null)
        {
            AddManifestFile(src, dst, NUSPEC_CONTENT_DIR, exclude, targetFramework);
        }


        /// <summary>
        /// Adds tools to the manifest.
        /// </summary>
        /// <param name="src">Source path to the tool file, folder with tool files, or pattern.</param>
        /// <param name="dst">Relative destination path of the files within the package's tools folder. Null or empty string means root.</param>
        /// <param name="exclude">Excluded files from src (when src is not a single file)</param>
        /// <param name="targetFramework">Target framework for the content (i.e. "net40", "net45"). Supported in NuGet version 2.0 and above.</param>
        public void AddTools(string src, string dst, string exclude = null, string targetFramework = null)
        {
            AddManifestFile(src, dst, NUSPEC_TOOLS_DIR, exclude, targetFramework);
        }


        /// <summary>
        /// Builds NuSpec manifest to <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Stream for writing a NuSpec manifest.</param>
        /// <exception cref="ValidationException">Thrown When the resulting NuSpec manifest is not valid.</exception>
        /// <seealso cref="ModulePackagingEvents.BuildNuSpecManifest"/>
        public void BuildToStream(SystemIO.Stream stream)
        {
            Manifest manifest;

            var eventArgs = new BuildNuSpecManifestEventArgs(ModulePackageMetadata.Id) { NuSpecBuilder = this };
            using (var h = ModulePackagingEvents.Instance.BuildNuSpecManifest.StartEvent(eventArgs))
            {
                manifest = BuildManifest();
                h.EventArguments.Manifest = manifest;

                h.FinishEvent();
            }

            // The data is validated during save              
            eventArgs.Manifest.Save(stream, true, MinimumManifestVersion);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Builds the manifest (metadata, files).
        /// </summary>
        /// <returns>Manifest built from current properties.</returns>
        private Manifest BuildManifest()
        {
            // Create object representation of NuSpec
            Manifest manifest = new Manifest
            {
                Metadata = new ManifestMetadata
                {
                    Id = ModulePackageMetadata.Id,
                    Title = ModulePackageMetadata.Title,
                    Version = ModulePackageMetadata.Version,
                    Authors = ModulePackageMetadata.Authors,
                    Description = ModulePackageMetadata.Description,
                    MinClientVersionString = MinimumClientVersion,
                },

                // Make sure that mFiles isn't affected by the Manifest usage
                Files = new List<ManifestFile>(mFiles),
            };

            if (ReadmeFilePath != null)
            {
                var readmeManifestFile = new ManifestFile
                {
                    Source = ReadmeFilePath,
                    Target = "readme.txt",
                };
                manifest.Files.Add(readmeManifestFile);
            }

            return manifest;
        }


        /// <summary>
        /// Adds file or files to the manifest.
        /// </summary>
        /// <param name="src">Source path to the file, folder with tool files, or pattern.</param>
        /// <param name="dst">Relative destination path of the files within the package's folder specified by <paramref name="packageFolder"/>. Null or empty string means root.</param>
        /// <param name="packageFolder">Name of the package folder. <paramref name="dst"/> is relative to specified folder.</param>
        /// <param name="exclude">Excluded files from src (when src is not a single file)</param>
        /// <param name="targetFramework">Target framework for the content (i.e. "net40", "net45"). Supported in NuGet version 2.0 and above.</param>
        private void AddManifestFile(string src, string dst, string packageFolder, string exclude = null, string targetFramework = null)
        {
            ManifestFile manifestFile = new ManifestFile
            {
                Source = src,
                Target = BuildTargetPath(packageFolder, dst, targetFramework),
                Exclude = exclude
            };
            mFiles.Add(manifestFile);
        }


        /// <summary>
        /// Builds target path based on folder ("lib", "content", etc.), destination and target framework (optional).
        /// </summary>
        /// <param name="folder">Folder within the package.</param>
        /// <param name="dst">Relative destination path within folder (or target framework folder).</param>
        /// <param name="targetFramework">Target framework (optional).</param>
        /// <returns>Target path built from given arguments.</returns>
        private string BuildTargetPath(string folder, string dst, string targetFramework = null)
        {
            StringBuilder target = new StringBuilder();
            target.Append(folder);
            if (!String.IsNullOrEmpty(targetFramework))
            {
                target.Append("\\").Append(targetFramework);
            }
            if (!String.IsNullOrEmpty(dst))
            {
                if (!dst.StartsWith("\\", StringComparison.Ordinal))
                {
                    target.Append("\\");
                }
                target.Append(dst);
            }

            return target.ToString();
        }

        #endregion

    }
}
