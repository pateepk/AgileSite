using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.CKEditor.Web.UI
{
    /// <summary>
    /// CKEditor configuration class.
    /// </summary>
    [Serializable]
    public class CKEditorConfiguration : ISerializable
    {
        #region "Variables"

        private readonly Hashtable mConfigs;

        #endregion


        #region "Properties"

        /// <summary>
        /// Configuration indexer.
        /// </summary>
        public string this[string configurationName]
        {
	        get
	        {
		        if (mConfigs.ContainsKey(configurationName))
		        {
			        return (string)mConfigs[configurationName];
		        }
		        else
		        {
			        return null;
		        }
	        }
	        set
            {
                mConfigs[configurationName] = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CKEditor configurations.
        /// </summary>
        internal CKEditorConfiguration()
        {
            mConfigs = new Hashtable();
        }


        /// <summary>
        /// Constructor - Creates CKEditor configurations from serialized info.
        /// </summary>
        protected CKEditorConfiguration(SerializationInfo info, StreamingContext context)
        {
            mConfigs = (Hashtable)info.GetValue("ConfigTable", typeof(Hashtable));
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Removes configuration entry.
        /// </summary>
        /// <param name="configurationName">Name of configuration entry</param>
        public bool Remove(string configurationName)
        {
            if (mConfigs.ContainsKey(configurationName))
            {
                mConfigs.Remove(configurationName);
            }
            else
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Returns javascript configuration array.
        /// </summary>
        internal string GetJsConfigArray()
        {
            StringBuilder osParams = new StringBuilder();

            foreach (DictionaryEntry oEntry in mConfigs)
            {
                if (oEntry.Value == null)
                {
                    continue;
                }

                if (osParams.Length > 0)
                {
                    osParams.Append(",");
                }

                string value = (oEntry.Value != null ? oEntry.Value.ToString() : "null");
                if (oEntry.Key.ToString().ToLowerCSafe() == "templatesxmlpath")
                {
                    value = UrlResolver.ResolveUrl(value);
                }

                if (value.ToLowerCSafe() == "null")
                {
                    // Ensure null settings
                    osParams.AppendFormat("{0}: {1}\n", oEntry.Key, value.ToLowerCSafe());
                }
                else
                {
                    if (ValidationHelper.IsBoolean(value))
                    {
                        osParams.AppendFormat("{0}: {1}\n", oEntry.Key, value.ToLowerCSafe());
                    }
                    else
                    {
                        if (ValidationHelper.IsDouble(value))
                        {
                            osParams.AppendFormat("{0}: {1}\n", oEntry.Key, value);
                        }
                        else
                        {
                            // Skip object and array definitions
                            if ((value.StartsWithCSafe("[") && value.EndsWithCSafe("]")) || (value.StartsWithCSafe("{") && value.EndsWithCSafe("}")))
                            {
                                osParams.AppendFormat("{0}: {1}\n", oEntry.Key, value);
                            }
                            else if (IsSpecialKey(oEntry.Key))
                            {
                                osParams.AppendFormat("{0}: {1}\n", oEntry.Key, value);
                            }
                            else
                            {
                                // Escape strings
                                osParams.AppendFormat("{0}: {1}\n", oEntry.Key, ScriptHelper.GetString(value.Trim('\'')));
                            }
                        }
                    }
                }
            }

            return osParams.ToString();
        }


        private static bool IsSpecialKey(object key)
        {
            var stringKey = key.ToString();
            return stringKey.Equals(nameof(CKEditorControl.EnterMode), StringComparison.OrdinalIgnoreCase) || stringKey.Equals(nameof(CKEditorControl.ShiftEnterMode), StringComparison.OrdinalIgnoreCase);
        }

        #endregion


        #region "ISerializable Members"

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ConfigTable", mConfigs);
        }

        #endregion
    }
}