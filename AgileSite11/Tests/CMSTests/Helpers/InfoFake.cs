using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Fakes the given info
    /// </summary>
    internal class InfoFake<TInfo> : InfoFake, IInfoFake<TInfo> 
        where TInfo : BaseInfo, IInfo, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Fake settings</param>
        public InfoFake(InfoFakeSettings settings = null)
            : base(EnsureSettingsWithType(settings))
        {
        }


        /// <summary>
        /// Applies type to settings
        /// </summary>
        /// <param name="settings">Fake settings</param>
        private static InfoFakeSettings EnsureSettingsWithType(InfoFakeSettings settings)
        {
            if (settings == null)
            {
                return new InfoFakeSettings(typeof(TInfo));
            }

            settings.Type = typeof (TInfo);

            return settings;
        }
    }


    /// <summary>
    /// Fakes the given info
    /// </summary>
    internal class InfoFake : IFake
    {
        /// <summary>
        /// Nested fakes
        /// </summary>
        private List<InfoFake> mNestedFakes;


        /// <summary>
        /// Info type
        /// </summary>
        public Type InfoType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Faked class structure info
        /// </summary>
        public ClassStructureInfo ClassStructureInfo
        {
            get
            {
                return FakeClassStructureInfo;
            }
        }


        /// <summary>
        /// Faked class structure info
        /// </summary>
        private FakeClassStructureInfo FakeClassStructureInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Fake settings</param>
        public InfoFake(InfoFakeSettings settings)
        {
            InfoType = settings.Type;
            FakeClassStructureInfo = new FakeClassStructureInfo(settings);
            
            FakeInfo(settings);
        }


        /// <summary>
        /// Fakes the info object
        /// </summary>
        /// <param name="settings">Fake settings</param>
        private void FakeInfo(InfoFakeSettings settings)
        {
            // Get the type infos for the given type
            var infos = 
                String.IsNullOrEmpty(settings.ObjectType) ? 
                ObjectTypeManager.GetTypeInfos(settings.Type) : 
                new [] { ObjectTypeManager.GetTypeInfo(settings.ObjectType) };

            var faked = false;

            // Fake all type infos
            foreach (var typeInfo in infos)
            {
                typeInfo.ClassStructureInfo = FakeClassStructureInfo;

                // Ensure ID column information if not explicitly given
                if (FakeClassStructureInfo.IDColumn == null)
                {
                    FakeClassStructureInfo.IDColumn = typeInfo.IDColumn;
                }

                faked = true;

                // Fake nested infos if exist
                if (typeInfo.NestedInfoTypes != null)
                {
                    mNestedFakes = new List<InfoFake>();

                    // Fake nested objects based on their info types
                    foreach (var nested in typeInfo.NestedInfoTypes)
                    {
                        var nestedInfo = ModuleManager.GetObject(nested);
                        var nestedFake = new InfoFake(new InfoFakeSettings(nestedInfo.GetType()));

                        mNestedFakes.Add(nestedFake);

                        // Include nested fake to the faked class structure info
                        FakeClassStructureInfo.AddNestedClass(nestedFake.FakeClassStructureInfo);
                    }
                }
            }

            if (!faked)
            {
                throw new Exception("[CMSTest.Fake]: Cannot fake type '" + settings.Type.FullName + "', no type info was found for this type.");
            }
        }


        /// <summary>
        /// Resets the info fake
        /// </summary>
        /// <param name="type">Info type to fake</param>
        private void ResetInfo(Type type)
        {
            // Get the type infos for the given type
            var infos = ObjectTypeManager.GetTypeInfos(type);

            // Reset all type infos
            foreach (var typeInfo in infos)
            {
                // Reset nested infos first if exist
                if (mNestedFakes != null)
                {
                    foreach (var nested in mNestedFakes)
                    {
                        nested.Reset();
                    }
                }

                mNestedFakes = null;

                // Reset current info
                typeInfo.ClassStructureInfo = null;
            }
        }


        /// <summary>
        /// Resets the fake
        /// </summary>
        public virtual void Reset()
        {
            ResetInfo(InfoType);
        }
    }
}