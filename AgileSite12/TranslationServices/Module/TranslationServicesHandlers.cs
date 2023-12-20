using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Event handlers for translation services module
    /// </summary>
    internal class TranslationServicesHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += ClearServiceSettings;
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        private static void ClearServiceSettings(object sender, SettingsKeyChangedEventArgs e)
        {
            // Clear the cached translation service settings
            switch (e.KeyName.ToLowerCSafe())
            {
                case "cmstranslationscomurl":
                case "cmstranslationscomusername":
                case "cmstranslationscompassword":
                case "cmstranslationscomprojectcode":
                    AbstractHumanTranslationService.ClearHashtables(true);
                    break;


                case "cmsmstranslatorclientsecret":
                case "cmsmstranslatorclientid":
                case "cmsgoogletranslateapikey":
                    AbstractMachineTranslationService.ClearHashtables(true);
                    break;
            }
        }
    }
}
