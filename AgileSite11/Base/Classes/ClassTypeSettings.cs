using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Class type settings class.
    /// </summary>
    public class ClassTypeSettings
    {
        #region "Variables"

        private bool mShowClasses = true;
        private List<Type> mBaseTypes;
        private string mBaseClassNames;
        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether automatic type creation by system should be checked.
        /// </summary>
        /// <remarks>Checks whether system is able to create type automatically</remarks>
        public bool CheckAutoCreation
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if classes should be selected.
        /// </summary>
        public bool ShowClasses
        {
            get
            {
                return mShowClasses;
            }
            set
            {
                mShowClasses = value;
            }
        }


        /// <summary>
        /// Indicates if enumerations should be selected.
        /// </summary>
        public bool ShowEnumerations
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if interfaces should be selected.
        /// </summary>
        public bool ShowInterfaces
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets base class names, separated by semi-colon, to filter classes.
        /// </summary>
        public string BaseClassNames
        {
            get
            {
                return mBaseClassNames;
            }
            set
            {
                mBaseClassNames = value;
                mBaseTypes = null;
            }
        }


        /// <summary>
        /// Gets list of current base types for class validation
        /// </summary>
        public List<Type> BaseTypes
        {
            get
            {
                if (mBaseTypes == null && !String.IsNullOrEmpty(BaseClassNames))
                {
                    mBaseTypes = SplitAndTrimNames(BaseClassNames).Select(p => Type.GetType(p)).ToList();
                }

                return mBaseTypes;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseClassNames">Base class names (separated by semi-colon) to filter its child classes.</param>
        /// <param name="showClasses">Indicates if classes should be selected.</param>
        /// <param name="showEnumerations">Indicates if enumerations should be selected.</param>
        /// <param name="showInterfaces">Indicates if interfaces should be selected.</param>
        public ClassTypeSettings(string baseClassNames = null, bool showClasses = true, bool showEnumerations = false, bool showInterfaces = false)
        {
            BaseClassNames = baseClassNames;
            ShowClasses = showClasses;
            ShowEnumerations = showEnumerations;
            ShowInterfaces = showInterfaces;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns key to access cached data for given assembly name.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        public string GetCacheKey(string assemblyName)
        {
            return assemblyName + "|" + BaseClassNames + "|" + ShowClasses + "|" + ShowEnumerations + "|" + ShowInterfaces;
        }


        /// <summary>
        /// Checks if selected class is child of given interface.
        /// </summary>
        /// <param name="type">Type of selected class</param>
        public bool CheckInterfaces(Type type)
        {
            if (!String.IsNullOrEmpty(BaseClassNames))
            {
                return SplitAndTrimNames(BaseClassNames).Any(i => type.GetInterface(i, true) != null);
            }

            return false;
        }


        private IEnumerable<string> SplitAndTrimNames(string names)
        {
            return names.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());
        }

        #endregion
    }
}
