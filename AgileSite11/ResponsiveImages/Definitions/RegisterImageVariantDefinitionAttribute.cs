using System;

namespace CMS.ResponsiveImages
{
    /// <summary>
    /// Registers an image variant definition within the system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterImageVariantDefinitionAttribute : Attribute, IPreInitAttribute
    {
        private readonly Type mMarkedType;

        
        /// <summary>
        /// Definition's type.
        /// </summary>
        public Type MarkedType
        {
            get
            {
                return mMarkedType;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Definition type which implements <see cref="IImageVariantDefinition"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the definition type doesn't implement the <see cref="IImageVariantDefinition"/> interface,
        ///     is abstract or doesn't provide a parameterless constructor.
        /// </exception>
        public RegisterImageVariantDefinitionAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!typeof(IImageVariantDefinition).IsAssignableFrom(type))
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "must implement the IImageVariantDefinition interface.", type.FullName), "type");
            }

            if (type.IsAbstract)
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "cannot be abstract.", type.FullName), "type");
            }

            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException(String.Format("Image variant definition's implementation ({0}) " +
                                                          "must include a parameterless constructor.", type.FullName), "type");
            }

            mMarkedType = type;
        }


        /// <summary>
        /// Registers the image variant definition during application pre-initialization.
        /// </summary>
        public void PreInit()
        {
            ImageVariantDefinitionManager.RegisterDefinition(MarkedType);            
        }
    }
}
