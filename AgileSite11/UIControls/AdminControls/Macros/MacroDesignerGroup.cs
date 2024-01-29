using System;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for the group of boolean expressions control.
    /// </summary>
    public abstract class MacroDesignerGroup : FormEngineUserControl
    {
        /// <summary>
        /// Gets or sets the current macro structure.
        /// </summary>
        public virtual MacroDesignerTree CurrentGroup
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Returns the condition created by this boolean expression designer.
        /// </summary>
        public virtual string Condition
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        /// <summary>
        /// Returns the operator of the group.
        /// </summary>
        public virtual string GroupOperator
        {
            get
            {
                return "&&";
            }
            set
            {
            }
        }


        /// <summary>
        /// Builds the designer controls structure.
        /// </summary>
        /// <param name="recursive">If true, BuldDesigner is called to child groups as well</param>
        public virtual void BuildDesigner(bool recursive)
        {
            throw new NotImplementedException("[MacroDesignerGroup]: You have to implement this method in the inherited class.");
        }


        /// <summary>
        /// Saves the current data to the MacroDesignerTree object.
        /// </summary>
        public virtual void StoreData()
        {
            throw new NotImplementedException("[MacroDesignerGroup]: You have to implement this method in the inherited class.");
        }
    }
}