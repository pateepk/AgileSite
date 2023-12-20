using System.Drawing;

using CMS;
using CMS.Helpers;
using CMS.UIControls;

[assembly: RegisterImplementation(typeof(IQRCodeGenerator), typeof(QRCodeGenerator), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.UIControls
{
    /// <summary>
    /// Interface for QR code generator
    /// </summary>
    public interface IQRCodeGenerator
    {
        /// <summary>
        /// Generates the QR code as an image using the given parameters
        /// </summary>
        /// <param name="code">Code to generate by the QR code</param>
        /// <param name="qrCodeSettings">QR code settings</param>
        Image GenerateQRCode(string code, QRCodeSettings qrCodeSettings);
    }
}