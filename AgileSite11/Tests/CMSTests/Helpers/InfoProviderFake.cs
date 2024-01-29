using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Fakes the given info and provider
    /// </summary>
    internal class InfoProviderFake<TInfo, TProvider> : IInfoProviderFake<TInfo, TProvider> 
        where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
        where TProvider : class, ITestableProvider, new()
    {
        #region "Variables"

        /// <summary>
        /// Faked provider
        /// </summary>
        private readonly List<ITestableProvider> mProviders = new List<ITestableProvider>();

        /// <summary>
        /// Faked info
        /// </summary>
        private readonly InfoFake<TInfo> mInfoFake;

        /// <summary>
        /// Source items
        /// </summary>
        private List<TInfo> mSourceItems;

        /// <summary>
        /// Source data
        /// </summary>
        private DataSet mSourceData;

        /// <summary>
        /// If true, the data of the provider was faked
        /// </summary>
        private bool mDataFaked;

        private MemoryDataQuerySource mDataQuerySource;
        private ITestableProvider mProviderObject;
        private ITestableProvider mOriginalProviderObject;

        private bool mProviderIsDefault = true;
        
        #endregion

        
        #region "Properties"

        /// <summary>
        /// Returns true if the used provider is a default provider
        /// </summary>
        internal bool ProviderIsDefault
        {
            get
            {
                return mProviderIsDefault;
            }
        }

        
        /// <summary>
        /// Gets the data query source
        /// </summary>
        private MemoryDataQuerySource DataQuerySource
        {
            get
            {
                return mDataQuerySource ?? (mDataQuerySource = GetDataQuerySource());
            }
        }

        
        /// <summary>
        /// Source data
        /// </summary>
        public DataSet SourceData
        {
            get
            {
                return mSourceData ?? (mSourceData = new InfoFakeDataSet(mSourceItems.ToArray()));
            }
        }


        /// <summary>
        /// Provider object
        /// </summary>
        public ITestableProvider ProviderObject
        {
            get
            {
                return mProviderObject ?? EnsureDefaultProvider();
            }
            protected set
            {
                mProviderObject = value;
                mProviderIsDefault = (value != null);
            }
        }


        /// <summary>
        /// Ensures the default provider object
        /// </summary>
        private ITestableProvider EnsureDefaultProvider()
        {
            if (mProviderObject == null)
            {
                mProviderIsDefault = true;

                mProviderObject = new TProvider();
            }

            return mProviderObject;
        }


        /// <summary>
        /// Original provider object
        /// </summary>
        private ITestableProvider OriginalProviderObject
        {
            get
            {
                return mOriginalProviderObject ?? (mOriginalProviderObject = ProviderObject.GetCurrentProvider());
            }
            set
            {
                mOriginalProviderObject = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="providerObject">Provider object</param>
        /// <param name="fakeInfo">If true, the info object structure is faked</param>
        public InfoProviderFake(TProvider providerObject = null, bool fakeInfo = true)
        {
            if (fakeInfo)
            {
                mInfoFake = new InfoFake<TInfo>();
            }

            ProviderObject = providerObject;

            providerObject?.SetAsDefaultProvider();
        }


        /// <summary>
        /// Includes the given data to the source items while keeping the existing data intact
        /// </summary>
        /// <param name="items">Source items to include</param>
        public void IncludeData(params TInfo[] items)
        {
            if (mSourceItems == null)
            {
                mSourceItems = new List<TInfo>();
            }

            // Fake provider
            var typeInfo = GetTypeInfo();
             
            PrepareData(items, typeInfo);

            mSourceItems.AddRange(items);

            // Reset source dataset to ensure that new data are loaded from source items
            mSourceData = null;

            // Fake the provider source
            FakeProviderSource();
        }


        private void RemoveData(params TInfo[] items)
        {
            if (mSourceItems != null)
            {
                foreach (var item in items)
                {
                    mSourceItems.RemoveAll(i => i.Generalized.ObjectID == item.Generalized.ObjectID);
                }
            }
        }


        /// <summary>
        /// Fakes the data for the given provider. Resets any previous data and only includes the given source items.
        /// </summary>
        public IInfoProviderFake<TInfo, TProvider> WithData(params TInfo[] sourceItems)
        {
            // Reset the source items
            mSourceItems = null;

            // Include the data
            IncludeData(sourceItems);

            return this;
        }


        /// <summary>
        /// Ensures that the original data source name is used for the faked data.
        /// </summary>
        /// <remarks>
        /// Call it before adding info objects via method <see cref="WithData"/> or via method <see cref="IncludeData"/>.
        /// Otherwise there could be data source name mismatch due to usage of random guids.
        /// </remarks>
        public IInfoProviderFake<TInfo, TProvider> WithOriginalSourceName()
        {
            var query = ((IInternalProvider)OriginalProviderObject).GetGeneralObjectQuery(true);
            DataQuerySource.DataSourceName = query.DataSourceName;

            return this;
        }


        /// <summary>
        /// Ensures that write operations (insert, update, etc.) modify the faked data.
        /// </summary>
        internal InfoProviderFake<TInfo, TProvider> HandleWriteOperations()
        {
            var typeInfoEvents = GetTypeInfo().Events;

            typeInfoEvents.Insert.Before += (sender, e) =>
            {
                var infoObject = (TInfo)e.Object;

                IncludeData(infoObject);
            };

            typeInfoEvents.Update.Before += (sender, e) =>
            {
                var infoObject = (TInfo)e.Object;

                RemoveData(infoObject);
                IncludeData(infoObject);
            };

            typeInfoEvents.Delete.Before += (sender, e) =>
            {
                var infoObject = (TInfo)e.Object;

                RemoveData(infoObject);
            };

            return this;
        }


        /// <summary>
        /// Gets the data query source
        /// </summary>
        private MemoryDataQuerySource GetDataQuerySource()
        {
            return new MemoryDataQuerySource(() => SourceData);
        }
        

        /// <summary>
        /// Fakes the source of the provider to the data of this fake
        /// </summary>
        private void FakeProviderSource()
        {
            if (!mDataFaked)
            {
                // Fake provider
                var provider = ProviderObject;

                var typeInfo = GetTypeInfo();

                FakeProvider(provider);

                // Fake nested infos if exist
                if (typeInfo.NestedInfoTypes != null)
                {
                    foreach (var nested in typeInfo.NestedInfoTypes)
                    {
                        var nestedTypeInfo = ObjectTypeManager.GetTypeInfo(nested);

                        provider = (ITestableProvider)nestedTypeInfo.ProviderObject;

                        FakeProvider(provider);
                    }
                }

                mDataFaked = true;
            }
        }


        /// <summary>
        /// Prepares the source items to be able to be used in fake data
        /// </summary>
        /// <param name="sourceItems">Source items</param>
        /// <param name="typeInfo">Type info</param>
        private void PrepareData(TInfo[] sourceItems, ObjectTypeInfo typeInfo)
        {
            // Connect nested data through parent
            if (typeInfo.NestedInfoTypes != null)
            {
                foreach (var nested in typeInfo.NestedInfoTypes)
                {
                    var nestedTypeInfo = ObjectTypeManager.GetTypeInfo(nested);

                    var parentIdColumn = nestedTypeInfo.ParentIDColumn;
                    if (parentIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Distribute object ID to parent ID column of source item
                        foreach (var sourceItem in sourceItems)
                        {
                            sourceItem.SetValue(parentIdColumn, sourceItem.Generalized.ObjectID);
                        }
                    }
                }
            }
            
            EnsureIds(sourceItems, typeInfo.IDColumn);
        }


        private void EnsureIds(IEnumerable<TInfo> sourceItems, string idColumn)
        {
            var maxId = mSourceItems.Select(i => i.GetValue(idColumn))
                                    .OfType<int>()
                                    .DefaultIfEmpty(0)
                                    .Max();

            // Set the ID to objects without one
            foreach (var sourceItem in sourceItems.Where(item => item.GetValue(idColumn) == null))
            {
                sourceItem.SetValue(idColumn, ++maxId);
            }
        }


        /// <summary>
        /// Fakes the given provider with the given source
        /// </summary>
        private void FakeProvider(ITestableProvider provider)
        {
            OriginalProviderObject = provider.GetCurrentProvider();

            provider.DataSource = DataQuerySource;
            provider.SetAsDefaultProvider();

            TransactionScopeFactory.RegisterTransaction(provider.GetType(), () => new MemoryTransactionScope());
            
            mProviders.Add(provider);
        }


        /// <summary>
        /// Resets the fake
        /// </summary>
        public virtual void Reset()
        {
            foreach (var provider in mProviders)
            {
                provider.ResetToDefault();
            }

            mProviders.Clear();
            mInfoFake?.Reset();
        }

        private ObjectTypeInfo GetTypeInfo()
        { 
            return ObjectTypeManager.GetTypeInfos(typeof(TInfo)).First();
        }


        #endregion


        #region "InfoFakeDataSet"

        private class InfoFakeDataSet : InfoDataSet<TInfo>
        {
            // Empty constructor
            public InfoFakeDataSet()
            {
            }

            // Special case. Added computed column used TranslationHelper to data set.
            public InfoFakeDataSet(params TInfo[] items)
            {
                CreateEmptyDataSet();

                Tables[0].Columns.Add(TranslationHelper.QUERY_OBJECT_TYPE_COLUMN, typeof (string));
                var objectTypeColumnIndex = Tables[0].Columns.IndexOf(TranslationHelper.QUERY_OBJECT_TYPE_COLUMN);

                AddItems(items);

                for (int i = 0; i < items.Length; i++)
                {
                    Tables[0].Rows[i][objectTypeColumnIndex] = items[i].TypeInfo.ObjectType;
                }
            }
        }

        #endregion
    }
}