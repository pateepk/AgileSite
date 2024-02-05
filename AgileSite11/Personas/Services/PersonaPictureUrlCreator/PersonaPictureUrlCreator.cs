using System;
using System.Globalization;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Personas
{
    /// <summary>
    /// Creates urls of the persona images.
    /// </summary>
    public sealed class PersonaPictureUrlCreator : IPersonaPictureUrlCreator
    {
        /// <summary>
        /// Guid of the default image used when persona does not have picture selected.
        /// </summary>
        /// <remarks>
        /// Default picture is class thumbnail whose name starts with "default.".
        /// </remarks>
        private readonly Lazy<Guid?> defaultPictureGuid = new Lazy<Guid?>(() => new DefaultClassThumbnail(PersonaInfo.OBJECT_TYPE).GetDefaultClassThumbnailGuid());


        /// <summary>
        /// Gets URL of the the image which should be used when persona has no picture selected or when picture of contact belonging to no persona should be displayed.
        /// </summary>
        /// <param name="imageSideSizeInPx">Maximum size of one side of the image. Persona pictures are usually squares</param>
        /// <exception cref="ArgumentException"><paramref name="imageSideSizeInPx"/> is lower than 1</exception>
        /// <returns>Url of the default persona picture</returns>
        public string CreateDefaultPersonaPictureUrl(int imageSideSizeInPx)
        {
            if (imageSideSizeInPx <= 0)
            {
                throw new ArgumentException("Persona picture size has to be 1px or greater");
            }

            Guid? defaultPersonaPictureGuid = defaultPictureGuid.Value;

            if (defaultPersonaPictureGuid == null)
            {
                return null;
            }

            return GetMetafileUrl(defaultPersonaPictureGuid.Value, "default", imageSideSizeInPx);
        }


        /// <summary>
        /// Gets url of the specified persona's picture.
        /// </summary>
        /// <param name="persona">Url to the image of this persona will be created</param>
        /// <param name="imageSideSizeInPx">Maximum size of pictures side</param>
        /// <exception cref="ArgumentNullException"><paramref name="persona"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="imageSideSizeInPx"/> is lower than 1</exception>
        /// <returns>Url leading to the persona's image or to the default image if persona does not have picture set</returns>
        public string CreatePersonaPictureUrl(PersonaInfo persona, int imageSideSizeInPx)
        {
            if (persona == null)
            {
                throw new ArgumentNullException("persona");
            }
            if (imageSideSizeInPx <= 0)
            {
                throw new ArgumentException("Persona picture size has to be 1px or greater");
            }

            Guid? pictureGuid = persona.PersonaPictureMetafileGUID ?? defaultPictureGuid.Value;

            if (pictureGuid == null)
            {
                return null;
            }

            return GetMetafileUrl(pictureGuid.Value, persona.PersonaName, imageSideSizeInPx);
        }


        /// <summary>
        /// Gets url to the specified metafile.
        /// </summary>
        /// <param name="metafileGuid">Url to the metafile with this guid will be created</param>
        /// <param name="metafileSeoFileName">Returned image will have this name. No spaces are allowed</param>
        /// <param name="sizeInPx">Maximum size of one side of picture</param>
        /// <returns>Url to the metafil</returns>
        private string GetMetafileUrl(Guid metafileGuid, string metafileSeoFileName, int sizeInPx)
        {
            string imageUrl = MetaFileURLProvider.GetMetaFileUrl(metafileGuid, metafileSeoFileName);
            imageUrl = URLHelper.UpdateParameterInUrl(imageUrl, "maxsidesize", sizeInPx.ToString(CultureInfo.InvariantCulture));
            imageUrl = URLHelper.ResolveUrl(imageUrl);

            return imageUrl;
        }
    }
}
