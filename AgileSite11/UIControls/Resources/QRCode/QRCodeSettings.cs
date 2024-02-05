using System.Drawing;

namespace CMS.UIControls
{
    /// <summary>
    /// Contains settings for the QR code generator
    /// </summary>
    public class QRCodeSettings
    {
        #region "Properties"

        /// <summary>
        /// Encoding, possible options (B - Byte, AN - Alphanumeric, N - Numeric)
        /// </summary>
        public string Encoding
        {
            get;
            set;
        }


        /// <summary>
        /// Image size, image is rendered with recommended resolution for the QR code
        /// </summary>
        public int Size
        {
            get;
            set;
        }


        /// <summary>
        /// QR code version (by default supported 1 to 10), additional data templates may be put into ~/App_Data/CMS_Modules/QRCode/Resources.zip
        /// </summary>
        public int Version
        {
            get;
            set;
        }


        /// <summary>
        /// Correction type, possible options (L, M, Q, H)
        /// </summary>
        public string Correction
        {
            get;
            set;
        }


        /// <summary>
        /// Foreground color
        /// </summary>
        public Color? FgColor
        {
            get;
            set;
        }


        /// <summary>
        /// Background color
        /// </summary>
        public Color? BgColor
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encoding">Encoding, possible options (B - Byte, AN - Alphanumeric, N - Numeric)</param>
        /// <param name="size">Image size, image is rendered with recommended resolution for the QR code</param>
        /// <param name="version">QR code version (by default supported 1 to 10), additional data templates may be put into ~/App_Data/CMS_Modules/QRCode/Resources.zip</param>
        public QRCodeSettings(string encoding, int size, int version)
        {
            Encoding = encoding;
            Size = size;
            Version = version;
        }

        #endregion
    }
}