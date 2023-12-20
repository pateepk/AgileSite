using System.Text;
using System.Web.Mvc;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Base class for controllers of condition types.
    /// </summary>
    public abstract class ConditionTypeController<TConditionType> : Controller
        where TConditionType : class, IConditionType, new()
    {
        private IConditionTypeParametersRetriever<TConditionType> mParametersRetriever;


        private IConditionTypeParametersRetriever<TConditionType> ParametersRetriever
        {
            get
            {
                if (mParametersRetriever == null)
                {
                    var securityChecker = new PageSecurityChecker(new VirtualContextPageRetriever());
                    var postDataRetriever = new PageBuilderPostDataRetriever<PageBuilderPostData>(HttpContext, securityChecker);
                    var serializer = new ConditionTypeParametersSerializer();
                    mParametersRetriever = new ConditionTypeParametersRetriever<TConditionType>(postDataRetriever, serializer);
                }

                return mParametersRetriever;
            }
        }


        /// <summary>
        /// Creates an instance of the <see cref="ConditionTypeController{TConditionType}"/> class.
        /// </summary>
        protected ConditionTypeController()
        {
            
        }


        /// <summary>
        /// Creates an instance of the <see cref="ConditionTypeController{TConditionType}"/> class.
        /// </summary>
        /// <param name="parametersRetriever">Retriever for condition type parameters.</param>
        internal ConditionTypeController(IConditionTypeParametersRetriever<TConditionType> parametersRetriever)
        {
            mParametersRetriever = parametersRetriever;
        }


        /// <summary>
        /// Creates a <see cref="JsonCamelCaseResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) camel case format using the content type, content encoding, and the JSON request behavior.
        /// </summary>
        /// <param name="data">The JavaScript object graph to serialize.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <param name="behavior">The JSON request behavior.</param>
        /// <returns>The result object that serializes the specified object to JSON format in camel case.</returns>
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonCamelCaseResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }


        /// <summary>
        /// Gets parameters of a condition type based on <typeparamref name="TConditionType"/> type.
        /// </summary>
        protected internal TConditionType GetParameters()
        {
            return ParametersRetriever.Retrieve();
        }
    }
}