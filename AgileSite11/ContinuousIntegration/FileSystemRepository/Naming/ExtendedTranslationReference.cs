using System.Collections.Generic;

using CMS.DataEngine;
using CMS.DataEngine.Serialization;

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Translation reference carrying data from additional fields.
    /// </summary>
    /// <remarks>
    /// This class is not to be instantiated in your custom code. It supports the framework
    /// API for customizations.
    /// </remarks>
    public sealed class ExtendedTranslationReference : TranslationReference
    {
        private ObjectTypeInfo mTypeInfo;
        private ExtendedTranslationReference mExtendedSite;
        private ExtendedTranslationReference mExtendedParent;
        private ExtendedTranslationReference mExtendedGroup;
        private IDictionary<string, object> mAdditionalFields = new Dictionary<string, object>();


        /// <summary>
        /// Type info of object represented by this translation reference.
        /// </summary>
        public ObjectTypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo ?? (mTypeInfo = ObjectTypeManager.GetTypeInfo(ObjectType));
            }
            internal set
            {
                mTypeInfo = value;
            }
        }


        /// <summary>
        /// Dictionary containing pairs [Field name] -> [Value].
        /// </summary>
        public IDictionary<string, object> AdditionalFields
        {
            get
            {
                return mAdditionalFields ?? (mAdditionalFields = new Dictionary<string, object>());
            }
            set
            {
                mAdditionalFields = value;
            }
        }


        /// <summary>
        /// Extended translation reference of object's site.
        /// </summary>
        internal ExtendedTranslationReference ExtendedSite
        {
            get
            {
                return mExtendedSite;
            }
            set
            {
                Site = mExtendedSite = value;
            }
        }


        /// <summary>
        /// Extended translation reference of object's parent.
        /// </summary>
        internal ExtendedTranslationReference ExtendedParent
        {
            get
            {
                return mExtendedParent;
            }
            set
            {
                Parent = mExtendedParent = value;
            }
        }


        /// <summary>
        /// Extended translation reference of object's group.
        /// </summary>
        internal ExtendedTranslationReference ExtendedGroup
        {
            get
            {
                return mExtendedGroup;
            }
            set
            {
                Group = mExtendedGroup = value;
            }
        }

        
        /// <summary>
        /// Extended translation reference of object's category.
        /// </summary>
        internal ExtendedTranslationReference ExtendedCategory
        {
            get;
            set;
        }


        /// <summary>
        /// Extended translation references of object's filter dependencies specified by <see cref="ContinuousIntegrationSettings.FilterDependencies"/>.
        /// </summary>
        internal ICollection<ExtendedTranslationReference> ExtendedFilterDependencies
        {
            get;
            set;
        }


        /// <summary>
        /// Translation helper used to initialize this translation reference.
        /// </summary>
        internal ContinuousIntegrationTranslationHelper TranslationHelper
        {
            get;
            set;
        }
        

        /// <summary>
        /// Initializer preventing instantiation in custom code.
        /// </summary>
        internal ExtendedTranslationReference()
        {

        }
    }
}
