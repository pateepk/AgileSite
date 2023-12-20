namespace CMS.DataEngine
{
    /// <summary>
    /// Marks a class for which permissions are to be checked during macro resolution.
    /// </summary>
    public interface IMacroSecurityCheckPermissions
    {
        /// <summary>
        /// Gets an object for which to perform the permissions check. This can be the class' instance itself or an object it encapsulates.
        /// </summary>
        object GetObjectToCheck();
    }
}
