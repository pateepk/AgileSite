﻿using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DancingGoat.Samples;
using CMS.DataEngine;
using CMS.DataProtection;
using CMS.Helpers;

[assembly: RegisterModule(typeof(DancingGoatSamplesModule))]

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Represents module with DataProtection sample code.
    /// </summary>
    public class DancingGoatSamplesModule : Module
    {
        private const string DATA_PROTECTION_SAMPLES_ENABLED_SETTINGS_KEY_NAME = "DataProtectionSamplesEnabled";


        /// <summary>
        /// Initializes a new instance of the <see cref="DancingGoatSamplesModule"/> class.
        /// </summary>
        public DancingGoatSamplesModule()
            : base("CMS.DancingGoat.Samples")
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitializeSamples();
        }


        /// <summary>
        /// Registers sample personal data collectors immediately or attaches an event handler to register the collectors upon dedicated key insertion.
        /// Disabling or toggling registration of the sample collectors is not supported.
        /// </summary>
        private void InitializeSamples()
        {
            var dataProtectionSamplesEnabledSettingsKey = SettingsKeyInfoProvider.GetSettingsKeyInfo(DATA_PROTECTION_SAMPLES_ENABLED_SETTINGS_KEY_NAME);
            if (dataProtectionSamplesEnabledSettingsKey?.KeyValue.ToBoolean(false) ?? false)
            {
                RegisterSamples();
            }
            else
            {
                SettingsKeyInfoProvider.OnSettingsKeyChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.KeyName.Equals(DATA_PROTECTION_SAMPLES_ENABLED_SETTINGS_KEY_NAME, StringComparison.OrdinalIgnoreCase) && 
                        (eventArgs.Action == SettingsKeyActionEnum.Insert) && eventArgs.KeyValue.ToBoolean(false))
                    {
                        RegisterSamples();
                    }
                };
            }
        }


        private void RegisterSamples()
        {
            IdentityCollectorRegister.Instance.Add(new SampleContactInfoIdentityCollector());
            IdentityCollectorRegister.Instance.Add(new SampleCustomerInfoIdentityCollector());

            PersonalDataCollectorRegister.Instance.Add(new SampleContactDataCollector());
            PersonalDataCollectorRegister.Instance.Add(new SampleCustomerDataCollector());

            PersonalDataEraserRegister.Instance.Add(new SampleContactPersonalDataEraser());
            PersonalDataEraserRegister.Instance.Add(new SampleCustomerPersonalDataEraser());

            RegisterConsentRevokeHandler();
        }


        private void RegisterConsentRevokeHandler()
        {
            DataProtectionEvents.RevokeConsentAgreement.Execute += (sender, args) =>
            {
                if (args.Consent.ConsentName.Equals(DataProtectionDataGenerator.TRACKING_CONSENT_NAME, StringComparison.Ordinal))
                {
                    // Delete contact activities
                    var configuration = new Dictionary<string, object>
                    {
                        { "deleteActivities", true }
                    };

                    new SampleContactPersonalDataEraser().Erase(new[] { args.Contact }, configuration);

                    // Set the cookie level to default
                    var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();
                    cookieLevelProvider.SetCurrentCookieLevel(cookieLevelProvider.GetDefaultCookieLevel());
                }
            };
        }
    }
}