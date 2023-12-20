using System.Collections.Generic;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Provides interface to retrieve allowed domains from/to which post messages can be received/sent on a client.
    /// </summary>
    internal interface IAllowedDomainsRetriever
    {
        /// <summary>
        /// Retrieves allowed domains from/to which post messages can be received/sent ona client.
        /// </summary>
        IEnumerable<string> Retrieve();
    }
}