using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to add users to the kicked list.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class AddUserToKickedListWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the user which will be added to the kicked list.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AuthenticationHelper.AddUserToKicked(int)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            AuthenticationHelper.AddUserToKicked(UserId);
        }
    }
}
