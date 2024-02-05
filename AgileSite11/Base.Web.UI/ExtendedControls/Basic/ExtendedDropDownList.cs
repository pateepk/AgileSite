using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extends dropdown list.
    /// Adds possibility to save all attributes of Items to ViewState, so for example CSS class of Items persist through PostBack.
    /// </summary>
    [ToolboxData("<{0}:ExtendedDropDownList runat=server />"), Serializable]
    public class ExtendedDropDownList : CMSDropDownList
    {
        #region "Public properties"

        /// <summary>
        /// Allows to save all attributes of inner items to viewstate.
        /// </summary>
        public bool SaveItemAttributesToViewState
        {
            get;
            set;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        ///  Saves the current view state of the System.Web.UI.WebControls.ListControl - derived control and the items it contains.
        /// </summary>
        protected override object SaveViewState()
        {
            object[] allStates = new object[Items.Count + 1];

            // Save base state
            object baseState = base.SaveViewState();
            allStates[0] = baseState;

            if (SaveItemAttributesToViewState)
            {
                int i = 1;
                foreach (ListItem li in Items)
                {
                    int j = 0;
                    string[][] attributes = new string[li.Attributes.Count][];

                    // Save attributes
                    foreach (string attribute in li.Attributes.Keys)
                    {
                        attributes[j++] = new[] { attribute, li.Attributes[attribute] };
                    }
                    allStates[i++] = attributes;
                }
            }
            return allStates;
        }


        /// <summary>
        /// Loads the previously saved view state of the System.Web.UI.WebControls.DetailsView control.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;

                // Restore base state
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                if (SaveItemAttributesToViewState)
                {
                    int i = 1;
                    foreach (ListItem li in Items)
                    {
                        // Restore attributes
                        foreach (string[] attribute in (string[][])myState[i++])
                        {
                            li.Attributes[attribute[0]] = attribute[1];
                        }
                    }
                }
            }
        }

        #endregion
    }
}