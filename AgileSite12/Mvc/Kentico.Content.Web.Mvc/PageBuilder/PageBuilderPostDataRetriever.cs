using System;
using System.IO;
using System.Web;

using CMS.DataEngine;

using Kentico.Content.Web.Mvc;

using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Retrieves data from Page builder POST request.
    /// </summary>
    internal sealed class PageBuilderPostDataRetriever<TPostData> : IPageBuilderPostDataRetriever<TPostData>
        where TPostData : class
    {
        private const string DATA_KEY = "Kentico.PageBuilder.PostData";
        private readonly HttpContextBase context;
        private readonly IPageSecurityChecker checker;


        /// <summary>
        /// Creates an instance of <see cref="IPageBuilderPostDataRetriever{TPostData}"/> class.
        /// </summary>
        /// <param name="context">HTTP context of the application.</param>
        /// <param name="checker">Page permission checker.</param>
        /// <exception cref="ArgumentNullException">Is thrown when <paramref name="context"/> or <paramref name="checker"/> is <c>null</c>.</exception>
        public PageBuilderPostDataRetriever(HttpContextBase context, IPageSecurityChecker checker)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }


        /// <summary>
        /// Retrieves data from the page builder POST request.
        /// </summary>
        /// <returns>Returns deserialized POST request data into the type <typeparamref name="TPostData"/>.</returns>
        /// <exception cref="InvalidOperationException">Is thrown when incorrect data format is retrieved and cannot be deserialized into the type <typeparamref name="TPostData"/>.</exception>
        public TPostData Retrieve()
        {
            if (!IsPostRequest())
            {
                return null;
            }

            return GetOrCreateFromContext();
        }


        private TPostData GetOrCreateFromContext()
        {
            var items = context.Items;

            TPostData data = items[DATA_KEY] as TPostData;
            if (data == null)
            {
                data = GetDataFromBody();
                items[DATA_KEY] = data;
            }

            return data;
        }


        private TPostData GetDataFromBody()
        {
            if (!checker.Check(PermissionsEnum.Modify))
            {
                throw new InvalidOperationException("User is not authorized to modify current page.");
            }

            try
            {
                var json = JObject.Parse(GetJsonFromBody(context.Request));
                return json.ToObject<TPostData>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Incorrect data format is retrieved.", ex);
            }
        }


        private bool IsPostRequest()
        {
            var request = context.Request;

            return string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase);
        }


        private string GetJsonFromBody(HttpRequestBase request)
        {
            string json;
            Stream stream = null;

            try
            {
                stream = request.InputStream;

                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = CMS.IO.StreamReader.New(stream))
                {
                    json = reader.ReadToEnd();
                }
            }
            finally
            {
                stream?.Dispose();
            }

            return json;
        }
    }
}
