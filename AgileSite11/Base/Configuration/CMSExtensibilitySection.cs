using System;
using System.Configuration;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Extensibility section for the web.config.
    /// </summary> 
    public class CMSExtensibilitySection : ConfigurationSection
    {
        #region "Variables"

        private static CMSExtensibilitySection mSection = null;
        private static bool mSectionLoaded = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Configuration section.
        /// </summary>
        internal static CMSExtensibilitySection Section
        {
            get
            {
                if (!mSectionLoaded)
                {
                    mSection = (CMSExtensibilitySection)ConfigurationManager.GetSection("cms.extensibility");
                    mSectionLoaded = true;
                }

                return mSection;
            }
        }


        /// <summary>
        /// Providers.
        /// </summary>
        [ConfigurationProperty("providers")]
        internal ExtensibilityCollection Providers
        {
            get
            {
                return (ExtensibilityCollection)this["providers"];
            }
            set
            {
                this["providers"] = value;
            }
        }


        /// <summary>
        /// Helpers.
        /// </summary>
        [ConfigurationProperty("helpers")]
        internal ExtensibilityCollection Helpers
        {
            get
            {
                return (ExtensibilityCollection)this["helpers"];
            }
            set
            {
                this["helpers"] = value;
            }
        }


        /// <summary>
        /// Managers.
        /// </summary>
        [ConfigurationProperty("managers")]
        internal ExtensibilityCollection Managers
        {
            get
            {
                return (ExtensibilityCollection)this["managers"];
            }
            set
            {
                this["managers"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the provider object.
        /// </summary>
        public static ProviderType LoadProvider<ProviderType>()
            where ProviderType : class, new()
        {
            if (Section != null)
            {
                string name = typeof(ProviderType).Name;

                // Search through the providers
                ExtensibilityCollection providers = Section.Providers;
                if (providers != null)
                {
                    foreach (ExtensibilityElement provider in providers)
                    {
                        if (provider.Name.EqualsCSafe(name, true))
                        {
                            // Provider match
                            return (ProviderType)ClassHelper.GetClass(provider.Assembly, provider.Type);
                        }
                    }
                }
            }

            // Create default provider
            return ObjectFactory<ProviderType>.New();
        }


        /// <summary>
        /// Loads the helper object.
        /// </summary>
        public static HelperType LoadHelper<HelperType>()
            where HelperType : class, new()
        {
            if (Section != null)
            {
                string name = typeof(HelperType).Name;

                // Search through the helpers
                ExtensibilityCollection helpers = Section.Helpers;
                if (helpers != null)
                {
                    foreach (ExtensibilityElement helper in helpers)
                    {
                        if (helper.Name.EqualsCSafe(name, true))
                        {
                            // helper match
                            return (HelperType)ClassHelper.GetClass(helper.Assembly, helper.Type);
                        }
                    }
                }
            }

            // Create default helper
            return ObjectFactory<HelperType>.New();
        }


        /// <summary>
        /// Changes the default manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public static void ChangeManagerType<OriginalType>(Type newType)
            where OriginalType : class, new()
        {
            ObjectFactory<OriginalType>.SetObjectTypeTo(newType, true);
        }


        /// <summary>
        /// Loads the manager object. Returns given default instance if manager not present in Section.Managers.
        /// </summary>
        public static ManagerType LoadManager<ManagerType>()
            where ManagerType : class, new()
        {
            if (Section != null)
            {
                string name = typeof(ManagerType).Name;

                // Search through the managers
                ExtensibilityCollection managers = Section.Managers;
                if (managers != null)
                {
                    foreach (ExtensibilityElement manager in managers)
                    {
                        if (manager.Name.EqualsCSafe(name, true))
                        {
                            // Helper match
                            return (ManagerType)ClassHelper.GetClass(manager.Assembly, manager.Type);
                        }
                    }
                }
            }

            // Create default provider
            return ObjectFactory<ManagerType>.New();
        }

        #endregion


        #region "Classes"

        /// <summary>
        /// Extensibility collection for classes registration.
        /// </summary> 
        [ConfigurationCollection(typeof(ExtensibilityElement))]
        internal class ExtensibilityCollection : ConfigurationElementCollection
        {
            /// <summary>
            /// Gets the new configuration element instance.
            /// </summary>
            protected override ConfigurationElement CreateNewElement()
            {
                return new ExtensibilityElement();
            }


            /// <summary>
            /// Gets the element key.
            /// </summary>
            /// <param name="element">Element to analyze</param>
            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ExtensibilityElement)element).Name;
            }
        }


        /// <summary>
        /// Extensibility element for class registration.
        /// </summary>
        internal class ExtensibilityElement : ConfigurationElement
        {
            /// <summary>
            /// Name.
            /// </summary>
            [ConfigurationProperty("name", IsRequired = true)]
            public String Name
            {
                get
                {
                    return (String)this["name"];
                }
                set
                {
                    this["name"] = value;
                }
            }


            /// <summary>
            /// Assembly.
            /// </summary>
            [ConfigurationProperty("assembly", IsRequired = true)]
            public String Assembly
            {
                get
                {
                    return (String)this["assembly"];
                }
                set
                {
                    this["assembly"] = value;
                }
            }


            /// <summary>
            /// Type.
            /// </summary>
            [ConfigurationProperty("type", IsRequired = true)]
            public String Type
            {
                get
                {
                    return (String)this["type"];
                }
                set
                {
                    this["type"] = value;
                }
            }
        }

        #endregion
    }
}