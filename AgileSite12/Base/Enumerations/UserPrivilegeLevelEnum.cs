namespace CMS.Base
{
    /// <summary>
    /// User privilege level enum
    /// </summary>
    public enum UserPrivilegeLevelEnum
    {
        /// <summary>
        /// User has no privilege level
        /// </summary>
        None,


        /// <summary>
        /// User is able to use administration interface
        /// </summary>
        Editor,


        /// <summary>
        /// User can use all applications except the global applications and functionality
        /// </summary>
        Admin,


        /// <summary>
        /// User can use all applications and functionality without any exceptions
        /// </summary>
        GlobalAdmin

    }
}
