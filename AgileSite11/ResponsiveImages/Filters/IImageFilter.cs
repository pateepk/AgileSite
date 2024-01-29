namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Represents an image filter which can be applied on an image.
    /// </summary>
    public interface IImageFilter
    {
        /// <summary>
        /// Applies the filter on the image data.
        /// </summary>
        /// <param name="container">Input image container.</param>
        /// <returns>
        /// New instance of <see cref="ImageContainer"/> with the applied filter or <c>null</c> when the filter was not applied.
        /// </returns>
        /// <exception cref="ImageFilterException">Thrown when an error occurs during the filter application. Throwing this exception will cause that the image variant will not be generated.</exception>
        ImageContainer ApplyFilter(ImageContainer container);
    }
}
