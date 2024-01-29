using System;
using System.Collections.Generic;


namespace NHG_C.BlueKey
{
    /// <summary>
    /// Static configuration class for client/site config information
    /// </summary>
    public static class Configuration
    {
        /// Debug Settings
        public static Boolean DebugMode { get { return true; } }
        public static Boolean LogDebug { get { return true; } }
        public static string DebugEmail { get { return "nshepherd+testing@bluekeyinc.com"; } }

        /// Shipping options
        public static string DefaultShippingCode { get { return "FDX_FEDEX_GROUND"; } }
        public static string DefaultShippingCodeName { get { return "FedEx Ground"; } }
        public static string ShipFromZip { get { return "29418"; } }
        public static string ShipFromCountry { get { return "US"; } }
        public static string SendSupplierEmail { get { return "true"; } }
        public static string AuthLogin { get { return "merrifieldgarden123"; } }
        public static string AuthKey { get { return "32cT5XtkE2d6S4Sd"; } }
        public static Boolean EmailSuppliers { get { return true; } }
    }
}