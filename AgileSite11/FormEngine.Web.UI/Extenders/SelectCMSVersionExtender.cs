using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Extender for SelectCMSVersion form control. 
    /// </summary>
    /// <remarks>Loads data and checks whether any value is selected</remarks>
    public class SelectCMSVersionExtender : ControlExtender<ListFormControl>
    {
        /// <summary>
        /// Control initialization
        /// </summary>
        public override void OnInit()
        {
            Control.Init += Control_Init;

            Control.SetValue("options", String.Join(Environment.NewLine, GetListItems()));
        }


        private IEnumerable<string> GetListItems()
        {
            var items = new[] { "3.0", "3.1", "3.1a", "4.0", "4.1", "5.0", "5.5", "5.5R2", "6.0", "7.0", "8.0", "8.1", "8.2", "9.0", "10.0", "11.0" };

            return new[] { ";(none)" }
                .Concat(items.Select(x => $"{x};CMS {x}"));
        }


        private void Control_Init(object sender, EventArgs e)
        {
            if (Control.Form != null)
            {
                Control.Form.OnItemValidation += Form_OnItemValidation;
            }
        }


        private void Form_OnItemValidation(object sender, ref string errorMessage)
        {
            if ((sender == Control) && String.IsNullOrEmpty(Convert.ToString(Control.Value)))
            {
                errorMessage = ResHelper.GetString("general.requirescmsversion");
            }
        }
    }
}
