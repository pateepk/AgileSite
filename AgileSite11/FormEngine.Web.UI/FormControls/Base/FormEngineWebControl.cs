using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form engine web control.
    /// </summary>
    public abstract class FormEngineWebControl : WebControl, IFormControl
    {
        #region "Properties"

        /// <summary>
        /// Parent form.
        /// </summary>
        public BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Field info object.
        /// </summary>
        public FormFieldInfo FieldInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Gets ClientID of the control from which the Value is retrieved or 
        /// null if such a control can't be specified.
        /// </summary>
        public virtual string ValueElementID
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        public abstract object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Returns value prepared for validation.
        /// </summary>
        public virtual object ValueForValidation
        {
            get
            {
                return Value;
            }
        }

        /// <summary>
        /// Returns true if the control has value, if false, the value from the control should not be used within the form to update the data
        /// </summary>
        public virtual bool HasValue
        {
            get
            {
                return Visible;
            }
        }


        /// <summary>
        /// Gets the display name of the value item. Returns null if display name is not available.
        /// </summary>
        public virtual string ValueDisplayName
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Node data. This property is used only for passing values to the control.
        /// </summary>
        public virtual DataRow DataDR
        {
            get;
            set;
        }


        /// <summary>
        /// Validation error string shown when the control is not valid.
        /// </summary>
        public string ValidationError
        {
            get;
            set;
        }


        /// <summary>
        /// If true, control does not process the data.
        /// </summary>
        public bool StopProcessing
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["StopProcessing"], false);
            }
            set
            {
                ViewState["StopProcessing"] = value;
            }
        }


        /// <summary>
        /// CSS class of the control.
        /// </summary>
        public override string CssClass
        {
            get
            {
                return ValidationHelper.GetString(ViewState["CssClass"], "");
            }
            set
            {
                ViewState["CssClass"] = value;
            }
        }


        /// <summary>
        /// CSS style of the control.
        /// </summary>
        public new string ControlStyle
        {
            get
            {
                return ValidationHelper.GetString(ViewState["ControlStyle"], "");
            }
            set
            {
                ViewState["ControlStyle"] = value;
            }
        }


        /// <summary>
        /// Helper property to use custom parameter in form control.
        /// </summary>
        public virtual object FormControlParameter
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads control value
        /// </summary>
        /// <param name="value">Value to load</param>
        public virtual void LoadControlValue(object value)
        {
            Value = value;
        }


        /// <summary>
        /// Loads the other fields values to the state of the form control
        /// </summary>
        public virtual void LoadOtherValues()
        {
        }


        /// <summary>
        /// Returns an array of values of any other fields returned by the control.
        /// </summary>
        /// <remarks>It returns an array where first dimension is attribute name and the second dimension is its value.</remarks>
        public virtual object[,] GetOtherValues()
        {
            return null;
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public virtual bool IsValid()
        {
            return true;
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public virtual List<string> GetSpellCheckFields()
        {
            return null;
        }


        /// <summary>
        /// Renders user control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // For design mode display control ID
            if (Context == null)
            {
                writer.Write(" [ FormEngineWebControl : " + ID + " ]");
                return;
            }

            // Set user control css class and style
            if (!String.IsNullOrEmpty(CssClass) || !String.IsNullOrEmpty(ControlStyle))
            {
                // Create "div" around the control
                writer.Write("<div");
                if (!String.IsNullOrEmpty(CssClass))
                {
                    writer.Write(" class=\"" + CssClass + "\"");
                }
                if (!String.IsNullOrEmpty(ControlStyle))
                {
                    writer.Write(" style=\"" + ControlStyle + "\"");
                }
                writer.Write(">");

                base.Render(writer);

                writer.Write("</div>");
            }
            else
            {
                base.Render(writer);
            }
        }

        #endregion
    }
}