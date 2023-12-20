using System;
using System.Threading;

using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="ConsentAgreementComponent"/>.
    /// </summary>
    public class ConsentAgreementProperties : FormComponentProperties<Guid?>
    {
        private ConsentInfo mConsentInfo;


        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentAgreementProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Guid"/>.
        /// </remarks>
        public ConsentAgreementProperties() : base(FieldDataType.Guid)
        {
        }


        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        public override Guid? DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating whether the underlying field is required. False by default.
        /// If false, the form component's implementation must accept nullable input.
        /// </summary>
        public override bool Required
        {
            get;
            set;
        }


        /// <summary>
        /// Text of selected <see cref="ConsentInfo"/> to be displayed in view.
        /// </summary>
        public string ConsentText
        {
            get
            {
                if (ConsentInfo == null)
                {
                    return ResHelper.GetString("kentico.formbuilder.component.consentagreement.properties.consentnotavailable");
                }

                var cultureCode = Thread.CurrentThread.CurrentUICulture.ToString();

                return ConsentInfo?.GetConsentText(cultureCode).ShortText;
            }
        }


        /// <summary>
        /// Returns <see cref="ConsentInfo"/> selected by <see cref="ConsentCodeName"/>.
        /// </summary>
        public ConsentInfo ConsentInfo
        {
            get
            {
                if (mConsentInfo != null)
                {
                    return mConsentInfo;
                }

                return mConsentInfo = ConsentInfoProvider.GetConsentInfo(ConsentCodeName);
            }
        }


        /// <summary>
        /// Represents text of selected consent.
        /// </summary>
        [EditingComponent(ConsentSelectorComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.consentagreement.properties.consenttext$}")]
        public string ConsentCodeName
        {
            get;
            set;
        }
    }
}
