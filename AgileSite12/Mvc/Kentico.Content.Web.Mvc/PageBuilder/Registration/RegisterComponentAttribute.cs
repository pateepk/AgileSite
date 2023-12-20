using System;

using CMS;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Registers definition of Page builder feature component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class RegisterComponentAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Type of the registered component controller.
        /// </summary>
        public Type MarkedType { get; protected set; }


        /// <summary>
        /// Unique identifier of the component definition.
        /// </summary>
        public string Identifier { get; }


        /// <summary>
        /// Name of the registered component.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Description of the registered component.
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Font-icon CSS class of the registered component.
        /// </summary>
        public string IconClass { get; set; }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterComponentAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the component definition.</param>
        /// <param name="controllerType">Type of the component controller to register.</param>
        /// <param name="name">Name of the registered component.</param>       
        protected RegisterComponentAttribute(string identifier, Type controllerType, string name)
        {
            Identifier = identifier;
            MarkedType = controllerType;

            Name = name;
        }


        /// <summary>
        /// Registers the component definition during application pre-initialization.
        /// </summary>
        public abstract void PreInit();
    }
}
