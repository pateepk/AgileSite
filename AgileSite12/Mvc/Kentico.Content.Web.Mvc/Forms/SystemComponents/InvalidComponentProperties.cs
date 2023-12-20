using System;

using CMS.DataEngine;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of an <see cref="InvalidComponent"/>.
    /// </summary>
    public class InvalidComponentProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Gets or sets the default value of the form component.
        /// </summary>
        public override string DefaultValue { get; set; }


        /// <summary>
        /// Gets or sets the exception that is related to the original component.
        /// </summary>
        [JsonIgnore]
        public Exception Exception { get; set; }


        /// <summary>
        /// Gets or sets the message describing the error of the original component.
        /// </summary>
        public string ErrorMessage { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidComponentProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The component is not intended to be serialized into the database.
        /// </remarks>
        public InvalidComponentProperties()
            : base(FieldDataType.Unknown)
        {
        }
    }
}