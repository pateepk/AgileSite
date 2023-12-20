using System;
using System.Collections.Generic;

namespace CMS.UIControls
{
    /// <summary>
    /// Delegate defining method which is used to create URL of the mass action which will be later used to redirect page or open modal dialog.
    /// </summary>
    /// <param name="actionScope">All/Selected items</param>
    /// <param name="selectedItems">IDs of the selected items. Null if scope All is passed</param>
    /// <param name="additionalParameters">Additional </param>
    /// <returns></returns>
    public delegate string CreateUrlDelegate(MassActionScopeEnum actionScope, List<int> selectedItems, object additionalParameters);


    /// <summary>
    /// Represents single mass action displayed in the mass action drop-down list.
    /// </summary>
    public class MassActionItem
    {
        /// <summary>
        /// Resource string of label which will be displayed in the drop-down list.
        /// </summary>
        public string DisplayNameResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// Code name of action used for its unique identification.
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Action which should be performed when current mass action is selected.
        /// </summary>
        public MassActionTypeEnum ActionType
        {
            get;
            set;
        }


        /// <summary>
        /// Declares method for creating URL the action will use for its computations.
        /// </summary>
        public CreateUrlDelegate CreateUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Converts the value of this instance to its equivalent string representation. 
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", CodeName, ActionType, DisplayNameResourceString);
        }
    }
}
