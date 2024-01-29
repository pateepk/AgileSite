
namespace CMS.FormEngine
{
    /// <summary>
    /// Indicates which group of users can manage form.
    /// </summary>
    public enum FormAccessEnum
    {
        /// <summary>
        /// All bizform users can access bizform management.
        /// </summary>
        AllBizFormUsers = 0,

        /// <summary>
        /// Only authorized roles can acces bizform management.
        /// </summary>
        OnlyAuthorizedRoles = 1
    }
}