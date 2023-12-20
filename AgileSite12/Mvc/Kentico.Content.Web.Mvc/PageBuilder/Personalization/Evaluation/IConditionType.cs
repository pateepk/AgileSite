namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Provides interface for personalization condition type.
    /// </summary>
    public interface IConditionType 
    {
        /// <summary>
        /// Evaluate condition type.
        /// </summary>
        /// <returns>Returns <c>true</c> if implemented condition is met.</returns>
        bool Evaluate();
    }
}
