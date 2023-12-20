using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Components.Web.Mvc.Dialogs.Internal;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.FormComponents.Internal
{
    /// <summary>
    /// Represents a base class for the page selector form components.
    /// </summary>
    public abstract class PageSelectorBase<TProperties, TValue> : FormComponent<TProperties, IList<TValue>>
        where TProperties : PageSelectorPropertiesBase<TValue>, new()
    {
        private string mValue;
        private IEnumerable<TValue> mPageIdentifiers;


        /// <summary>
        /// Gets the serialized JSON model of the selected page.
        /// </summary>
        public string SelectedPageModel
        {
            get
            {
                if (PageIdentifiers.Any())
                {
                    var page = GetPage(PageIdentifiers.FirstOrDefault());
                    var model = PageSelectorItemModel.Create(page, new UrlHelper(HttpContext.Current.Request.RequestContext));

                    return JsonConvert.SerializeObject(model);
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the dialog root path.
        /// </summary>
        public string RootPath => Properties.RootPath;


        /// <summary>
        /// Gets the deserialized selected page identifiers.
        /// </summary>
        private IEnumerable<TValue> PageIdentifiers
        {
            get
            {
                if (mPageIdentifiers == null)
                {
                    mPageIdentifiers = !String.IsNullOrEmpty(Value) ? JsonConvert.DeserializeObject<IList<TValue>>(Value) : Enumerable.Empty<TValue>();
                }

                return mPageIdentifiers;
            }
        }


        /// <summary>
        /// Represents the input value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mPageIdentifiers = null;
                mValue = value;
            }
        }


        /// <summary>
        /// Binds contextual information to the form component.
        /// </summary>
        /// <param name="context">Form component context.</param>
        public override void BindContext(FormComponentContext context)
        {
            base.BindContext(context);

            if (!(context is PageBuilderFormComponentContext))
            {
                throw new NotSupportedException("Page and path selectors are only available in page builder.");
            }
        }


        /// <summary>
        /// Validates whether selected page is valid.
        /// </summary>        
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = base.Validate(validationContext);

            // Some page has been selected
            if (PageIdentifiers.Any())
            {
                var page = GetPage(PageIdentifiers.FirstOrDefault());
                if (page == null)
                {
                    // Selected page will be NULL for users with insufficient permissions or when the page has been deleted
                    var materializedResult = results.ToList();
                    materializedResult.Add(new ValidationResult(ResHelper.GetString("kentico.components.pageselector.invalidvalue.validationtext")));

                    return materializedResult;
                }
            }

            return results;
        }


        /// <summary>
        /// Gets the <see cref="Value"/>.
        /// </summary>
        public override IList<TValue> GetValue()
        {
            if (PageIdentifiers.Any())
            {
                return PageIdentifiers as IList<TValue>;
            }

            return null;
        }


        /// <summary>
        /// Sets the <see cref="Value"/>.
        /// </summary>
        public override void SetValue(IList<TValue> value)
        {
            Value = (value != null) ? JsonConvert.SerializeObject(value) : null;
        }


        /// <summary>
        /// Gets the page specified by the <paramref name="pageIdentifier"/>.
        /// </summary>
        abstract internal TreeNode GetPage(TValue pageIdentifier);
    }
}
