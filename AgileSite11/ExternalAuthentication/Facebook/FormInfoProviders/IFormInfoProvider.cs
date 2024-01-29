using CMS.DataEngine;
using CMS.FormEngine;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Provides form info objects suitable for mapping.
    /// </summary>
    public interface IFormInfoProvider
    {

        #region "Methods"

        /// <summary>
        /// Creates a new instance of the form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="info">The CMS object to create the form info for.</param>
        /// <returns>A new instance of the form info suitable for mapping, if applicable; otherwise, null.</returns>
        FormInfo GetFormInfo(BaseInfo info);

        /// <summary>
        /// Creates a new instance of the form info suitable for mapping, and returns it.
        /// </summary>
        /// <param name="typeInfo">The CMS object type info to create the form info for.</param>
        /// <returns>A new instance of the form info suitable for mapping, if applicable; otherwise, null.</returns>
        FormInfo GetFormInfo(ObjectTypeInfo typeInfo);

        #endregion

    }

}