using System.Web.UI;

namespace CMS.Membership.Web.UI
{
    /// <summary>
    /// Interface for objects providing third party logout scripts
    /// </summary>
    public interface ICustomSignOutScriptProvider
    {
        /// <summary>
        /// Builds and returns a custom logout script which is obligated to call the given callback upon finish.
        /// </summary>
        /// <param name="finishCallBack">Callback method to be called when this custom script finishes</param>
        /// <param name="page">Page to which helper scripts can be registered</param>
        /// <returns>A custom logout script which is obligated to call the given callback upon finish
        /// or null if no script is required by this provider</returns>
        string GetSignOutScript(string finishCallBack, Page page);
    }
}
