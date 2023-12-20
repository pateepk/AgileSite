using CMS.Base;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Container to wrap the form control structure
    /// </summary>
    public class FormControlMacroContainer : ReadOnlyAbstractHierarchicalObject<FormControlMacroContainer>, IMacroObject
    {
        #region "Variables"

        private readonly FormEngineUserControl mControl;
        private object mValue;
        private bool mEnabled;
        private bool mVisible;
        private FormFieldInfo mInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Control value
        /// </summary>
        public object Value
        {
            get
            {
                return (mControl != null) ? mControl.Value : mValue;
            }
            set
            {
                mValue = value;
            }
        }


        /// <summary>
        /// Indicates that control is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (mControl != null) ? mControl.Enabled : mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Indicates that control is visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return (mControl != null) ? mControl.Visible : mVisible;
            }
            set
            {
                mVisible = value;
            }
        }


        /// <summary>
        /// Corresponding field info
        /// </summary>
        public FormFieldInfo Info
        {
            get
            {
                if ((mControl != null) && (mControl.FieldInfo != null))
                {
                    return mControl.FieldInfo;
                }

                return mInfo ?? (mInfo = new FormFieldInfo());
            }
            set
            {
                mInfo = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public FormControlMacroContainer()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Wrapped form control</param>
        public FormControlMacroContainer(FormEngineUserControl control)
        {
            mControl = control;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<object>("Value", f => f.Value);
            RegisterProperty<bool>("Enabled", f => f.Enabled);
            RegisterProperty<bool>("Visible", f => f.Visible);
            RegisterProperty<FormFieldInfo>("Info", f => f.Info);
        }


        /// <summary>
        /// Gets the context property.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Returning the value</param>
        public override bool TryGetProperty(string name, out object value)
        {
            var returnValue = base.TryGetProperty(name, out value);

            if (!returnValue)
            {
                if (mControl != null)
                {
                    value = mControl.GetValue(name);
                    return (value != null);
                }
            }

            return returnValue;
        }


        /// <summary>
        /// Returns a <see cref="T:System.String"/> of current Value.
        /// </summary>
        /// <returns><see cref="T:System.String"/> of current Value</returns>
        public override string ToString()
        {
            return ValidationHelper.GetString(Value, null);
        }

        #endregion


        #region "IMacroObject Members"

        /// <summary>
        /// Returns Value to string.
        /// </summary>
        public string ToMacroString()
        {
            return ValidationHelper.GetString(Value, "");
        }

        /// <summary>
        /// Returns Value.
        /// </summary>
        public object MacroRepresentation()
        {
            return Value;
        }

        #endregion
    }
}