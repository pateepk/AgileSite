using System;

namespace CMS.UIControls
{
    /// <summary>
    /// Registers controls for personal data collection and erasure.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class DataProtectionControlsRegister
    {
        private static Lazy<DataProtectionControlsRegister> mInstance = new Lazy<DataProtectionControlsRegister>(() => new DataProtectionControlsRegister());

        private volatile string dataSubjectIdentifiersFilterControlVirtualPath = "~/CMSModules/DataProtection/Controls/DefaultDataSubjectIdentifiersFilter.ascx";
        private volatile string erasureControlVirtualPath = "~/CMSModules/DataProtection/Controls/DefaultErasureConfiguration.ascx";


        /// <summary>
        /// Gets the <see cref="DataProtectionControlsRegister"/> instance.
        /// </summary>
        public static DataProtectionControlsRegister Instance => mInstance.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectionControlsRegister"/> class.
        /// </summary>
        internal DataProtectionControlsRegister()
        {
        }


        /// <summary>
        /// Registers control identified by its <paramref name="controlVirtualPath"/> as a control to be used when specifying the data subject identifiers.
        /// The control must inherit the <see cref="DataSubjectIdentifiersFilterControl"/> class.
        /// </summary>
        /// <param name="controlVirtualPath">Virtual path of the control.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controlVirtualPath"/> is null.</exception>
        /// <seealso cref="GetDataSubjectIdentifiersFilterControl"/>
        /// <seealso cref="DataSubjectIdentifiersFilterControl"/>
        public void RegisterDataSubjectIdentifiersFilterControl(string controlVirtualPath)
        {
            if (controlVirtualPath == null)
            {
                throw new ArgumentNullException(nameof(controlVirtualPath));
            }

            dataSubjectIdentifiersFilterControlVirtualPath = controlVirtualPath;
        }


        /// <summary>
        /// Registers control identified by its <paramref name="controlVirtualPath"/> as a control to be used when configuring the personal data erasure process.
        /// The control must inherit the <see cref="ErasureConfigurationControl"/> class.
        /// </summary>
        /// <param name="controlVirtualPath">Virtual path of the control.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controlVirtualPath"/> is null.</exception>
        /// <seealso cref="GetErasureConfigurationControl"/>
        /// <seealso cref="ErasureConfigurationControl"/>
        public void RegisterErasureConfigurationControl(string controlVirtualPath)
        {
            if (controlVirtualPath == null)
            {
                throw new ArgumentNullException(nameof(controlVirtualPath));
            }

            erasureControlVirtualPath = controlVirtualPath;
        }


        /// <summary>
        /// Gets virtual path of control registered for specifying the data subject identifiers. The control inherits the <see cref="DataSubjectIdentifiersFilterControl"/> class.
        /// </summary>
        /// <returns>Virtual path of the control.</returns>
        /// <remarks>
        /// The system default value is the path <c>~/CMSModules/DataProtection/Controls/DefaultDataSubjectIdentifiersFilter.ascx</c>.
        /// </remarks>
        /// <seealso cref="RegisterDataSubjectIdentifiersFilterControl"/>
        /// <seealso cref="DataSubjectIdentifiersFilterControl"/>
        public string GetDataSubjectIdentifiersFilterControl()
        {
            return dataSubjectIdentifiersFilterControlVirtualPath;
        }


        /// <summary>
        /// Gets virtual path of control registered for configuring the personal data erasure process. The control inherits the <see cref="ErasureConfigurationControl"/> class.
        /// </summary>
        /// <returns>Virtual path of the control.</returns>
        /// <remarks>
        /// The system default value is the path <c>~/CMSModules/DataProtection/Controls/DefaultErasureConfiguration.ascx</c>.
        /// </remarks>
        /// <seealso cref="RegisterErasureConfigurationControl"/>
        /// <seealso cref="ErasureConfigurationControl"/>
        public string GetErasureConfigurationControl()
        {
            return erasureControlVirtualPath;
        }
    }
}
