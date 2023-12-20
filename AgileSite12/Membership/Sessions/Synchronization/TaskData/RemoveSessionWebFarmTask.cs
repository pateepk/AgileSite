using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to delete sessions of users.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveSessionWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the user to be deleted.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SessionManager.RemoveUser(int, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            SessionManager.RemoveUser(UserId, false);
        }
    }
}
