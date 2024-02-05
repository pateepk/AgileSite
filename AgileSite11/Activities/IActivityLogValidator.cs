using System.Web;

namespace CMS.Activities
{
    /// <summary>
    /// Provides method for validating the <see cref="IActivityInfo"/> and determining, whether the <see cref="IActivityInfo"/>
    /// is valid to be logged or not. Validator can be register as global validator for all activities being logged and it is used in 
    /// <see cref="IActivityLogService.Log(IActivityInitializer, HttpRequestBase, bool)"/> or <see cref="IActivityLogService.LogWithoutModifiersAndFilters"/>.
    /// </summary>
    public interface IActivityLogValidator
    {
        /// <summary>
        /// Determines whether the given <paramref name="activity"/> is valid or not, e.g. checks consistency of
        /// activity properties.
        /// </summary>
        /// <param name="activity">Activity to be validated</param>
        /// <returns>True if validation check was passed; otherwise, false</returns>
        bool IsValid(IActivityInfo activity);
    }
}