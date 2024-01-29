namespace CMS.PortalEngine
{
    /// <summary>
    /// Web part (web part zone) events
    /// </summary>
    public static class WebPartEvents
    {
        /// <summary>
        /// Fires when web parts load variants
        /// </summary>
        public static WebPartLoadVariantHandler WebPartZoneLoadVariant = new WebPartLoadVariantHandler { Name = "WebPartEvents.WebPartZoneLoadVariant" };

        /// <summary>
        /// Fires when web parts load variants
        /// </summary>
        public static WebPartLoadVariantHandler WebPartLoadVariant = new WebPartLoadVariantHandler { Name = "WebPartEvents.WebPartLoadVariant" };

        /// <summary>
        /// Fires when single web part is moved
        /// </summary>
        public static MoveWebPartHandler MoveWebPart = new MoveWebPartHandler { Name = "WebPartEvents.MoveWebPart" };

        /// <summary>
        /// Fires when all web parts are moved
        /// </summary>
        public static MoveWebPartHandler MoveAllWebParts = new MoveWebPartHandler { Name = "WebPartEvents.MoveAllWebParts" };

        /// <summary>
        /// Fires when web part is removed
        /// </summary>
        public static RemoveWebPartHandler RemoveWebPart = new RemoveWebPartHandler { Name = "WebPartEvents.RemoveWebPart" };

        /// <summary>
        /// Fires when all web part are removed
        /// </summary>
        public static RemoveWebPartHandler RemoveAllWebParts = new RemoveWebPartHandler { Name = "WebPartEvents.RemoveAllWebParts" };

        /// <summary>
        /// Fires when a layout zone id is changed
        /// </summary>
        public static ChangeLayoutZoneIdHandler ChangeLayoutZoneId = new ChangeLayoutZoneIdHandler { Name = "WebPartEvents.ChangeLayoutZoneId" };
    }
}
