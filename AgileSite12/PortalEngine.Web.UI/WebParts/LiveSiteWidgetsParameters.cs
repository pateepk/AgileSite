using System;

using CMS.Helpers;
using CMS.Localization;

namespace CMS.PortalEngine.Web.UI.Internal
{
    /// <summary>
    /// Parameters of live site dialog with widget properties.
    /// </summary>
    public class LiveSiteWidgetsParameters
    {
        private const string PURPOSE = "LiveWidgetPropertiesDialog";

        private readonly string aliasPath;
        private readonly ViewModeEnum viewMode;


        /// <summary>
        /// Zone identifier.
        /// </summary>
        public string ZoneId { get; set; } = string.Empty;


        /// <summary>
        /// Zone type.
        /// </summary>
        public WidgetZoneTypeEnum ZoneType { get; set; } = WidgetZoneTypeEnum.None;


        /// <summary>
        /// Template identifier.
        /// </summary>
        public int TemplateId { get; set; } = 0;


        /// <summary>
        /// Instance guid.
        /// </summary>
        public Guid InstanceGuid { get; set; } = Guid.Empty;


        /// <summary>
        /// Indicates whether widget is inline widget.
        /// </summary>
        public bool IsInlineWidget { get; set; } = false;


        /// <summary>
        /// Creates an instance of the <see cref="LiveSiteWidgetsParameters"/> class
        /// </summary>
        /// <param name="aliasPath">Node alias path.</param>
        /// <param name="viewMode">View mode.</param>
        public LiveSiteWidgetsParameters(string aliasPath, ViewModeEnum viewMode)
        {
            this.aliasPath = aliasPath;
            this.viewMode = viewMode;
        }


        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"{ZoneId}|{ZoneType}|{aliasPath}|{TemplateId}|{InstanceGuid}|{LocalizationContext.PreferredCultureCode}|{viewMode}|{IsInlineWidget}";
        }


        /// <summary>
        /// Returns hash string for the current instance.
        /// </summary>
        public string GetHashString()
        {
            return ValidationHelper.GetHashString(ToString(), new HashSettings(PURPOSE));
        }


        /// <summary>
        /// Validates given <paramref name="hash"/> for current instance.
        /// </summary>
        /// <param name="hash">Hash to validate.</param>
        public bool ValidateHash(string hash)
        {
            return ValidationHelper.ValidateHash(ToString(), hash, new HashSettings(PURPOSE) { Redirect = true });
        }
    }
}
