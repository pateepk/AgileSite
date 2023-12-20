using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Describes configuration of the form control used to pick the object related to A/B test conversion type.
    /// </summary>
    /// <seealso cref="ABTestConversionDefinition"/>.
    public sealed class ABTestFormControlDefinition
    {
        /// <summary>
        /// Type of the form control used to pick the object related to conversion type.
        /// </summary>
        public Type FormControlType
        {
            get;
        }


        /// <summary>
        /// Name of the form control used to pick the object related to conversion type.
        /// </summary>
        public string FormControlName
        {
            get;
        }


        /// <summary>
        /// Parameters which allow customization of form control defined by <see cref="FormControlName"/>.
        /// </summary>
        public Dictionary<string, object> FormControlParameters
        {
            get;
        }


        /// <summary>
        /// Caption shown above the form control used to pick the object related to conversion type.
        /// </summary>
        /// <seealso cref="FormControlName"/>.
        public string FormControlCaption
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestFormControlDefinition"/>.
        /// </summary>
        /// <param name="formControlName">Name of the form control used to pick the object related to conversion type.</param>
        /// <param name="formControlCaption">Caption shown as label above the form control used to pick the object related to conversion type.</param>
        /// <param name="formControlParameters">Parameters which allow customization of form control defined by <paramref name="formControlName"/>.</param>
        public ABTestFormControlDefinition(string formControlName, string formControlCaption, IDictionary<string, object> formControlParameters = null)
        {
            if (String.IsNullOrEmpty(formControlName))
            {
                throw new ArgumentException("Form control name cannot be empty.", nameof(formControlName));
            }

            if (String.IsNullOrEmpty(formControlCaption))
            {
                throw new ArgumentException("Form control caption cannot be empty.", nameof(formControlCaption));
            }

            FormControlName = formControlName;
            FormControlCaption = formControlCaption;

            if (formControlParameters != null)
            {
                FormControlParameters = new Dictionary<string, object>(formControlParameters);
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestFormControlDefinition"/>.
        /// </summary>
        /// <param name="formControlType">Type of the form control used to pick the object related to conversion type.</param>
        /// <param name="formControlCaption">Caption shown as label above the form control used to pick the object related to conversion type.</param>
        /// <param name="formControlParameters">Parameters which allow customization of form control defined by <paramref name="formControlType"/>.</param>
        public ABTestFormControlDefinition(Type formControlType, string formControlCaption, IDictionary<string, object> formControlParameters = null)
        {
            if (formControlType == null)
            {
                throw new ArgumentNullException(nameof(formControlType));
            }

            var baseTypes = ClassHelper.GetBaseTypes(formControlType);
            if (!baseTypes.Select(i => i.FullName).Contains("CMS.FormEngine.Web.UI.FormEngineUserControl"))
            {
                throw new ArgumentException("Type has to be derived from CMS.FormEngine.Web.UI.FormEngineUserControl.", nameof(formControlType));
            }

            if (String.IsNullOrEmpty(formControlCaption))
            {
                throw new ArgumentException("Form control caption cannot be empty.", nameof(formControlCaption));
            }

            FormControlType = formControlType;
            FormControlCaption = formControlCaption;

            if (formControlParameters != null)
            {
                FormControlParameters = new Dictionary<string, object>(formControlParameters);
            }
        }
    }
}
