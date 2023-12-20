using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to remove users from the kicked list.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveUserFromKickedList : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the user which will be removed from the kicked list.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AuthenticationHelper.RemoveUserFromKicked(int)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            AuthenticationHelper.RemoveUserFromKicked(UserId);
        }
    }
}
