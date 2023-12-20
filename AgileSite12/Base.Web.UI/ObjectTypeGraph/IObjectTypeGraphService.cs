using CMS;
using CMS.Base.Web.UI;

[assembly: RegisterImplementation(typeof(IObjectTypeGraphService), typeof(DefaultObjectTypeGraphService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines methods which need to be implemented by services that load data for vis.js object type graphs.
    /// </summary>
    internal interface IObjectTypeGraphService
    {
        /// <summary>
        /// Loads data for a vis.js object type graph. Must be in a format suitable for JSON serialization.
        /// </summary>
        /// <param name="objectType">The object type for which the graph data is loaded.</param>
        /// <param name="scope">Indicates which types of related object types are loaded for standard objects.</param>
        GraphData LoadGraphData(string objectType, ObjectTypeGraphScopeEnum scope);
    }
}