using System;
using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Class containing specific category attributes
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unit test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class UnitAttribute : CategoryAttribute { }


        /// <summary>
        /// Integration test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class IntegrationAttribute : CategoryAttribute { }
        

        /// <summary>
        /// Isolated integration test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class IsolatedIntegrationAttribute : CategoryAttribute { }


        /// <summary>
        /// UI test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class UITestsAttribute : CategoryAttribute { }


        /// <summary>
        /// Long-running test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class LongRunningAttribute : CategoryAttribute { }


        /// <summary>
        /// Web app instance test category
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
        public class WebAppInstanceRequiredAttribute : CategoryAttribute { }
        
    }
}
