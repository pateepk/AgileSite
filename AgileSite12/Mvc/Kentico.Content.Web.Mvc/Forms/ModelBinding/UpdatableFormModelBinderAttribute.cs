using System;
using System.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Annotate a parameter of a controller action with this attribute to use <see cref="UpdatableFormModelBinder"/>
    /// for its value binding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class UpdatableFormModelBinderAttribute : CustomModelBinderAttribute
    {
        /// <summary>
        /// Retrieves an instance of <see cref="UpdatableFormModelBinder"/>.
        /// </summary>
        /// <returns>An instance of <see cref="UpdatableFormModelBinder"/>.</returns>
        public override IModelBinder GetBinder()
        {
            return new UpdatableFormModelBinder();
        }
    }
}
