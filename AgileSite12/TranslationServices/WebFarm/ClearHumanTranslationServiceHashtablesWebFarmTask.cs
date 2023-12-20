using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.TranslationServices;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Web farm task used to clear human translation service hashtable.
    /// </summary>
    internal class ClearHumanTranslationServiceHashtablesWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Clears human translation service hashtable.
        /// </summary>
        public override void ExecuteTask()
        {
            AbstractHumanTranslationService.ClearHashtables(false);
        }
    }
}
