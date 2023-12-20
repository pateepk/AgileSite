using System;
using System.Collections.Generic;
using System.Reflection;

using CMS.Base;
using CMS.DataEngine;
using CMS.IO;
using CMS.LicenseProvider;

using NUnit.Framework;

namespace CMS.Tests
{
    using FakeDictionary = SafeDictionary<Type, IFake>;

    /// <summary>
    /// Base class for automated tests with data support
    /// </summary>
    public abstract class AutomatedTestsWithData : AutomatedTests
    {
        #region "Variables"

        private readonly List<IFake> mFakes = new List<IFake>();

        private readonly FakeDictionary mProviderFakes = new FakeDictionary();
        private readonly FakeDictionary mInfoFakes = new FakeDictionary();

        private IFakeMethods mFakeMethods;

        #endregion

        #region "Methods"

        /// <summary>
        /// Fixture setup
        /// </summary>
        [OneTimeSetUp]
        public void TestsWithDataFixtureSetup()
        {
            RunExtenderAction<AutomatedTestsWithData>(e => e.FixtureSetUp());
        }


        /// <summary>
        /// Test setup
        /// </summary>
        [SetUp]
        public void TestsWithDataSetup()
        {
            RunExtenderAction<AutomatedTestsWithData>(e => e.SetUp());
        }


        /// <summary>
        /// Cleans test base (Restore static context)
        /// </summary>
        [TearDown]
        public void TestsWithDataTearDown()
        {
            RunExtenderAction<AutomatedTestsWithData>(e => e.TearDown(), true);

            ResetAllFakes();
        }


        /// <summary>
        /// Fixture tear down
        /// </summary>
        [OneTimeTearDown]
        public void TestsWithDataFixtureTearDown()
        {
            RunExtenderAction<AutomatedTestsWithData>(e => e.FixtureTearDown(), true);
        }


        /// <summary>
        /// Inserts the license for localhost according to related config key
        /// </summary>
        public void EnsureLicense()
        {
            var licenseKey = TestsConfig.GetTestAppSetting(APP_KEY_LICENSE_KEY);

            if (!String.IsNullOrEmpty(licenseKey))
            {
                var license = new LicenseKeyInfo();
                license.LoadLicense(licenseKey, "");

                // Insert only if there exists no license on the same domain
                if (LicenseKeyInfoProvider.GetLicenseKeyInfo(license.Domain) == null)
                {
                    license.Insert();
                }
            }
        }

        #endregion


        #region "Fake methods"

        /// <summary>
        /// Resets all fakes registered within the system
        /// </summary>
        public virtual void ResetAllFakes()
        {
            foreach (var fake in mFakes)
            {
                fake.Reset();
            }

            mFakes.Clear();
            mProviderFakes.Clear();
            mInfoFakes.Clear();
        }


        /// <summary>
        /// Returns the entry which provides the fake methods from various modules
        /// </summary>
        public IFakeMethods Fake()
        {
            return mFakeMethods ?? (mFakeMethods = new FakeMethods(this));
        }


        /// <summary>
        /// Fakes the data for the given info
        /// </summary>
        /// <param name="settings">Fake settings</param>
        protected internal IInfoFake<TInfo> Fake<TInfo>(InfoFakeSettings settings = null)
            where TInfo : BaseInfo, IInfo, new()
        {
            var key = typeof(TInfo);

            var fake = (InfoFake<TInfo>)mInfoFakes[key];
            if (fake != null)
            {
                return fake;
            }

            fake = new InfoFake<TInfo>(settings);

            mFakes.Add(fake);
            mInfoFakes[key] = fake;

            return fake;
        }


        /// <summary>
        /// Fakes the data for the given info and provider
        /// </summary>
        /// <param name="providerObject">Provider object</param>
        /// <param name="fakeInfo">If true, the info object structure is faked</param>
        protected internal IInfoProviderFake<TInfo, TProvider> Fake<TInfo, TProvider>(TProvider providerObject = null, bool fakeInfo = true)
            where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
            where TProvider : class,  ITestableProvider, new()
        {
            var key = typeof(TProvider);

            var fake = (InfoProviderFake<TInfo, TProvider>)mProviderFakes[key];
            if ((fake != null) &&
                ((fake.ProviderObject == providerObject) ||
                 ((providerObject == null) && fake.ProviderIsDefault))
                )
            {
                return fake;
            }

            fake = new InfoProviderFake<TInfo, TProvider>(providerObject, fakeInfo)
                .HandleWriteOperations();

            mFakes.Add(fake);
            mProviderFakes[key] = fake;

            return fake;
        }


