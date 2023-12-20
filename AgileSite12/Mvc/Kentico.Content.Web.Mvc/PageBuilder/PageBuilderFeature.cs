using System;
using System.Web;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.Helpers;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents a feature that provides information about Page builder for the current request.
    /// </summary>
    internal sealed class PageBuilderFeature : IPageBuilderFeature, IPageBuilderInternalProperties
    {
        private int mPageIdentifier;
        private IPageBuilderDataContext mDataContext;

        private readonly HttpContextBase context;
        private readonly IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever;
        private readonly IVirtualContextPageRetriever virtualContextPageRetriever;
        private readonly IPageSecurityChecker securityChecker;


        /// <summary>
        /// Gets page builder options.
        /// </summary>
        public PageBuilderOptions Options
        {
            get;
        }


        /// <summary>
        /// Gets the data context cached per request.
        /// </summary>
        public IPageBuilderDataContext DataContext
        {
            get
            {
                if (mDataContext == null)
                {
                    mDataContext = new PageBuilderDataContext(PageIdentifier, EditingInstanceIdentifier);
                }

                return mDataContext;
            }
        }


        /// <summary>
        /// Gets a value indicating whether edit mode is enabled.
        /// </summary>
        public bool EditMode
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the identifier of the page where the Page builder stores and loads data from.
        /// </summary>
        public int PageIdentifier
        {
            get
            {
                if (mPageIdentifier <= 0 && EditMode)
                {
                    mPageIdentifier = TryGetFromPostData();
                }

                return mPageIdentifier;
            }
        }


        /// <summary>
        /// The editing instance identifier used for admin user interface.
        /// </summary>
        public Guid EditingInstanceIdentifier { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PageBuilderFeature"/> class from the Kentico virtual context.
        /// </summary>
        /// <param name="context">HTTP context of the application.</param>
        /// <param name="options">The Page builder options.</param>
        /// <param name="virtualContextPageRetriever">Retriever for a page initialized from virtual context.</param>
        /// <param name="securityChecker">Page security checker.</param>
        /// <param name="postDataRetriever">Retriever for POST data.</param>
        internal PageBuilderFeature(HttpContextBase context,
                                    PageBuilderOptions options,
                                    IVirtualContextPageRetriever virtualContextPageRetriever,
                                    IPageSecurityChecker securityChecker,
                                    IPageBuilderPostDataRetriever<PageBuilderPostData> postDataRetriever) :
            this(context, options, virtualContextPageRetriever)
        {
            this.securityChecker = securityChecker;
            this.postDataRetriever = postDataRetriever;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PageBuilderFeature"/> class from the Kentico virtual context.
        /// </summary>
        /// <param name="context">HTTP context of the application.</param>
        /// <param name="options">Page builder options.</param>
        public PageBuilderFeature(HttpContextBase context, PageBuilderOptions options) :
            this(context, options, new VirtualContextPageRetriever())
        {
            securityChecker = new PageSecurityChecker(virtualContextPageRetriever);
            postDataRetriever = new PageBuilderPostDataRetriever<PageBuilderPostData>(context, securityChecker);
        }


        private PageBuilderFeature(HttpContextBase context, PageBuilderOptions options, IVirtualContextPageRetriever virtualContextPageRetriever)
        {
            if (VirtualContext.IsPreviewLinkInitialized)
            {
                EditMode = context.Request.QueryString[PageBuilderHelper.EDIT_MODE_QUERY_NAME] != null;
                if (Guid.TryParse(context.Request.QueryString[PageBuilderHelper.EDITING_INSTANCE_QUERY_NAME], out var editingInstanceIndentifier))
                {
                    EditingInstanceIdentifier = editingInstanceIndentifier;
                }
            }

            Options = options;

            this.context = context;
            this.virtualContextPageRetriever = virtualContextPageRetriever;
        }


        /// <summary>
        /// Initializes the Page builder.
        /// </summary>
        /// <param name="pageIdentifier">The identifier of the page where the Page builder stores and loads data from.</param>
        public void Initialize(int pageIdentifier)
        {
            if (pageIdentifier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageIdentifier), "The Page builder feature needs to be initialized with identifier of an existing page.");
            }

            ClearPageCache(pageIdentifier);
            ValidateEditMode(pageIdentifier);

            mPageIdentifier = pageIdentifier;
        }


        private void ValidateEditMode(int pageIdentifier)
        {
            if (!EditMode)
            {
                return;
            }

            var page = virtualContextPageRetriever.Retrieve();
            if (pageIdentifier != page?.DocumentID)
            {
                EditMode = false;
                return;
            }

            if (!securityChecker.Check(PermissionsEnum.Modify))
            {
                throw new InvalidOperationException("User is not authorized to modify current page.");
            }
        }


        private void ClearPageCache(int pageIdentifier)
        {
            if (!VirtualContext.IsPreviewLinkInitialized)
            {
                return;
            }

            if (context.Request.QueryString[PageBuilderHelper.CLEAR_PAGE_CACHE_QUERY_NAME] == null)
            {
                return;
            }

            var page = virtualContextPageRetriever.Retrieve();
            if (pageIdentifier != page?.DocumentID)
            {
                return;
            }

            if (page.DocumentCheckedOutVersionHistoryID <= 0)
            {
                return;
            }

            var history = VersionHistoryInfoProvider.GetVersionHistoryInfo(page.DocumentCheckedOutVersionHistoryID);
            if (history == null)
            {
                return;
            }

            history.Generalized.Invalidate(false);
            CacheHelper.TouchKeys(new[] { CacheHelper.GetCacheItemName(null, "documentid", page.DocumentID) });
        }


        private int TryGetFromPostData()
        {
            var data = postDataRetriever.Retrieve();
            if (data == null)
            {
                return 0;
            }

            return data.PageIdentifier;
        }
    }
}
