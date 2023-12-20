using System;
using System.Web.Mvc;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterImplementation(typeof(IEditablePropertiesModelBinder), typeof(EditablePropertiesModelBinder), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines methods for binding form collection to a model.
    /// </summary>
    internal interface IEditablePropertiesModelBinder
    {
        /// <summary>
        /// Binds data from <paramref name="formCollection"/> to given <paramref name="model"/>, where <paramref name="formComponentInstancePrefix"/> is common prefix of <paramref name="formCollection"/>'s data keys that should be bound.
        /// </summary>
        /// <param name="controllerContext">Controller context to whose model state validation errors are added.</param>
        /// <param name="formComponentContext">Contextual information specifying where the form components representing the <paramref name="model"/> are being used.</param>
        /// <param name="model">Instance to which bind data from <paramref name="formCollection"/>.</param>
        /// <param name="formCollection">Form collection from which to bind data to given <paramref name="model"/>.</param>
        /// <param name="formComponentInstancePrefix">Form identifier used as common prefix of <paramref name="formCollection"/>'s data keys that should be bound.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> or <paramref name="formCollection"/> is null.</exception>
        void BindModel(ControllerContext controllerContext, FormComponentContext formComponentContext, object model, FormCollection formCollection, string formComponentInstancePrefix);
    }
}