        /// <summary>
        /// Fakes the dynamic data provider.
        /// </summary>
        /// <remarks>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </remarks>
        /// <param name="providerObject">Provider object.</param>
        /// <param name="objectType">Object type used for registration to <see cref="ObjectTypeManager"/>.</param>
        /// <param name="fakeInfo">If true, the info object structure is faked.</param>
        /// <exception cref="ArgumentNullException">Thrown when parameters <paramref name="providerObject"/> or <paramref name="objectType"/> are null.</exception>
        protected IInfoProviderFake<TInfo, TProvider> FakeDynamicObjectType<TInfo, TProvider>(TProvider providerObject, string objectType, bool fakeInfo = true)
            where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
            where TProvider : class, ITestableProvider, new()
        {
            if (providerObject == null)
            {
                throw new ArgumentNullException(nameof(providerObject));
            }

            if (string.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            DisconnectProviderLoading();

            var objectTypeInfo = new ObjectTypeInfo(providerObject.GetType(), objectType, null, null, null, null, null, null, null, null, null, null);
            ObjectTypeManager.RegisterObjectType(objectType, objectTypeInfo, typeof(TInfo));

            return Fake<TInfo, TProvider>(providerObject, fakeInfo);
        }


        private static void DisconnectProviderLoading()
        {
            var eventField = GetProviderLoadingEventField();
            eventField.SetValue(null, null);
        }


        private static System.Reflection.FieldInfo GetProviderLoadingEventField()
        {
            var type = typeof(InfoProviderLoader);
            System.Reflection.FieldInfo field = null;

            while (type != null)
            {
                field = type.GetField("LoadProvider", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                {
                    break;
                }
                type = type.BaseType;
            }

            return field;
        }


        /// <summary>
        /// Fakes the data for the given info
        /// </summary>
        /// <param name="idFrom">Starting object ID</param>
        /// <param name="idTo">Ending object ID</param>
        public TInfo[] FakeObjects<TInfo>(int idFrom, int idTo)
            where TInfo : BaseInfo, IInfo, new()
        {
            var result = new TInfo[idTo - idFrom + 1];

            int i = 0;

            for (int id = idFrom; id <= idTo; id++)
            {
                result[i++] = FakeObject<TInfo>(id);
            }

            return result;
        }


        /// <summary>
        /// Fakes the data for the given info
        /// </summary>
        /// <param name="id">ID of the fake object</param>
        /// <param name="siteId">Optional site ID of the object</param>
        /// <param name="finalizer">Optional action allowing to seed additional fields.</param>
        /// <remarks>
        /// When <paramref name="finalizer"/> is called, provided <typeparamref name="TInfo"/> object has following properties already set:
        /// <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectID"/>,
        /// <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectCodeName"/>,
        /// <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectGUID"/>,
        /// <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectDisplayName"/>.
        /// </remarks>
        public TInfo FakeObject<TInfo>(int id, int siteId = 0, Action<TInfo> finalizer = null)
            where TInfo : BaseInfo, IInfo, new()
        {
            var obj = new TInfo();
            var genObj = obj.Generalized;

            genObj.ObjectID = id;

            var name = typeof(TInfo).Name;

            genObj.ObjectDisplayName = "Test" + name;
            genObj.ObjectCodeName = "Test" + name + id;
            genObj.ObjectGUID = GetTestGuid(id);
            if (siteId > 0)
            {
                genObj.ObjectSiteID = siteId;
            }

            if (finalizer != null)
            {
                finalizer(obj);
            }

            return obj;
        }


        /// <summary>
        /// Gets a predictable test GUID for the given ID
        /// </summary>
        /// <param name="id">Source ID</param>
        protected static Guid GetTestGuid(int id)
        {
            return new Guid("00000000-0000-0000-0000-" + id.ToString().PadLeft(12, '0'));
        }


        /// <summary>
        /// Gets the class XML schema for the given type
        /// </summary>
        /// <param name="getFromParentType">If true, the columns from parent type are extracted</param>
        public string GetClassXmlSchema<T>(bool getFromParentType = false)
        {
            var settings = new InfoFakeSettings(typeof(T))
            {
                IncludeInheritedFields = getFromParentType
            };

            return new FakeClassStructureInfo(settings).GetXmlSchema();
        }


        /// <summary>
        /// Fakes the license for localhost according to related config key
        /// </summary>
        protected void FakeLicense()
        {
            Fake<LicenseKeyInfo>();
            LicenseKeyInfo license = new LicenseKeyInfo();
            license.LoadLicense(TestsConfig.GetTestAppSetting(APP_KEY_LICENSE_KEY), "localhost");
            Fake<LicenseKeyInfo, LicenseKeyInfoProvider>().WithData(license);
        }


        /// <summary>
        /// Returns virtual file system provider
        /// </summary>
        protected AbstractStorageProvider GetVirtualStorageProvider(string storageName = "TestVirtualFileSystem", string assemblyName = "CMS.FakeFileSystemStorage")
        {
            return new StorageProvider(storageName, assemblyName);
        }

        #endregion
    }
}
