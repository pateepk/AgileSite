using System.Web.Routing;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// CMS override of the default route class
    /// </summary>
    public class CMSRoute : Route
    {
        /// <summary>
        /// Route name
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a new route
        /// </summary>
        /// <param name="name">Route name</param>
        /// <param name="pattern">Route pattern</param>
        /// <param name="handler">Route handler</param>
        public CMSRoute(string name, string pattern, IRouteHandler handler)
            : base(pattern, handler)
        {
            Name = name;
        }
    }
}
