using System;

using Kentico.Forms.Web.Mvc;

[assembly: RegisterFormVisibilityCondition(NeverVisibleVisibilityCondition.IDENTIFIER, typeof(NeverVisibleVisibilityCondition), "Never")]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Form component visibility condition which hides the form component.
    /// </summary>
    [Serializable]
    public class NeverVisibleVisibilityCondition : VisibilityCondition, IDefaultVisibilityCondition
    {
        /// <summary>
        /// Identifier used for registration of this condition.
        /// </summary>
        /// <remarks>
        /// This constant is used in the FormBuilder's client side scripts.
        /// </remarks>
        public const string IDENTIFIER = "Kentico.NeverVisible";


        /// <summary>
        /// Gets a value indicating whether a form component is visible.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public override bool IsVisible()
        {
            return false;
        }
    }
}
