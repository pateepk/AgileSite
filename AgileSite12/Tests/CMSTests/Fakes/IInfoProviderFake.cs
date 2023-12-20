using System.Data;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Interface for automated tests fake of the info provider
    /// </summary>
    public interface IInfoProviderFake<TInfo, out TProvider> : IFake
        where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
        where TProvider : ITestableProvider
    {
        /// <summary>
        /// Faked provider object
        /// </summary>
        ITestableProvider ProviderObject
        {
            get;
        }

        /// <summary>
        /// Source data for the provider
        /// </summary>
        DataSet SourceData
        {
            get;
        }

        /// <summary>
        /// Includes the given data to the source items while keeping the existing data intact
        /// </summary>
        /// <param name="items">Items to include</param>
        void IncludeData(params TInfo[] items);
        
        /// <summary>
        /// Fakes the data for the given provider. Resets any previous data and only includes the given source items.
        /// </summary>
        IInfoProviderFake<TInfo, TProvider> WithData(params TInfo[] sourceItems);
        
        /// <summary>
        /// Ensures that the original data source name is used for the faked data
        /// </summary>
        IInfoProviderFake<TInfo, TProvider> WithOriginalSourceName();
    }
}
