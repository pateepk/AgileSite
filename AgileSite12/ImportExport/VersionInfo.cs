using System;
using System.Globalization;

using CMS.Base;
using CMS.Helpers;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class for version info.
    /// </summary>
    public class VersionInfo : IComparable
    {
        #region "Variables"

        private float mVersion;
        private int mHotfixVersion;
        private char mSubVersion = '#';
        private int mBetaVersion;
        private int mRCVersion;
        private bool mIsBeta;
        private bool mIsRC;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets main version number.
        /// </summary>
        public float Version
        {
            get
            {
                return mVersion;
            }
        }


        /// <summary>
        /// Gets hotfix version number.
        /// </summary>
        public int HotfixVersion
        {
            get
            {
                return mHotfixVersion;
            }
        }


        /// <summary>
        /// Gets sub version character.
        /// </summary>
        public char SubVersion
        {
            get
            {
                return mSubVersion;
            }
        }


        /// <summary>
        /// Gets beta version.
        /// </summary>
        public int BetaVersion
        {
            get
            {
                return mBetaVersion;
            }
        }


        /// <summary>
        /// Gets RC version.
        /// </summary>
        public int RCVersion
        {
            get
            {
                return mRCVersion;
            }
        }


        /// <summary>
        /// Indicates if version is beta.
        /// </summary>
        public bool IsBeta
        {
            get
            {
                return mIsBeta;
            }
        }


        /// <summary>
        /// Indicates if version is RC.
        /// </summary>
        public bool IsRC
        {
            get
            {
                return mIsRC;
            }
        }


        /// <summary>
        /// Indicates if version is normal.
        /// </summary>
        public bool IsNormal
        {
            get
            {
                return !mIsRC && !mIsBeta;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates version info.
        /// </summary>
        /// <param name="versionString">Version string</param>
        /// <param name="hotfixString">Hotfix version string</param>
        public VersionInfo(string versionString, string hotfixString)
        {
            if ((versionString == null) || (versionString.Length < 3) || (!versionString.Contains(".")))
            {
                throw new Exception("[VersionInfo]: Given version isn't in valid format!");
            }

            versionString = versionString.ToLowerCSafe();
            mIsBeta = versionString.Contains("beta");
            mIsRC = versionString.Contains("rc");

            // Get version
            int length = versionString.IndexOfCSafe('.') + 2;
            mVersion = float.Parse(versionString.Substring(0, length), CultureInfo.InvariantCulture);

            // Get BETA version
            if (IsBeta && !versionString.EndsWithCSafe("beta"))
            {
                mBetaVersion = ValidationHelper.GetInteger(versionString.Substring(versionString.IndexOfCSafe("beta") + 4), 0);
            }

            // Get RC version
            if (IsRC && !versionString.EndsWithCSafe("rc"))
            {
                mRCVersion = ValidationHelper.GetInteger(versionString.Substring(versionString.IndexOfCSafe("rc") + 2), 0);
            }

            // Get subversion
            string versionRest = versionString.Substring(length);
            if (!string.IsNullOrEmpty(versionRest) && !versionRest.StartsWithCSafe("beta") && !versionRest.StartsWithCSafe("rc"))
            {
                mSubVersion = versionRest[0];
            }

            if (hotfixString != null)
            {
                mHotfixVersion = ValidationHelper.GetInteger(hotfixString, 0);
            }
        }

        #endregion


        #region "IComparable Members"

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="obj">Version info object</param>
        public int CompareTo(object obj)
        {
            if (obj is VersionInfo)
            {
                VersionInfo version = (VersionInfo)obj;

                // Main version is lower
                if (Version < version.Version)
                {
                    return -1;
                }
                    // Identical main versions
                else if (Version == version.Version)
                {
                    // Sub version is lower
                    if (SubVersion.CompareTo(version.SubVersion) < 0)
                    {
                        return -1;
                    }
                        // Equal subversions
                    else if (SubVersion.CompareTo(version.SubVersion) == 0)
                    {
                        // Compare hotfix versions
                        if (HotfixVersion < version.HotfixVersion)
                        {
                            return -1;
                        }
                            // Both are BETA, compare BETA versions
                        else if (IsBeta && version.IsBeta)
                        {
                            return (BetaVersion < version.BetaVersion) ? -1 : 1;
                        }
                            // Both are RC, comapre RC versions
                        else if (IsRC && version.IsRC)
                        {
                            return (RCVersion < version.RCVersion) ? -1 : 1;
                        }
                        else if ((IsBeta && version.IsNormal) || (IsRC && version.IsNormal) || (IsBeta && version.IsRC))
                        {
                            return -1;
                        }
                    }
                }

                return 1;
            }
            else
            {
                throw new Exception("[VersionInfo]: Object is not type of VersionInfo!");
            }
        }

        #endregion
    }
}