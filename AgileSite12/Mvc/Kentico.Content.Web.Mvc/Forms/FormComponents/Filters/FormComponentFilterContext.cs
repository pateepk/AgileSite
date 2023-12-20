using System;

using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents the form component filter context.
    /// </summary>
    public class FormComponentFilterContext
    {
        /// <summary>
        /// Gets the form for form component filtering.
        /// </summary>
        public BizFormInfo Form { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="FormComponentFilterContext"/>.
        /// </summary>
        /// <param name="form">Form for form component filtering.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="form"/> is null.</exception>
        internal FormComponentFilterContext(BizFormInfo form)
        {
            Form = form ?? throw new ArgumentNullException(nameof(form));
        }
    }
}
