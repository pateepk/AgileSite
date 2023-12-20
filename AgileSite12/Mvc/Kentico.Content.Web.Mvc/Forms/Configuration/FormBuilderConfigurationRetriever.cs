using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Forms.Web.Mvc.Internal;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Retrieves form builder configuration for a form.
    /// </summary>
    /// <remarks>
    /// Provides a cached wrapper around the <see cref="IFormBuilderConfigurationSerializer.Deserialize(string)"/> method.
    /// </remarks>
    internal sealed class FormBuilderConfigurationRetriever : IFormBuilderConfigurationRetriever
    {
        internal int CacheMinutes { get; set; } = 10;

        private readonly IFormBuilderConfigurationSerializer serializer;


        /// <summary>
        /// Creates an instance of <see cref="FormBuilderConfigurationRetriever"/> class.
        /// </summary>
        /// <param name="serializer">Form builder configuration serializer.</param>
        public FormBuilderConfigurationRetriever(IFormBuilderConfigurationSerializer serializer)
        {
            this.serializer = serializer;
        }


        /// <summary>
        /// Retrieves configuration for a given form.
        /// </summary>
        /// <param name="formInfo">Form to retrieve configuration for.</param>
        /// <remarks>
        /// Configuration is retrieved from cache if already cached.
        /// </remarks>
        public FormBuilderConfiguration Retrieve(BizFormInfo formInfo)
        {
            return CacheHelper.Cache(
                () => serializer.Deserialize(formInfo.FormBuilderLayout),
                new CacheSettings(CacheMinutes, "FormBuilder", "FormBuilderConfigurationPerForm", formInfo.FormID)
                {
                    GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] {
                        CacheHelper.GetCacheItemName(null, BizFormInfo.OBJECT_TYPE, "byid", formInfo.FormID)
                    })
                });
        }
    }
}