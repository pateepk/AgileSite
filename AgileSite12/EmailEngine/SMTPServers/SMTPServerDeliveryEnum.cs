using CMS.Helpers;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Enumeration of possible SMTP server delivery methods.
    /// </summary>
    public enum SMTPServerDeliveryEnum : int
    {
        /// <summary>
        /// Email is sent through the network to an SMTP server.
        /// </summary>
        [EnumDefaultValue]
        [EnumOrder(0)]
        [EnumStringRepresentation("network")]
        Network,


        /// <summary>
        /// Email is copied to the directory specified by the <see cref="CMS.EmailEngine.SMTPServerInfo.ServerPickupDirectory"/> property for delivery by an external application.
        /// </summary>
        [EnumOrder(1)]
        [EnumStringRepresentation("pickupdirectory")]
        SpecifiedPickupDirectory,


        /// <summary>
        /// Email is copied to the pickup directory used by a local Internet Information Services (IIS) for delivery.
        /// </summary>
        [EnumOrder(2)]
        [EnumStringRepresentation("pickupfromiis")]
        PickupDirectoryFromIis
    }
}