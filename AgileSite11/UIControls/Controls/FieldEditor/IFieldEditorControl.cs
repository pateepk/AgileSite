using System;
using System.Collections;

namespace CMS.UIControls
{
    /// <summary>
    /// Used for loading / updating configuration of the control from the field editor parameters.
    /// </summary>
    public interface IFieldEditorControl
    {
        /// <summary>
        /// Sets inner controls according to the parameters and their values included in configuration collection. Parameters collection will be passed from Field editor.
        /// </summary>
        /// <param name="config">Parameters collection</param>
        void LoadConfiguration(Hashtable config);


        /// <summary>
        /// Updates parameters collection of parameters and values according to the values of the inner controls. Parameters collection which should be updated will be passed from Field editor.
        /// </summary>
        /// <param name="config">Parameters collection</param>
        void UpdateConfiguration(Hashtable config);


        /// <summary>
        /// Validates inner controls. If some error occurs appropriate error message is returned, otherwise empty string should be returned;.
        /// </summary>        
        string Validate();


        /// <summary>
        /// Clears inner controls / sets them to the default values.
        /// </summary>
        void ClearControl();
    }
}