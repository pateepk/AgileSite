using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents one line from the macro rule condition builder.
    /// </summary>
    public class MacroRuleInstance
    {
        /// <summary>
        /// Codename of the rule.
        /// </summary>
        public string MacroRuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Parameters that a user filled in an instance of a macro rule.
        /// </summary>
        public StringSafeDictionary<MacroRuleParameter> Parameters
        {
            get;
            set;
        }
    }
}
