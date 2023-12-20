using System;

using CMS;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Registers definition of Form builder feature component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class RegisterComponentAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Type of the registered component controller.
        /// </summary>
        public Type MarkedType { get; protected set; }


        /// <summary>
        /// Unique identifier of the definition.
        /// </summary>
        public string Identifier { get; }


        /// <summary>
        /// Name of the registered component.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterComponentAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the Form Builder definition.</param>
        /// <param name="controllerType">Type of the controller to register.</param>
        /// <param name="name">Name of the registered component.</param>
        protected RegisterComponentAttribute(string identifier, Type controllerType, string name)
        {
            Identifier = identifier;
            MarkedType = controllerType;
            Name = name;
        }


        /// <summary>
        /// Registers the Form Builder definition during application pre-initialization.
        /// </summary>
        public abstract void PreInit();
    }
}
