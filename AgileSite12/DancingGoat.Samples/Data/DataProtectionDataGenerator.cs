using System;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.FormEngine;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.PortalEngine;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Sample data protection data generator providing sample data for Dancing Goat demo site.
    /// </summary>
    public sealed class DataProtectionDataGenerator : ISampleDataGenerator
    {
        private readonly Guid COOKIE_LAW_WEBPART_INSTANCE_GUID = Guid.Parse("291bc8ee-03b1-4391-88af-f43ce0244434");
        private const string THIS_CONSTANT_IS_DEDICATED_TO_DES_WITH_PURE_LOVE_TEMPLATE_CODENAME_BTW = "ba51c614-23d1-4299-80f3-03152f2efe9a";

        private const string DATA_PROTECTION_SETTINGS_KEY = "DataProtectionSamplesEnabled";

        internal const string TRACKING_CONSENT_NAME = "DancingGoatTracking";
        private const string TRACKING_CONSENT_DISPLAY_NAME = "Dancing Goat - Tracking";
        private const string TRACKING_CONSENT_EN = @"At Dancing Goat, we have exciting offers and news about our products and services 
                that we hope you'd like to hear about. To present you the offers that suit you the most, we need to know a few personal 
                details about you. We will gather some of your activities on our website (such as which pages you've visited, etc.) and 
                use them to personalize the website content and improve analytics about our visitors. In addition, we will store small 
                piecies of data in your browser cookies. We promise we will treat your data with respect, store it in a secured storage, 
                and won't release it to any third parties.";

        private const string TRACKING_CONSENT_ES = @"En Dancing Goat, tenemos noticias y ofertas interesantes sobre nuestros productos y 
                servicios de los que esperamos que le gustaría escuchar. Para presentarle las ofertas que más le convengan, necesitamos 
                conocer algunos detalles personales sobre usted. Reuniremos algunas de sus actividades en nuestro sitio web (como las 
                páginas que visitó, etc.) y las usaremos para personalizar el contenido del sitio web y mejorar el análisis de nuestros 
                visitantes. Además, almacenaremos pequeñas cantidades de datos en las cookies del navegador. Nos comprometemos a tratar 
                sus datos con respeto, almacenarlo en un almacenamiento seguro, y no lo lanzará a terceros.";

        private const string FORM_CLASS_NAME = "BizForm.MachineRental";
        private const string FORM_CONSENT_NAME = "DancingGoatMachineRentalForm";
        private const string FORM_CONSENT_DISPLAY_NAME = "Dancing Goat - Machine rental form";
        private const string FORM_CONSENT_EN = "I hereby accept that these provided information can be used for marketing purposes and targeted website content.";
        private const string FORM_CONSENT_ES = "Por lo presente acepto que esta información proporcionada puede ser utilizada con fines de marketing y contenido de sitios web dirigidos.";
        private const string CONSENT_LONG_TEXT_EN = @"This is a sample consent declaration used for demonstration purposes only. 
                We strongly recommend forming a consent declaration suited for your website and consulting it with a lawyer.";
        private const string CONSENT_LONG_TEXT_ES = @"Esta es una declaración de consentimiento de muestra que se usa sólo para fines de demostración.
                Recomendamos encarecidamente formar una declaración de consentimiento adecuada para su sitio web y consultarla con un abogado.";

        private const string FORM_CONSENT_CONTACT_GROUP_DISPLAY_NAME = "Machine rental applicants";


        /// <summary>
        /// Generates sample data protection data. Suitable only for Dancing Goat demo site.
        /// </summary>
        /// <param name="siteID">ID of the site to generate sample data for.</param>
        public void Generate(int siteID)
        {
            EnableSamples();

            CreateFormConsent();
            UpdateMachineRentalForm(siteID);

            CreateContactGroupWithFormConsentAgreementRule();

            SetDefaultCookieLevel(siteID);
            CreateTrackingConsent();
            SetConsentToCookieLawWebPart(siteID);

            CreateConsentsWebPart();
        }


        private void CreateConsentsWebPart()
        {
            const string webPartName = "SampleDancingGoatConsents";
            const string webPartDisplayName = "Sample Dancing Goat - Consents";
            const string webPartFilePath = "DancingGoat.Samples/DancingGoatConsents.ascx";

            SampleWebPartsGenerator.EnsureWebpart(webPartName, webPartDisplayName, webPartFilePath);
        }


        private void SetConsentToCookieLawWebPart(int siteId)
        {
            var template = PageTemplateInfoProvider.GetPageTemplateInfo(THIS_CONSTANT_IS_DEDICATED_TO_DES_WITH_PURE_LOVE_TEMPLATE_CODENAME_BTW, siteId);
            if (template == null)
            {
                return;
            }

            var webPart = template.TemplateInstance.GetWebPart(COOKIE_LAW_WEBPART_INSTANCE_GUID);
            if (webPart == null)
            {
                return;
            }

            webPart["TrackingConsent"] = TRACKING_CONSENT_NAME;
            template.Update();
        }


        private void EnableSamples()
        {
            var keyInfo = new SettingsKeyInfo
            {
                KeyName = DATA_PROTECTION_SETTINGS_KEY,
                KeyDisplayName = DATA_PROTECTION_SETTINGS_KEY,
                KeyType = "boolean",
                KeyValue = "True",
                KeyDefaultValue = "False",
                KeyIsGlobal = true,
                KeyIsHidden = true
            };

            SettingsKeyInfoProvider.SetSettingsKeyInfo(keyInfo);
        }


        private void UpdateMachineRentalForm(int siteId)
        {
            UpdateForm();
            UpdateAlternativeForm();
            AddFormListingField(siteId);
        }


        private static void AddFormListingField(int siteId)
        {
            var form = BizFormInfoProvider.GetBizFormInfo("MachineRental", siteId);
            if (form == null)
            {
                return;
            }

            form.FormReportFields += ";Consent";
            BizFormInfoProvider.SetBizFormInfo(form);
        }


        private static void UpdateForm()
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(FORM_CLASS_NAME);
            if (classInfo == null)
            {
                return;
            }

            var form = new FormInfo(classInfo.ClassFormDefinition);
            var field = CreateFormField();

            form.AddFormItem(field);

            classInfo.ClassFormDefinition = form.GetXmlDefinition();
            classInfo.Update();
        }


        private static void UpdateAlternativeForm()
        {
            var alternativeForm = AlternativeFormInfoProvider.GetAlternativeFormInfo($"{FORM_CLASS_NAME}.MachineRental");
            if (alternativeForm == null)
            {
                return;
            }

            alternativeForm.FormLayout += "\r\n<div class=\"form-group\">\r\n<div class=\"form-group-label\">$$label:Consent$$</div>\r\n\r\n<div class=\"form-group-input\">$$input:Consent$$</div>\r\n\r\n<div class=\"message message-error\">$$validation:Consent$$</div>\r\n</div>\r\n";
            alternativeForm.Update();
        }


        private static FormFieldInfo CreateFormField()
        {
            var field = new FormFieldInfo
            {
                Name = "Consent",
                DataType = FieldDataType.Guid,
                FieldType = FormFieldControlTypeEnum.CustomUserControl,
                System = false,
                Visible = true,
                PublicField = true,
                AllowEmpty = true
            };

            field.Settings["controlname"] = "ConsentAgreement";
            field.Settings["Consent"] = FORM_CONSENT_NAME;
            field.SetPropertyValue(FormFieldPropertyEnum.FieldCaption, String.Empty);
            field.SetPropertyValue(FormFieldPropertyEnum.DefaultValue, String.Empty);
            field.SetPropertyValue(FormFieldPropertyEnum.FieldDescription, String.Empty);

            return field;
        }


        private static void CreateFormConsent()
        {
            CreateConsent(FORM_CONSENT_NAME, FORM_CONSENT_DISPLAY_NAME, FORM_CONSENT_EN, FORM_CONSENT_ES);
        }


        /// <summary>
        /// Indicates if data protection samples are enabled.
        /// </summary>
        public static bool AreSamplesEnabled()
        {
            return SettingsKeyInfoProvider.GetSettingsKeyInfo(DATA_PROTECTION_SETTINGS_KEY) != null;
        }


        private static void CreateTrackingConsent()
        {
            CreateConsent(TRACKING_CONSENT_NAME, TRACKING_CONSENT_DISPLAY_NAME, TRACKING_CONSENT_EN, TRACKING_CONSENT_ES);
        }


        private static void CreateConsent(string name, string displayName, string shortTextEn, string shortTextEs)
        {
            var consent = new ConsentInfo
            {
                ConsentName = name,
                ConsentDisplayName = displayName,
            };

            consent.UpsertConsentText("en-US", shortTextEn, CONSENT_LONG_TEXT_EN);
            consent.UpsertConsentText("es-ES", shortTextEs, CONSENT_LONG_TEXT_ES);

            ConsentInfoProvider.SetConsentInfo(consent);
        }


        private static void CreateContactGroupWithFormConsentAgreementRule()
        {
            var rule = CreateContactGroupRule();

            var contactGroup = new ContactGroupInfo
            {
                ContactGroupDisplayName = FORM_CONSENT_CONTACT_GROUP_DISPLAY_NAME,
                ContactGroupName = InfoHelper.CODENAME_AUTOMATIC,
                ContactGroupDynamicCondition = rule,
                ContactGroupEnabled = true
            };

            ContactGroupInfoProvider.SetContactGroupInfo(contactGroup);
        }


        private static string CreateContactGroupRule()
        {
            var rule = $@"{{%Rule(""(Contact.AgreedWithConsent(""{FORM_CONSENT_NAME}""))"", "" <rules><r pos =\""0\"" par=\""\"" op=\""and\"" n=\""CMSContactHasAgreedWithConsent\"" >
                        <p n=\""consent\""><t>{FORM_CONSENT_DISPLAY_NAME}</t><v>{FORM_CONSENT_NAME}</v><r>0</r><d>select consent</d><vt>text</vt><tv>0</tv></p>
                        <p n=\""_perfectum\""><t>has</t><v></v><r>0</r><d>select operation</d><vt>text</vt><tv>0</tv></p></r></rules>"")%}}";

            return MacroSecurityProcessor.AddSecurityParameters(rule, MacroIdentityOption.FromUserInfo(UserInfoProvider.AdministratorUser), null);
        }


        private void SetDefaultCookieLevel(int siteID)
        {
            SettingsKeyInfoProvider.SetValue("CMSDefaultCookieLevel", siteID, "essential");
        }
    }
}
