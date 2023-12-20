using System;
using System.Web.Mvc;

using Kentico.Builder.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// JSON result with camel case formatting of object properties.
    /// </summary>
    internal class JsonCamelCaseResult : JsonResult
    {
        private const string APPLICATION_JSON = "application/json";


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && 
                String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("GET request not allowed");
            }

            if (MaxJsonLength.HasValue)
            {
                throw new NotSupportedException();
            }

            if (RecursionLimit.HasValue)
            {
                throw new NotSupportedException();
            }

            var response = context.HttpContext.Response;

            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : APPLICATION_JSON;

            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data == null)
            {
                return;
            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = SerializerHelper.GetDefaultContractResolver()
            };

            response.Write(JsonConvert.SerializeObject(Data, serializerSettings));
        }
    }
}
