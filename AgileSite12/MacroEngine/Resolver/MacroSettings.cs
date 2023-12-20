using System;
using System.Collections.Generic;

namespace CMS.MacroEngine
{
    /// <summary>
    /// <see cref="MacroSettings"/> class specifies basic features of a <see cref="MacroResolver"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class MacroSettings
    {
        #region "Variables"

        private bool? mAllowContextMacros;
        private bool mEncapsulateMacroObjects = true;
        private bool mAllowRecursion = false;
        private bool mCheckSecurity = true;
        private bool mCheckIntegrity = true;
        private int mEvaluationTimeout = MacroStaticSettings.EvaluationTimeout;
        private int mMaxRecursionLevel = MacroStaticSettings.DEFAULT_MAX_RECURSION_LEVEL;

        #endregion


        #region "Properties"

        /// <summary>
        /// Related object allows set object used for resolving in macro method. Used in web part properties.
        /// </summary>
        public object RelatedObject 
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the properties from contexts are not resolved unless explicitly registered.
        /// </summary>
        public bool AllowContextMacros
        {
            get
            {
                if (mAllowContextMacros == null)
                {
                    return MacroStaticSettings.AllowContextMacros;
                }
                return mAllowContextMacros.Value;
            }
            set
            {
                mAllowContextMacros = value;
            }
        }


        /// <summary>
        /// If true, the object will be encapsulated to its macro representations.
        /// </summary>
        public bool EncapsulateMacroObjects
        {
            get
            {
                return mEncapsulateMacroObjects;
            }
            set
            {
                mEncapsulateMacroObjects = value;
            }
        }


        /// <summary>
        /// If true, unresolved macros are kept in their original form.
        /// </summary>
        public bool KeepUnresolvedMacros
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the security is checked within macro evaluation.
        /// </summary>
        public bool CheckSecurity
        {
            get
            {
                return mCheckSecurity;
            }
            set
            {
                mCheckSecurity = value;
            }
        }


        /// <summary>
        /// If true, the integrity of security parameters is checked within macro evaluation.
        /// </summary>
        public bool CheckIntegrity
        {
            get
            {
                return mCheckIntegrity;
            }
            set
            {
                mCheckIntegrity = value;
            }
        }


        /// <summary>
        /// Gets or sets the identity option to be used to verify security when CheckIntegrity is false.
        /// </summary>
        public MacroIdentityOption IdentityOption
        {
            get;
            set;
        }


        /// <summary>
        /// Expression evaluation timeout in milliseconds. If the evaluation time exceeds this time, evaluation will be aborted and the result will be null.
        /// </summary>
        public int EvaluationTimeout
        {
            get
            {
                return mEvaluationTimeout;
            }
            set
            {
                mEvaluationTimeout = value;
            }
        }


        /// <summary>
        /// If true, all the context macros are disabled (only base MacroResolver sources are checked).
        /// </summary>
        public bool DisableContextMacros
        {
            get;
            set;
        }


        /// <summary>
        /// If true, page context macros are not available (CurrentDocument, CurrentPageInfo).
        /// </summary>
        public bool DisablePageContextMacros
        {
            get;
            set;
        }


        /// <summary>
        /// If true, context objects (such as ForumContext, CommunityContext, CMSContext, ...) will not be resolved.
        /// </summary>
        public bool DisableContextObjectMacros
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the virtual mode is enabled for the macro resolver, meaning that the resolver always tries to return non-null values. Instead, empty objects are provided.
        /// </summary>
        public bool VirtualMode
        {
            get;
            set;
        }


        /// <summary>
        /// Culture used for the resolving.
        /// </summary>
        public string Culture
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the resolving of the macros should avoid SQL injection (escapes the apostrophes in output). Default value is false
        /// </summary>
        public bool AvoidInjection
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the resolved macro values are encoded to avoid XSS. Default value is false
        /// </summary>
        public bool EncodeResolvedValues
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the recursion is allowed within the macro resolving. Default value is false.
        /// </summary>
        public bool AllowRecursion
        {
            get
            {
                return mAllowRecursion;
            }
            set
            {
                mAllowRecursion = value;
            }
        }


        /// <summary>
        /// Gets or sets the maximal recursion level.
        /// </summary>
        public int MaxRecursionlevel
        {
            get
            {
                return mMaxRecursionLevel;
            }
            set
            {
                mMaxRecursionLevel = value;
            }
        }


        /// <summary>
        /// If true, the resolving tracks cache dependencies. Default value is false.
        /// </summary>
        public bool TrackCacheDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Outputs the cache dependencies collected during the processing.
        /// </summary>
        public List<string> CacheDependencies
        {
            get;
            private set;
        }


        /// <summary>
        /// Outputs the file cache dependencies collected during the processing.
        /// </summary>
        public List<string> FileCacheDependencies
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets the function used as timeout watchdog besides EvaluationTimeout setting. Returns true if timeout has occurred.
        /// </summary>
        public Func<bool> ExternalTimeoutChecker
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new, empty instance of <see cref="MacroSettings"/> class.
        /// </summary>
        public MacroSettings()
        {
        }


        /// <summary>
        /// Adds the cache dependencies to the macro context.
        /// </summary>
        /// <param name="files">Files to add</param>
        public void AddFileCacheDependencies(IEnumerable<string> files)
        {
            if (FileCacheDependencies == null)
            {
                FileCacheDependencies = new List<string>(files);
            }
            else
            {
                FileCacheDependencies.AddRange(files);
            }
        }


        /// <summary>
        /// Adds the cache dependencies to the macro context.
        /// </summary>
        /// <param name="items">Items to add</param>
        public void AddCacheDependencies(IEnumerable<string> items)
        {
            if (CacheDependencies == null)
            {
                CacheDependencies = new List<string>(items);
            }
            else
            {
                CacheDependencies.AddRange(items);
            }
        }


        /// <summary>
        /// Creates a shallow copy of the <see cref="MacroSettings"/>.
        /// </summary>
        public MacroSettings Clone()
        {
            MacroSettings result = new MacroSettings();

            result.RelatedObject = RelatedObject;
            result.AllowContextMacros = AllowContextMacros;
            result.EncapsulateMacroObjects = EncapsulateMacroObjects;
            result.KeepUnresolvedMacros = KeepUnresolvedMacros;
            result.CheckSecurity = CheckSecurity;
            result.CheckIntegrity = CheckIntegrity;
            result.IdentityOption = IdentityOption;
            result.EvaluationTimeout = EvaluationTimeout;
            result.DisableContextMacros = DisableContextMacros;
            result.DisablePageContextMacros = DisablePageContextMacros;
            result.DisableContextObjectMacros = DisableContextObjectMacros;
            result.VirtualMode = VirtualMode;
            result.Culture = Culture;
            result.AvoidInjection = AvoidInjection;
            result.EncodeResolvedValues = EncodeResolvedValues;
            result.AllowRecursion = AllowRecursion;
            result.MaxRecursionlevel = MaxRecursionlevel;
            result.TrackCacheDependencies = TrackCacheDependencies;
            result.ExternalTimeoutChecker = ExternalTimeoutChecker;

            if (CacheDependencies != null)
            {
                result.CacheDependencies = new List<string>(CacheDependencies);
            }

            if (FileCacheDependencies != null)
            {
                result.FileCacheDependencies = new List<string>(FileCacheDependencies);
            }

            return result;
        }

        #endregion
    }
}