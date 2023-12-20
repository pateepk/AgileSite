using System;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.PageBuilder;

[assembly: RegisterImplementation(typeof(ITempPageBuilderWidgetsPropagator), typeof(TempPageBuilderWidgetsPropagator), Priority = RegistrationPriority.Fallback)]

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Propagates widgets and template configuration from temporary data to page data.
    /// </summary>
    public class TempPageBuilderWidgetsPropagator : ITempPageBuilderWidgetsPropagator
    {
        private const string TEMP_GUID_COLUMN_NAME = "PageBuilderWidgetsGuid";
        private const string WIDGETS_DATA_COLUMN_NAME = "DocumentPageBuilderWidgets";
        private const string TEMPLATE_DATA_COLUMN_NAME = "DocumentPageTemplateConfiguration";

        private TreeNode page;


        /// <summary>
        /// Initializes a new instance of the <see cref="TempPageBuilderWidgetsPropagator"/> class.
        /// </summary>
        public TempPageBuilderWidgetsPropagator()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TempPageBuilderWidgetsPropagator"/> class.
        /// </summary>
        /// <param name="page">Page instance.</param>
        [Obsolete("Use CMS.Core.Service.Resolve<CMS.DocumentEngine.PageBuilder.ITempPageBuilderWidgetsPropagator>() in conjunction with the Propagate(CMS.DocumentEngine.TreeNode, System.Guid) method override instead.")]
        public TempPageBuilderWidgetsPropagator(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            this.page = page;
        }


        /// <summary>
        /// Propagates widgets and template configuration from temporary data to page data.
        /// </summary>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        [Obsolete("Use CMS.Core.Service.Resolve<CMS.DocumentEngine.PageBuilder.ITempPageBuilderWidgetsPropagator>() in conjunction with the Propagate(CMS.DocumentEngine.TreeNode, System.Guid) method override instead.")]
        public void Propagate(Guid instanceGuid)
        {
            if (page == null)
            {
                throw new InvalidOperationException("The class must be instantiated using the constructor specifying CMS.DocumentEngine.TreeNode instance in order to use this method.");
            }

            Propagate(page, instanceGuid);
        }


        /// <summary>
        /// Propagates widgets and template configuration from temporary data to page data.
        /// </summary>
        /// <param name="page">Page to propagate data into.</param>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public virtual void Propagate(TreeNode page, Guid instanceGuid)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var tempData = GetTempDataQuery(instanceGuid).TopN(1).FirstOrDefault();
            if (tempData == null)
            {
                throw new InvalidOperationException($"Temporary data wasn't found for '{instanceGuid}' editing instance.");
            }

            if(!string.IsNullOrEmpty(tempData.PageBuilderWidgetsConfiguration))
            {
                page.SetValue(WIDGETS_DATA_COLUMN_NAME, tempData.PageBuilderWidgetsConfiguration);
            }

            if (!string.IsNullOrEmpty(tempData.PageBuilderTemplateConfiguration))
            {
                page.SetValue(TEMPLATE_DATA_COLUMN_NAME, tempData.PageBuilderTemplateConfiguration);
            }
        }


        /// <summary>
        /// Deletes widgets configuration from temporary data.
        /// </summary>
        /// <param name="instanceGuid">Instance GUID of editing.</param>
        public void Delete(Guid instanceGuid)
        {
            var tempData = GetTempDataQuery(instanceGuid).TopN(1).FirstOrDefault();
            tempData?.Delete();
        }


        /// <summary>
        /// Gets object query for retrieving <see cref="TempPageBuilderWidgetsInfo"/> by its GUID.
        /// </summary>
        /// <param name="instanceGuid">GUID of the <see cref="TempPageBuilderWidgetsInfo"/> to retrieve.</param>
        /// <returns>Returns query for retrieving <see cref="TempPageBuilderWidgetsInfo"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instanceGuid"/> is <see cref="Guid.Empty"/>.</exception>
        protected ObjectQuery<TempPageBuilderWidgetsInfo> GetTempDataQuery(Guid instanceGuid)
        {
            if (instanceGuid == Guid.Empty)
            {
                throw new ArgumentException("Editing instance GUID not provided.", nameof(instanceGuid));
            }

            return TempPageBuilderWidgetsInfoProvider.GetPageBuilderWidgets()
                                                     .WhereEquals(TEMP_GUID_COLUMN_NAME, instanceGuid);
        }
    }
}