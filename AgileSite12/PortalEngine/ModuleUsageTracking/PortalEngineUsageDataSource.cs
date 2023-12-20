using System;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.ModuleUsageTracking;

[assembly: RegisterModuleUsageDataSource(typeof(PortalEngineUsageDataSource))]

namespace CMS.PortalEngine.ModuleUsageTracking
{
    /// <summary>
    /// Module usage data for portal engine.
    /// </summary>
    internal class PortalEngineUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Portal engine usage data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.PortalEngine";
            }
        }


        /// <summary>
        /// Get portal engine usage data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();
            
            #region "Objects with custom CSS"

            // Get count of transformations containing custom CSS
            result.Add("TransformationWithCSSCount",
                GetCSSCount(TransformationInfoProvider.GetTransformations(), "TransformationCSS"));

            // Get count of web parts containing custom CSS
            result.Add("WebPartsWithCSSCount",
                GetCSSCount(WebPartInfoProvider.GetWebParts(), "WebPartCSS"));

            // Get count of web part layouts containing custom CSS
            result.Add("WebParLayoutsWithCSSCount",
                GetCSSCount(WebPartLayoutInfoProvider.GetWebPartLayouts(), "WebPartLayoutCSS"));

            // Get count of page templates containing custom CSS
            result.Add("PageTemplatesWithCSSCount",
                GetCSSCount(PageTemplateInfoProvider.GetTemplates(), "PageTemplateCSS"));

            // Get count of web part containers containing custom CSS
            result.Add("WebPartContainersWithCSSCount",
                GetCSSCount(WebPartContainerInfoProvider.GetContainers(), "ContainerCSS"));

            // Get count of layouts containing custom CSS
            result.Add("LayoutsWithCSSCount",
                GetCSSCount(LayoutInfoProvider.GetLayouts(), "LayoutCSS"));

            // Get count of template device layouts containing custom CSS
            result.Add("TemplateDeviceLayoutsWithCSSCount",
                GetCSSCount(PageTemplateDeviceLayoutInfoProvider.GetTemplateDeviceLayouts(), "LayoutCSS"));

            #endregion


            return result;
        }


        /// <summary>
        /// Gets the total number of objects which have defined custom CSS.
        /// </summary>
        /// <param name="query">The object query to be used for data retrieval</param>
        /// <param name="cssColumnName">Name of the column which carries CSS</param>
        private static int GetCSSCount<T>(ObjectQuery<T> query, string cssColumnName)
            where T : AbstractInfoBase<T>, new()
        {
            return query
                .Column(cssColumnName)
                // Empty columns can contain CR LF. 
                .WhereGreaterThan(String.Format("DATALENGTH({0})", cssColumnName), 2)
                .Count;
        }

    }
}
