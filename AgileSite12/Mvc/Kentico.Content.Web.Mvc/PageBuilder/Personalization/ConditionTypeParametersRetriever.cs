using System;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Retrieves condition type parameters for a condition type controller from request body.
    /// </summary>
    /// <typeparam name="TConditionType">Type of the condition type parameters.</typeparam>
    internal class ConditionTypeParametersRetriever<TConditionType> : IConditionTypeParametersRetriever<TConditionType>
        where TConditionType : class, IConditionType, new()
    {
        private readonly IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever;
        private readonly IConditionTypeParametersSerializer serializer;


        /// <summary>
        /// Creates an instance of <see cref="ConditionTypeParametersRetriever{TConditionType}"/> class.
        /// </summary>
        /// <param name="postDataRetriever">Retriever for data from POST data.</param>
        /// <param name="serializer">Serializer of the condition type parameters.</param>
        /// <exception cref="ArgumentNullException"><paramref name="postDataRetriever"/> or <paramref name="serializer"/> is <c>null</c>.</exception>
        public ConditionTypeParametersRetriever(IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever, IConditionTypeParametersSerializer serializer)
        {
            this.postDataRetriever = postDataRetriever ?? throw new ArgumentNullException(nameof(postDataRetriever));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }


        /// <summary>
        /// Retrieves condition type parameters. 
        /// </summary>
        public TConditionType Retrieve()
        {
            var conditionType = GetPropertiesFromPostData();

            return conditionType ?? throw new InvalidOperationException($"Unable to retrieve the condition type parameters of type {typeof(TConditionType).FullName}.");
        }


        private TConditionType GetPropertiesFromPostData()
        {
            var data = postDataRetriever.Retrieve();
            if (data == null)
            {
                return null;
            }

            return serializer.Deserialize<TConditionType>(data.Data);
        }
    }
}
