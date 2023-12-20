namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Provides an interface for retrieving condition type parameters.
    /// </summary>
    /// <typeparam name="TConditionType">Type of condition type parameters.</typeparam>
    internal interface IConditionTypeParametersRetriever<out TConditionType>
        where TConditionType : class, IConditionType, new()
    {
        /// <summary>
        /// Retrieves condition type parameters.
        /// </summary>
        TConditionType Retrieve();
    }
}
