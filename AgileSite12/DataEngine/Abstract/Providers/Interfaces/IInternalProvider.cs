
namespace CMS.DataEngine
{
    /// <summary>
    /// Gathers all provider properties and methods which needs to be accessed internally.
    /// </summary>
    internal interface IInternalProvider
    {
        /// <summary>
        /// Indicates if the provider instance is up-to-date and can be used to manage object instances.
        /// </summary>
        bool IsValid
        {
            get;
        }


        /// <summary>
        /// Sets this provider as invalid.
        /// </summary>
        void Invalidate();


        /// <summary>
        /// Gets an object query from the provider.
        /// </summary>
        /// <param name="checkLicense">If true, the call checks the license</param>
        IObjectQuery GetGeneralObjectQuery(bool checkLicense);
    }
}