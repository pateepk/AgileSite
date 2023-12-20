using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Represents the configuration of a widget within the <see cref="Zone.Widgets"/> list.
    /// </summary>
    [DataContract(Namespace = "")]
    public sealed class Widget
    {
        /// <summary>
        /// Identifier of the widget instance.
        /// </summary>
        [DataMember(Order = 0)]
        public Guid Identifier
        {
            get;
            internal set;
        }


        /// <summary>
        /// Identifier of the widget type.
        /// </summary>
        [DataMember(Order = 1)]
        public Guid TypeIdentifier
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates that widget contains at least one unfilled required property.
        /// </summary>
        /// <remarks>Property is not serialized to output XML, if the value is <c>false</c>.</remarks>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public bool HasUnfilledRequiredProperty
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that widget definition was not properly loaded.
        /// </summary>
        /// <remarks>Property is not serialized to output XML.</remarks>
        public bool WidgetDefinitionNotFound
        {
            get;
            set;
        }


        /// <summary>
        /// Widget specific properties.
        /// </summary>
        [DataMember(Order = 3)]
        public List<WidgetProperty> Properties
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an instance of <see cref="Widget"/> class.
        /// </summary>
        /// <param name="identifier">Widget identifier representing the instance.</param>
        /// <param name="typeIdentifier">Widget type identifier.</param>
        public Widget(Guid identifier, Guid typeIdentifier)
        {
            TypeIdentifier = typeIdentifier;
            Identifier = identifier;
            Properties = new List<WidgetProperty>();
        }
    }
}
