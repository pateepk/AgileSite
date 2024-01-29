using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Localization
{
    /// <summary>
    /// Action context uses for localization methods
    /// </summary>
    public class LocalizationActionContext : AbstractActionContext<LocalizationActionContext>
    {
        private bool? mResolveSubstitutionMacros;


        /// <summary>
        /// Sets the value that indicates whether substitution macros should be resolved in resource strings
        /// </summary>
        public bool ResolveSubstitutionMacros 
        {
            set
            {
                // Keep current settings
                StoreOriginalValue(ref OriginalData.mResolveSubstitutionMacros, CurrentResolveSubstitutionMacros);

                // Ensure requested settings
                CurrentResolveSubstitutionMacros = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether substitution macros should be resolved in resource strings
        /// </summary>
        public static bool CurrentResolveSubstitutionMacros
        {
            get
            {
                return Current.mResolveSubstitutionMacros ?? true;
            }
            private set
            {
                Current.mResolveSubstitutionMacros = value;
            }
        }


        /// <summary>
        /// Restores the original values to the context
        /// </summary>
        protected override void RestoreOriginalValues()
        {
            // Restore current data context
            var o = OriginalData;

            if (o.mResolveSubstitutionMacros.HasValue)
            {
                CurrentResolveSubstitutionMacros = o.mResolveSubstitutionMacros.Value;
            }

            base.RestoreOriginalValues();
        }

    }
}
