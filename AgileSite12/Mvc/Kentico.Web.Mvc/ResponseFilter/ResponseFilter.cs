using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using CMS.Base;
using CMS.Helpers;

namespace Kentico.Web.Mvc.Internal
{
    /// <summary>
    /// Ensures that filter routines are executed on response stream data
    /// </summary>
    public sealed class ResponseFilter
    {
        private const string INSTANCE_KEY = "KenticoResponseFilterInstance";
        private readonly HttpContextBase httpContext;
        private readonly IList<Func<string, string>> filterRegister;
        private readonly ISet<string> filterNames; 
        private bool responseFilterRegistered;


        /// <summary>
        /// Collection of registered filters, used for testing purposes.
        /// </summary>
        internal IEnumerable<Func<string, string>> Filters => filterRegister;


        /// <summary>
        /// Creates new instance of <see cref="ResponseFilter"/>.
        /// </summary>
        internal ResponseFilter()
            : this(new List<Func<string, string>>(), CMSHttpContext.Current)
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="ResponseFilter"/>.
        /// </summary>
        internal ResponseFilter(IList<Func<string, string>> filterRegister, HttpContextBase httpContext)
        {
            this.filterRegister = filterRegister;
            this.httpContext = httpContext;

            filterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Initialize ResponseFilter pipeline
        /// </summary>
        internal static void Init()
        {
            RequestEvents.ReleaseRequestState.Execute += (s, e) => Instance.EnsureRepsonseFilter();
        }


        /// <summary>
        /// Indicates whether response filter can be used. Depends on existence of underlaying <see cref="HttpResponse.Filter"/> stream.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                return CMSHttpContext.Current?.Response?.Filter != null;
            }
        }


        /// <summary>
        /// Indicates whether current response is text/html content type
        /// </summary>
        public static bool IsHtmlResponse
        {
            get
            {
                return String.Equals(CMSHttpContext.Current?.Response?.ContentType, "text/html", StringComparison.OrdinalIgnoreCase);
            }
        }


        /// <summary>
        /// Returns request specific instance of <see cref="ResponseFilter"/>.
        /// </summary>
        public static ResponseFilter Instance
        {
            get
            {
                var items = CMSHttpContext.Current?.Items;
                var instance = items[INSTANCE_KEY] as ResponseFilter;
                if (instance == null)
                {
                    instance = new ResponseFilter();
                    items[INSTANCE_KEY] = instance;
                }

                return instance;
            }
        }

               
        /// <summary>
        /// Add response filter to the collection of filters to be executed for current response.
        /// </summary>
        /// <param name="filterName">Unique filter name</param>
        /// <param name="filter">Response filter</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> or <paramref name="filterName"/> is not set.</exception>
        public void Add(string filterName, Func<string, string> filter)
        {
            if (String.IsNullOrEmpty(filterName))
            {
                throw new ArgumentException(nameof(filterName));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

           if (!filterNames.Contains(filterName))
            {
                filterRegister.Add(filter);
                filterNames.Add(filterName);
            }
        }


        /// <summary>
        /// Ensures that <see cref="ResponseFilterStream" /> is registered as the <see cref="HttpResponse.Filter"/> stream only once.
        /// </summary>
        internal void EnsureRepsonseFilter()
        {
            if (responseFilterRegistered || !filterRegister.Any() || httpContext?.Response?.Filter == null)
            {
                return;
            }

            httpContext.Response.Filter = new ResponseFilterStream(httpContext.Response.Filter, filterRegister);
            responseFilterRegistered = true;
        }
    }
}
