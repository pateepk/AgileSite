using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Helper class for sample web part generation purposes.
    /// </summary>
    public sealed class SampleWebPartsGenerator
    {
        private const string SAMPLE_WEB_PART_CATEGORY_DISPLAY_NAME = "Dancing Goat";
        private const string SAMPLE_WEB_PART_CATEGORY_NAME = "SampleCategoryDancingGoat";


        /// <summary>
        /// Creates a web part if it doesn't exist.
        /// </summary>
        /// <param name="webPartName">Web part code name.</param>
        /// <param name="webPartDisplayName">Web part display name.</param>
        /// <param name="webPartFilePath">Web part file path.</param>
        /// <param name="webPartResourceId">Id of the module.</param>
        public static void EnsureWebpart(string webPartName, string webPartDisplayName, string webPartFilePath, int? webPartResourceId = null)
        {
            var categoryInfo = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName(SAMPLE_WEB_PART_CATEGORY_NAME);
            if (categoryInfo == null)
            {
                categoryInfo = GetWebPartCategoryInfo();
                WebPartCategoryInfoProvider.SetWebPartCategoryInfo(categoryInfo);
            }

            var webPartInfo = WebPartInfoProvider.GetWebPartInfo(webPartName);
            if (webPartInfo == null)
            {
                if (webPartResourceId.HasValue)
                {
                    webPartInfo = InitializeWebPartInfo(categoryInfo.CategoryID, webPartName, webPartDisplayName, webPartFilePath, webPartResourceId.Value);
                }
                else
                {
                    webPartInfo = InitializeWebPartInfo(categoryInfo.CategoryID, webPartName, webPartDisplayName, webPartFilePath);
                }
                WebPartInfoProvider.SetWebPartInfo(webPartInfo);
            }
        }


        private static WebPartCategoryInfo GetWebPartCategoryInfo()
        {
            return new WebPartCategoryInfo
            {
                CategoryName = SAMPLE_WEB_PART_CATEGORY_NAME,
                CategoryParentID = WebPartCategoryInfoProvider.GetWebPartCategoryInfoByCodeName("/").CategoryID,
                CategoryDisplayName = SAMPLE_WEB_PART_CATEGORY_DISPLAY_NAME,
            };
        }


        private static WebPartInfo InitializeWebPartInfo(int webPartCategoryId, string webPartName, string webPartDisplayName, string webPartFilePath)
        {
            return new WebPartInfo
            {
                WebPartDisplayName = webPartDisplayName,
                WebPartFileName = webPartFilePath,
                WebPartName = webPartName,
                WebPartCategoryID = webPartCategoryId,
                WebPartIconClass = PortalHelper.DefaultWebPartIconClass,
                WebPartType = (int)WebPartTypeEnum.Standard,
                WebPartProperties = "<defaultvalues/>",
                WebPartDefaultValues = "<form/>",
            };
        }


        private static WebPartInfo InitializeWebPartInfo(int webPartCategoryId, string webPartName, string webPartDisplayName, string webPartFilePath, int webPartResourceId)
        {
            var info = InitializeWebPartInfo(webPartCategoryId, webPartName, webPartDisplayName, webPartFilePath);
            info.WebPartResourceID = webPartResourceId;

            return info;
        }
    }
}
