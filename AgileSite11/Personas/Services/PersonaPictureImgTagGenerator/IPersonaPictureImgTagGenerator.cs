using CMS;
using CMS.Personas;

[assembly: RegisterImplementation(typeof(IPersonaPictureImgTagGenerator), typeof(PersonaPictureImgTagGenerator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Personas
{    
    /// <summary>
    /// Defines contract of the service capable of generating HTML img tags displaying persona picture.
    /// </summary>
    public interface IPersonaPictureImgTagGenerator
    {
        /// <summary>
        /// Generates img tag which displays persona picture with a specified size.
        /// </summary>
        /// <param name="persona">This persona's picture will be displayed</param>
        /// <param name="imageSideSizeInPx">Size of the picture</param>
        /// <returns>HTML which displays persona picture</returns>
        string GenerateImgTag(PersonaInfo persona, int imageSideSizeInPx);
    }
}