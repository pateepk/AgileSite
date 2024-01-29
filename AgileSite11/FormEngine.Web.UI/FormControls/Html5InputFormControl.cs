using System;
using System.Linq;
using System.Web.UI.HtmlControls;

using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class representing HTML5 input element.
    /// </summary>
    public class Html5InputFormControl : FormEngineUserControl
    {
        #region "Variables"

        private HtmlInputControl mInput;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns an input element.
        /// </summary>
        public HtmlInputControl Input  => mInput ?? (mInput = GetControlForInputType(Type));


        /// <summary>
        /// Returns HTML5 input id.
        /// </summary>
        public override string InputClientID => Input.ClientID;


        /// <summary>
        /// Defines alt attribute of the HTML5 input element.
        /// </summary>
        public virtual string Alt
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Alt)), String.Empty); }
            set { SetValue(nameof(Alt), value); }
        }


        /// <summary>
        /// Defines autocomplete attribute of the HTML5 input element.
        /// </summary>
        public virtual string Autocomplete
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Autocomplete)), String.Empty); }
            set { SetValue(nameof(Autocomplete), value); }
        }


        /// <summary>
        /// Defines max attribute of the HTML5 input element.
        /// </summary>
        public virtual string Max
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Max)), String.Empty); }
            set { SetValue(nameof(Max), value); }
        }


        /// <summary>
        /// Defines maxlength attribute of the HTML5 input element.
        /// </summary>
        [Obsolete("Use Maxlength instead.")]
        public virtual int? Maxlenght
        {
            get { return Maxlength; }
            set { Maxlength = value; }
        }


        /// <summary>
        /// Defines maxlength attribute of the HTML5 input element.
        /// </summary>
        public virtual int? Maxlength
        {
            get { return GetNullableIntValue(nameof(Maxlength)) ?? GetNullableIntValue("Maxlenght"); }
            set { SetValue(nameof(Maxlength), value); }
        }


        /// <summary>
        /// Defines min attribute of the HTML5 input element.
        /// </summary>
        public virtual string Min
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Min)), String.Empty); }
            set { SetValue(nameof(Min), value); }
        }


        /// <summary>
        /// Defines minlength attribute of the HTML5 input element.
        /// </summary>
        [Obsolete("Use Minlength instead.")]
        public virtual int? Minlenght
        {
            get { return Minlength; }
            set { Minlength = value; }
        }


        /// <summary>
        /// Defines minlength attribute of the HTML5 input element.
        /// </summary>
        public virtual int? Minlength
        {
            get { return GetNullableIntValue(nameof(Minlength)) ?? GetNullableIntValue("Minlenght"); }
            set { SetValue(nameof(Minlength), value); }
        }


        /// <summary>
        /// Defines multiple attribute of the HTML5 input element.
        /// </summary>
        public virtual bool Multiple
        {
            get { return ValidationHelper.GetBoolean(GetValue(nameof(Multiple)), false); }
            set { SetValue(nameof(Multiple), value); }
        }


        /// <summary>
        /// Defines pattern attribute of the HTML5 input element.
        /// </summary>
        public virtual string Pattern
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Pattern)), String.Empty); }
            set { SetValue(nameof(Pattern), value); }
        }


        /// <summary>
        /// Defines pattern attribute of the HTML5 input element.
        /// </summary>
        public virtual string Placeholder
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Placeholder)), String.Empty); }
            set { SetValue(nameof(Placeholder), value); }
        }


        /// <summary>
        /// Defines readonly attribute of the HTML5 input element.
        /// </summary>
        public virtual bool Readonly
        {
            get { return ValidationHelper.GetBoolean(GetValue(nameof(Readonly)), false); }
            set { SetValue(nameof(Readonly), value); }
        }


        /// <summary>
        /// Defines required attribute of the HTML5 input element.
        /// </summary>
        public virtual bool Required
        {
            get { return ValidationHelper.GetBoolean(GetValue(nameof(Required)), false); }
            set { SetValue(nameof(Required), value); }
        }


        /// <summary>
        /// Defines size attribute of the HTML5 input element.
        /// </summary>
        public virtual int? Size
        {
            get { return GetNullableIntValue(nameof(Size)); }
            set { SetValue(nameof(Size), value); }
        }


        /// <summary>
        /// Defines step attribute of the HTML5 input element.
        /// </summary>
        public virtual string Step
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Step)), String.Empty); }
            set { SetValue(nameof(Step), value); }
        }

        /// <summary>
        /// Defines type attribute of the HTML5 input element.
        /// </summary>
        public virtual string Title
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Title)), String.Empty); }
            set { SetValue(nameof(Title), value); }
        }


        /// <summary>
        /// Defines type attribute of the HTML5 input element.
        /// </summary>
        public virtual string Type
        {
            get { return ValidationHelper.GetString(GetValue(nameof(Type)), "text"); }
            set { SetValue(nameof(Type), value); }
        }


        /// <summary>
        /// Defines custom attributes of the HTML5 input element.
        /// </summary>
        /// <remarks>
        /// String contains every key value pair split by semicolon on the new line.
        /// </remarks>
        /// <example>
        /// <para>key;value</para>
        /// <para>anotherKey;anotherValue</para>
        /// </example>
        public virtual string CustomAttributes
        {
            get { return ValidationHelper.GetString(GetValue(nameof(CustomAttributes)), String.Empty); }
            set { SetValue(nameof(CustomAttributes), value); }
        }


        /// <summary>
        /// Form control value.
        /// </summary>
        public override object Value
        {
            get
            {
                if (IsCheckbox(Type))
                {
                    return (Input as HtmlInputCheckBox)?.Checked ?? false;
                }

                return Input.Value;
            }
            set
            {
                if (IsCheckbox(Type))
                {
                    AddAttribute("checked", ValidationHelper.GetBoolean(value, false));
                    return;
                }

                Input.Value = Convert.ToString(value);
            }
        }


        /// <summary>
        /// Returns true if <param name="type"/> equals to "checkbox".
        /// </summary>
        private bool IsCheckbox(string type)
        {
            return type.Equals("checkbox", StringComparison.OrdinalIgnoreCase);
        }


        #endregion


        #region "Methods"

        /// <summary>
        /// Dynamically creates HTML5 input.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Input.Attributes["type"] = Type;

            Input.ID = "html5input";
            Input.Attributes["class"] = "form-control";

            AddWellKnownAttributes();
            AddCustomAttributes();
            Controls.Add(Input);
        }


        /// <summary>
        /// Returns instance derived from <see cref="HtmlInputControl"/> determined by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">HTML5 input type.</param>
        /// <remarks>
        /// Returned instance is used by <see cref="Input"/> property.
        /// </remarks>
        protected virtual HtmlInputControl GetControlForInputType(string type)
        {
            return IsCheckbox(type) ? (HtmlInputControl)new HtmlInputCheckBox() : new HtmlInputGenericControl();
        }


        /// <summary>
        /// Adds the most commonly used attributes with their values to HTML5 input.
        /// </summary>
        private void AddWellKnownAttributes()
        {
            AddAttribute("autocomplete", Autocomplete);
            AddAttribute("alt", Alt);
            AddAttribute("max", Max);
            AddAttribute("maxlength", Maxlength);
            AddAttribute("min", Min);
            AddAttribute("minlength", Minlength);
            AddAttribute("multiple", Multiple);
            AddAttribute("pattern", Pattern);
            AddAttribute("placeholder", Placeholder);
            AddAttribute("readonly", Readonly);

            // If input is generated somewhere in admin UI (like form builder) then submit btn
            // triggers client-side validation for some unrelated input and submit can not be performed
            if (IsLiveSite)
            {
                AddAttribute("required", Required);
            }

            AddAttribute("size", Size);
            AddAttribute("step", Step);
            AddAttribute("title", Title);
        }


        /// <summary>
        /// Adds attribute with a defined string value to the HTML5 input.
        /// </summary>
        private void AddAttribute(string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                Input.Attributes.Add(name, value);
            }
        }


        /// <summary>
        /// Adds attribute with a defined integer value to the HTML5 input.
        /// </summary>
        private void AddAttribute(string name, int? value)
        {
            if (value != null)
            {
                Input.Attributes.Add(name, value.ToString());
            }
        }


        /// <summary>
        /// Adds attribute with a defined boolean value to the HTML5 input.
        /// </summary>
        private void AddAttribute(string name, bool value)
        {
            if (value)
            {
                Input.Attributes.Add(name, name);
            }
        }


        /// <summary>
        /// Adds custom attributes to the HTML5 input.
        /// </summary>
        private void AddCustomAttributes()
        {
            var attributes = CustomAttributes.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line =>
                {
                    var keyValuePair = line.Split(new[] { ';' }, 2);
                    return new { Key = keyValuePair[0], Value = keyValuePair[1] };
                });

            foreach (var attribute in attributes)
            {
                Input.Attributes.Add(attribute.Key, ContextResolver.ResolveMacros(attribute.Value));
            }
        }


        /// <summary>
        /// Returns integer value of the form control property.
        /// If the value can not be parsed then null is returned.
        /// </summary>
        private int? GetNullableIntValue(string propertyName)
        {
            int result;

            if (int.TryParse((GetValue(propertyName))?.ToString(), out result))
            {
                return result;
            }

            return null;
        }

        #endregion
    }
}
