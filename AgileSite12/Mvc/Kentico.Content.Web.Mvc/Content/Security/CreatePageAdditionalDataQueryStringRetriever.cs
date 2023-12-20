using System;

using CMS.Base;
using CMS.Core;


namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Implements <see cref="ICreatePageAdditionalDataRetriever"/> by extracting the desired additional data from the query string.
    /// </summary>
    internal class CreatePageAdditionalDataQueryStringRetriever : ICreatePageAdditionalDataRetriever
    {
        private readonly IHttpContextAccessor httpContextAccessor;


        public CreatePageAdditionalDataQueryStringRetriever()
            : this(Service.Resolve<IHttpContextAccessor>())
        {
        }


        internal CreatePageAdditionalDataQueryStringRetriever(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        /// <summary>
        /// Returns <see cref="CreatePagePermissionCheckAdditionalData"/> populated with data from query string.
        /// </summary>
        public CreatePagePermissionCheckAdditionalData GetData()
        {
            var queryString = httpContextAccessor.HttpContext.Request.QueryString;

            return new CreatePagePermissionCheckAdditionalData
            {
                PageType = queryString["pagetype"],
                Culture = queryString["culture"],
            };
        }
    }
}
