using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to delete sessions of authenticated users.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveAuthenticatedUserWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site from which user session will be deleted.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the authenticated user to be deleted.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SessionManager.RemoveAuthenticatedUser(string, int, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            SessionManager.RemoveAuthenticatedUser(SiteName, UserId, false);
        }
    }
}
