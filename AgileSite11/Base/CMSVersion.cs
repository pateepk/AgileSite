using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// CMS version
    /// </summary>
    public class CMSVersion
    {
        #region "Variables"

        private static Version mVersion;
        private static string mMainVersion;
        private static FileVersionInfo mFileVersionInfo = null;
        private static string mInformationalVersion = null;
        private static string mVersionSuffix = null;
        private static Regex mInformationalVersionRegex = null;
        private static Version mFileVersion = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Hotfix version.
        /// </summary>
        public static string HotfixVersion
        {
            get
            {
                return Version.Build.ToString();
            }
        }


        /// <summary>
        /// Version of the system, e.g. "8.0 RC"
        /// </summary>
        public static string MainVersion
        {
            get
            {
                return mMainVersion ?? (mMainVersion = GetVersion(true, true, false, true));
            }
        }


        /// <summary>
        /// Returns the system version.
        /// </summary>
        /// <returns>Actual version of CMS</returns>
        public static Version Version
        {
            get
            {
                if (mVersion == null)
                {
                    string versionString = InformationalVersionRegex.Match(InformationalVersion).Groups["version"].Value;

                    if (!string.IsNullOrEmpty(versionString))
                    {
                        mVersion = new Version(versionString);
                    }
                }

                return mVersion;
            }
        }


        /// <summary>
        /// Suffix of the system version, e.g. "BETA"
        /// </summary>
        public static string VersionSuffix
        {
            get
            {
                return mVersionSuffix ?? (mVersionSuffix = InformationalVersionRegex.Match(InformationalVersion).Groups["suffix"].Value);
            }
        }


        #region "Private properties"

        /// <summary>
        /// Gets [AssemblyInformationalVersion] of the current assembly.
        /// </summary>
        private static string InformationalVersion
        {
            get
            {
                return mInformationalVersion ?? (mInformationalVersion = FileVersionInfo.ProductVersion);
            }
        }               


        private static Regex InformationalVersionRegex
        {
            get
            {
                return mInformationalVersionRegex ?? (mInformationalVersionRegex = new Regex(@"(?<version>\d+\.\d+\.\d+)\s*(?<suffix>\S*)"));
            }
        }


        /// <summary>
        /// Gets [AssemblyFileVersion] of the current assembly.
        /// Contains 4 digits separated by dot.
        /// </summary>
        private static Version FileVersion
        {
            get
            {
                return mFileVersion ?? (mFileVersion = new Version(FileVersionInfo.FileVersion));
            }
        }


        /// <summary>
        /// Gets file version info of current assembly, which is used to determine product versions.
        /// </summary>
        private static FileVersionInfo FileVersionInfo
        {
            get
            {
                return mFileVersionInfo ?? (mFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location));
            }
        }

        #endregion

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns string representation of system version. Private since build and hotfix numbers are mutually exclusive.
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="build">Build number</param>
        /// <param name="hotfix">Hotfix number</param>
        /// <param name="revision">Revision number</param>
        /// <param name="suffix">Version suffix</param>
        private static string GetVersion(bool major, bool minor, bool build, bool hotfix, bool revision, bool suffix)
        {
            // Build the version string
            var sb = new StringBuilder();

            var version = Version;

            if (major)
            {
                sb.Append(version.Major);
            }

            if (minor)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(version.Minor);
            }

            if (build)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(FileVersion.Build);
            }

            if (hotfix)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(HotfixVersion);
            }

            if (revision)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(FileVersion.Revision);
            }

            if (suffix)
            {
                if ((sb.Length > 0) && !string.IsNullOrEmpty(VersionSuffix))
                {
                    sb.Append(" ");
                }
                sb.Append(VersionSuffix);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns string representation of full hotfix version. All parts of the version are optional.
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="hotfix">Hotfix version</param>
        /// <param name="suffix">Version suffix</param>
        public static string GetVersion(bool major, bool minor, bool hotfix, bool suffix)
        {
            return GetVersion(major, minor, false, hotfix, false, suffix);
        }


        /// <summary>
        /// Returns string representation of full system version. All parts of the version are optional.
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="build">Build number</param>
        /// <param name="revision">Revision number</param>
        /// <param name="suffix">Version suffix</param>
        public static string GetVersion(bool major, bool minor, bool build, bool revision, bool suffix)
        {
            return GetVersion(major, minor, build, false, revision, suffix);
        }


        /// <summary>
        /// Indicates if current version is BETA version.
        /// </summary>
        public static bool IsBetaVersion()
        {
            return VersionSuffix.ToLowerCSafe().Contains("beta");
        }


        /// <summary>
        /// Returns system version text.
        /// </summary>
        /// <param name="includeBuildNumber">Includes buildnumber</param>
        public static string GetFriendlySystemVersion(bool includeBuildNumber)
        {
            StringBuilder sb = new StringBuilder(Version.Major + "." + Version.Minor);
            if (!String.IsNullOrEmpty(VersionSuffix))
            {
                sb.Append("&nbsp;");
                sb.Append(VersionSuffix);
            }
            
            if (includeBuildNumber)
            {
                sb.Append("&nbsp;");
                sb.Append(CoreServices.Localization.GetFileString("Footer.Build"));
                sb.Append("&nbsp;");
                sb.Append(GetVersion(true, true, true, false, false));
            }

            return sb.ToString();
        }

        #endregion
    }
}
