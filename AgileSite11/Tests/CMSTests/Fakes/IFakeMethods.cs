using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Interface for attaching methods for faking data within automated tests
    /// </summary>
    public interface IFakeMethods
    {
        /// <summary>
        /// Fakes the data for the given info
        /// </summary>
        /// <param name="settings">Fake settings</param>
        IInfoFake<TInfo> Info<TInfo>(InfoFakeSettings settings = null)
            where TInfo : BaseInfo, IInfo, new();
        
        /// <summary>
        /// Fakes the data for the given info and provider
        /// </summary>
        IInfoProviderFake<TInfo, TProvider> InfoProvider<TInfo, TProvider>()
            where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
            where TProvider : AbstractInfoProvider<TInfo, TProvider>, new();


        /// <summary>
        /// Gets the class XML schema for the given type
        /// </summary>
        /// <param name="getFromParentType">If true, the columns from parent type are extracted</param>
        string GetClassXmlSchema<T>(bool getFromParentType = false);
    }
}
