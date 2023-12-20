using System.Collections;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class encapsulating parameters for object cloning (used in InsertAsClone settings)
    /// </summary>
    public class CloneSettings
    {
        #region "Variables"

        private bool mIncludeChildren = true;
        private bool mIncludeBindings = true;
        private bool mIncludeOtherBindings = true;
        private bool mIncludeSiteBindings = true;
        private bool mIncludeMetafiles = true;
        private int mMaxRelativeLevel = -1;
        private List<string> mExcludedChildTypes;
        private List<string> mExcludedBindingTypes;
        private List<string> mExcludedOtherBindingTypes;
        private TranslationHelper mTranslations;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the translation helper object (always at least empty instance, cannot return null) which provides translation Original object ID -> Cloned object ID.
        /// </summary>
        public TranslationHelper Translations
        {
            get
            {
                if (mTranslations == null)
                {
                    mTranslations = new TranslationHelper();
                }
                return mTranslations;
            }
            set
            {
                mTranslations = value;
            }
        }


        /// <summary>
        /// Hashtable with custom parameters (for special cases such as BizForms, CustomTables, Webparts, etc.) where additional operations have to be done.
        /// </summary>
        public Hashtable CustomParameters
        {
            get;
            set;
        }


        /// <summary>
        /// Custom parameter passed within the settings (for handlers usage, etc.).
        /// </summary>
        public object Parameter
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the root object - the object the cloning procedure was called on (for example when cloning country with states, in the states the root object will be still the country).
        /// </summary>
        public BaseInfo CloneBase
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the list of child object types which should not be cloned, if null than all child object types are cloned.
        /// </summary>
        public List<string> ExcludedChildTypes
        {
            get
            {
                return mExcludedChildTypes ?? (mExcludedChildTypes = new List<string>());
            }
            set
            {
                mExcludedChildTypes = value;
            }
        }


        /// <summary>
        /// Gets or sets the list of binding object types which should not be cloned, if null than all child object types are cloned.
        /// </summary>
        public List<string> ExcludedBindingTypes
        {
            get
            {
                return mExcludedBindingTypes ?? (mExcludedBindingTypes = new List<string>());
            }
            set
            {
                mExcludedBindingTypes = value;
            }
        }


        /// <summary>
        /// Gets or sets the list of other binding object types which should not be cloned, if null than all child object types are cloned.
        /// </summary>
        public List<string> ExcludedOtherBindingTypes
        {
            get
            {
                return mExcludedOtherBindingTypes ?? (mExcludedOtherBindingTypes = new List<string>());
            }
            set
            {
                mExcludedOtherBindingTypes = value;
            }
        }


        /// <summary>
        /// New code name. If null, automatically generated unique code name is generated.
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// New display name. If null, automatically generated unique display name is generated.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// If object has SiteBinding than a binding for given site is created after the object is cloned.
        /// </summary>
        public int AssignToSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, child objects are included in the result.
        /// </summary>
        public bool IncludeChildren
        {
            get
            {
                return mIncludeChildren;
            }
            set
            {
                mIncludeChildren = value;
            }
        }


        /// <summary>
        /// If true, binding objects are included in the result.
        /// </summary>
        public bool IncludeBindings
        {
            get
            {
                return mIncludeBindings;
            }
            set
            {
                mIncludeBindings = value;
            }
        }


        /// <summary>
        /// If true, other binding objects are included in the result.
        /// </summary>
        public bool IncludeOtherBindings
        {
            get
            {
                return mIncludeOtherBindings;
            }
            set
            {
                mIncludeOtherBindings = value;
            }
        }


        /// <summary>
        /// If true, site binding objects are included in the result. This setting is applied only when IncludeBindings is true.
        /// </summary>
        public bool IncludeSiteBindings
        {
            get
            {
                return mIncludeSiteBindings;
            }
            set
            {
                mIncludeSiteBindings = value;
            }
        }


        /// <summary>
        /// If true, metafiles of the object are included in the result as well.
        /// </summary>
        public bool IncludeMetafiles
        {
            get
            {
                return mIncludeMetafiles;
            }
            set
            {
                mIncludeMetafiles = value;
            }
        }


        /// <summary>
        /// Determines maximal level of the relationship (parent-child). -1 means all levels, 0 means no child objects, 1 means first level of children, etc.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel;
            }
            set
            {
                mMaxRelativeLevel = value;
            }
        }


        /// <summary>
        /// ID of the parent object to which the cloned object should be assigned.
        /// </summary>
        public int ParentID
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the site to clone the object to. Only for site objects. If this value is 0, object will be cloned under the site of the original object 
        /// unless the object type has SupportsGlobalObjects = true. In that case, 0 means that an object is cloned as a global object.
        /// </summary>
        public int CloneToSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, CloneToSiteID is ignored and SiteID of the original object is kept.
        /// </summary>
        public bool KeepOriginalSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, cloned object will keep all the fields which use localization macros {$res.string$} without change. 
        /// If false, cloned object will replace the localization with its translation in the default culture.
        /// This setting does not influence DisplayName - this will be always replaced by its translation.
        /// </summary>
        public bool KeepFieldsTranslated
        {
            get;
            set;
        }

        #endregion


        #region "Delegates"

        /// <summary>
        /// Handles actions before/after inserting the clone. Use to further modify given object according to settings.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="cloneToBeInserted">BaseInfo object of the clone ready to be inserted</param>
        public delegate void OnCloneInsert(CloneSettings settings, BaseInfo cloneToBeInserted);

        #endregion


        #region "Callbacks"

        /// <summary>
        /// Handles actions before inserting the clone. Use to further modify given object according to settings.
        /// </summary>
        public OnCloneInsert BeforeCloneInsertCallback
        {
            get;
            set;
        }


        /// <summary>
        /// Handles actions right after inserting the clone object.
        /// </summary>
        public OnCloneInsert AfterCloneInsertCallback
        {
            get;
            set;
        }


        /// <summary>
        /// Handles actions after processing main object and all the child, binging, site bindings, ... etc. objects.
        /// </summary>
        public OnCloneInsert AfterCloneStructureInsertCallback
        {
            get;
            set;
        }

        #endregion
    }
}