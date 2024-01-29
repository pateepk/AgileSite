namespace CMS.Base
{
    /// <summary>
    /// Interface for the object condition
    /// </summary>
    public interface IObjectCondition
    {
        /// <summary>
        /// Returns true if the condition over the given object matches
        /// </summary>
        /// <param name="value">Value to match</param>
        bool Matches(object value);
    }
}