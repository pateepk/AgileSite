using System.Runtime.Serialization;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Represents single property of a <see cref="Widget"/> within the <see cref="Zone.Widgets"/> list.
    /// </summary>
    [DataContract(Namespace = "", Name = "Property")]
    public sealed class WidgetProperty
    {
        /// <summary>
        /// Name of the <see cref="Widget"/> property.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Value of the <see cref="Widget"/> property.
        /// </summary>
        [DataMember]
        public string Value
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetProperty"/> class.
        /// </summary>
        /// <param name="name">Name of the widget property.</param>
        /// <param name="value">Value of the widget property.</param>
        public WidgetProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}