using System;

using CMS.Base;
using CMS.Helpers;
using CMS.WebFarmSync;

namespace CMS.UIControls
{
    /// <summary>
    /// Query log base class.
    /// </summary>
    public class WebFarmLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return WebFarmDebug.Settings;
            }
        }
        

        /// <summary>
        /// Gets the data of the task
        /// </summary>
        /// <param name="textData">Text data</param>
        /// <param name="binaryData">Binary data</param>
        protected string GetData(object textData, object binaryData)
        {
            string text = ValidationHelper.GetString(textData, null);
            string binary = ValidationHelper.GetString(binaryData, null);

            if (String.IsNullOrEmpty(text))
            {
                if (!String.IsNullOrEmpty(binary))
                {
                    return "byte[]: " + binary;
                }
            }
            else
            {
                string result = HTMLHelper.EnsureHtmlLineEndings(text);

                if (!String.IsNullOrEmpty(binary))
                {
                    result += " (byte[]: " + binary + ")";
                }

                return result;
            }

            return "";
        }

        #endregion
    }
}