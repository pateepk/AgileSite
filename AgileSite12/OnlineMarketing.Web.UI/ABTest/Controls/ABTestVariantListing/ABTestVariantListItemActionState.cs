using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Represents possible states of a variant list item action.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
    internal enum ABTestVariantListItemActionState
    {
        Hidden,
        Disabled,
        Enabled
    }
}
