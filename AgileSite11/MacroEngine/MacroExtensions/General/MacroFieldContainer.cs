using System.Collections.Generic;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Container for field extensions of an arbitrary object used by MacroEngine.
    /// </summary>
    public class MacroFieldContainer : MacroExtensionContainer<MacroFieldContainer, MacroField>
    {
        #region "Methods"
        
        /// <summary>
        /// Returns list of macro field extension registered for specified object.
        /// Returns null if there is no such extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        public static IEnumerable<MacroField> GetFieldsForObject(object obj)
        {
            return GetExtensionsForObject(obj);
        }


        /// <summary>
        /// Returns macro field object of given name if registered for specified object.
        /// It loops through all MacroFieldContainer extensions of given object type.
        /// Returns null if there is no such Extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="name">Name of the field</param>
        public static MacroField GetFieldForObject(object obj, string name)
        {
            return GetExtensionForObject(obj, name);
        }


        /// <summary>
        /// Returns a field of given name (return null if specified field does not exist).
        /// </summary>
        /// <param name="name">Field name</param>
        public MacroField GetField(string name)
        {
            return GetExtension(name);
        }


        /// <summary>
        /// Registers the given field.
        /// </summary>
        /// <param name="field">Field to register</param>
        public void RegisterField(MacroField field)
        {
            RegisterExtension(field);
        }


        /// <summary>
        /// Registers all the fields.
        /// </summary>
        protected virtual void RegisterFields()
        {
            // Do nothing by default
        }


        /// <summary>
        /// Registers all the fields.
        /// </summary>
        protected override void RegisterExtensions()
        {
            base.RegisterExtensions();

            RegisterFields();
        }

        #endregion
    }
}