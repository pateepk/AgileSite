namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides methods to retrieve additional data needed to perform "create" page permission check.
    /// </summary>
    internal interface ICreatePageAdditionalDataRetriever
    {
        /// <summary>
        /// Retrieves the additional data and encapsulates them into <see cref="CreatePagePermissionCheckAdditionalData"/>.
        /// </summary>
        CreatePagePermissionCheckAdditionalData GetData();
    }
}