using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Class providing methods for faking the data
    /// </summary>
    public class FakeMethods : IFakeMethods
    {
        /// <summary>
        /// Parent tests
        /// </summary>
        public AutomatedTestsWithData ParentTests
        {
            get;
            set;
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentTests">Parent unit tests</param>
        public FakeMethods(AutomatedTestsWithData parentTests)
        {
            ParentTests = parentTests;
        }


        /// <summary>
        /// Fakes the data for the given info
        /// </summary>
        /// <param name="settings">Fake settings</param>
        public IInfoFake<TInfo> Info<TInfo>(InfoFakeSettings settings = null)
            where TInfo : BaseInfo, IInfo, new()
        {
            return ParentTests.Fake<TInfo>(settings);
        }



        /// <summary>
        /// Fakes the data for the given info and provider
        /// </summary>
        public IInfoProviderFake<TInfo, TProvider> InfoProvider<TInfo, TProvider>()
            where TInfo : AbstractInfoBase<TInfo>, IInfo, new()
            where TProvider : AbstractInfoProvider<TInfo, TProvider>, new()
        {
            return ParentTests.Fake<TInfo, TProvider>();
        }


        /// <summary>
        /// Gets the class XML schema for the given type
        /// </summary>
        /// <param name="getFromParentType">If true, the columns from parent type are extracted</param>
        public string GetClassXmlSchema<T>(bool getFromParentType = false)
        {
            return ParentTests.GetClassXmlSchema<T>(getFromParentType);
        }


        /// <summary>
        /// Resets all fakes
        /// </summary>
        public void ResetAllFakes()
        {
            ParentTests.ResetAllFakes();
        }
    }
}
