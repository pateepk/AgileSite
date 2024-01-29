using CMS.DataEngine.Generators;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data engine code template generator.
    /// </summary>
    public class DataEngineCodeTemplateGenerator
    {
        /// <summary>
        /// Gets the template for the info class code generation for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static InfoTemplate GetInfoCodeTemplate(DataClassInfo dataClass)
        {
            var template = new InfoTemplate(dataClass);
            return template;
        }


        /// <summary>
        /// Generates the default info class code for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static string GetInfoCode(DataClassInfo dataClass)
        {
            var code = GetInfoCodeTemplate(dataClass).TransformText();
            return code;
        }


        /// <summary>
        /// Gets the template for the info provider class code generation for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static InfoProviderTemplate GetInfoProviderCodeTemplate(DataClassInfo dataClass)
        {
            var template = new InfoProviderTemplate(dataClass);
            return template;
        }


        /// <summary>
        /// Generates the default info provider class code for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static string GetInfoProviderCode(DataClassInfo dataClass)
        {
            var code = GetInfoProviderCodeTemplate(dataClass).TransformText();
            return code;
        }


        /// <summary>
        /// Gets the template for the binding info class code generation for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static BindingInfoTemplate GetBindingInfoCodeTemplate(DataClassInfo dataClass)
        {
            var template = new BindingInfoTemplate(dataClass);
            return template;
        }


        /// <summary>
        /// Generates the default binding info class code for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static string GetBindingInfoCode(DataClassInfo dataClass)
        {
            var code = GetBindingInfoCodeTemplate(dataClass).TransformText();
            return code;
        }


        /// <summary>
        /// Gets the template for the binding info provider class code generation for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static BindingInfoProviderTemplate GetBindingInfoProviderCodeTemplate(DataClassInfo dataClass)
        {
            var template = new BindingInfoProviderTemplate(dataClass);
            return template;
        }


        /// <summary>
        /// Generates the default binding info provider class code for the specified data class.
        /// </summary>
        /// <param name="dataClass">Data class</param>
        public static string GetBindingInfoProviderCode(DataClassInfo dataClass)
        {
            var code = GetBindingInfoProviderCodeTemplate(dataClass).TransformText();
            return code;
        }
    }
}