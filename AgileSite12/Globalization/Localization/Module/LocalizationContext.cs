using System;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Localization
{
    /// <summary>
    /// Localization related context methods and variables.
    /// </summary>
    [RegisterAllProperties]
    public class LocalizationContext : AbstractContext<LocalizationContext>
    {
        #region "Variables"

        /// <summary>
        /// Preferred culture code.
        /// </summary>
        protected static Property<string> mPreferredCultureCode = new Property<string>(CultureHelper.GetPreferredCulture, CultureHelper.SetPreferredCulture);

        private CultureInfo mCurrentCulture;
        private CultureInfo mCurrentUICulture;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Current culture info object according to the URL parameter of the current request. 
        /// Request can contain parameter "culturecode" or "cultureid" with valid value of the culture.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get
            {
                return GetCurrentCulture();
            }
            set
            {
                Current.mCurrentCulture = value;
            }
        }


        /// <summary>
        /// Current UI culture info object.
        /// </summary>
        public static CultureInfo CurrentUICulture
        {
            get
            {
                return GetCurrentUICulture();
            }
            set
            {
                Current.mCurrentUICulture = value;
            }
        }


        /// <summary>
        /// Preferred culture code.
        /// </summary>
        [RegisterColumn]
        public static string PreferredCultureCode
        {
            get
            {
                return mPreferredCultureCode;
            }
            set
            {
                mPreferredCultureCode.Value = value;
            }
        }


        /// <summary>
        /// Preferred UI culture code.
        /// </summary>
        [RegisterColumn]
        public static string PreferredUICultureCode
        {
            get
            {
                return CultureHelper.GetPreferredUICultureCode();
            }
            set
            {
                CultureHelper.SetPreferredUICultureCode(value);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns information on the current culture.
        /// </summary>
        public static CultureInfo GetCurrentCulture()
        {
            // Try to get the culture info from the request items collection
            var c = Current;
            CultureInfo cultureInfo = c.mCurrentCulture;

            if (cultureInfo == null)
            {
                // Try to get culture by id
                int cultureId = QueryHelper.GetInteger("cultureid", 0);
                if (cultureId > 0)
                {
                    cultureInfo = CultureInfoProvider.GetCultureInfo(cultureId);
                }

                // If culture was not found by it's culture id, try to get culture by it's code
                if (cultureInfo == null)
                {
                    string cultureCode = QueryHelper.GetString("culturecode", String.Empty);
                    if (!String.IsNullOrEmpty(cultureCode))
                    {
                        cultureInfo = CultureInfoProvider.GetCultureInfo(cultureCode);
                    }
                }

                // If culture was not found either, try to get preferred culture
                if (cultureInfo == null)
                {
                    cultureInfo = CultureInfoProvider.GetCultureInfo(CultureHelper.GetPreferredCulture());
                }

                // If culture was not found either, try to get default culture
                if (cultureInfo == null)
                {
                    cultureInfo = CultureInfoProvider.GetCultureInfo(CultureHelper.DefaultUICultureCode);
                }

                c.mCurrentCulture = cultureInfo;
            }

            return cultureInfo;
        }


        /// <summary>
        /// Returns information on the current UI culture.
        /// </summary>
        public static CultureInfo GetCurrentUICulture()
        {
            var c = Current;

            CultureInfo cultureInfo = c.mCurrentUICulture;
            if (cultureInfo == null)
            {
                cultureInfo = CultureInfoProvider.GetCultureInfo(CultureHelper.GetPreferredUICultureCode());

                if (cultureInfo == null)
                {
                    cultureInfo = CultureInfoProvider.GetCultureInfo(CultureHelper.DefaultUICultureCode);
                }

                c.mCurrentUICulture = cultureInfo;
            }

            return cultureInfo;
        }


        /// <summary>
        /// Clones the object for a new thread
        /// </summary>
        public override object CloneForNewThread()
        {
            var result = new LocalizationContext();

            result.mCurrentCulture = mCurrentCulture;
            result.mCurrentUICulture = mCurrentUICulture;

            // Do not copy tracked resources
            //result.mCurrentResources = mCurrentResources;

            return result;
        }

        #endregion
    }
}
