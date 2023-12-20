using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Registers class in <see cref="CustomCloneSettings"/> collection for advanced clone settings usage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterCustomCloneSettingsControlAttribute : Attribute, IInitAttribute
    {
        /// <summary>
        /// Info object type
        /// </summary>
        public string ObjectType
        {
            get;
        }


        /// <summary>
        /// Custom settings class
        /// </summary>
        public Type MarkedType
        {
            get;
        }


        /// <summary>
        /// Creates new instance of <see cref="RegisterCustomCloneSettingsControlAttribute"/>.
        /// </summary>
        /// <param name="objectType">Info object type.</param>
        /// <param name="type">Type</param>
        public RegisterCustomCloneSettingsControlAttribute(string objectType, Type type)
        {
            ObjectType = objectType;
            MarkedType = type;
        }


        /// <summary>
        /// Registers the class in <see cref="CustomCloneSettings"/>.
        /// </summary>
        public void Init()
        {
            CustomCloneSettings.AddOrUpdate(ObjectType, MarkedType);
        }
   }
}
