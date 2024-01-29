using CMS;
using CMS.DataEngine;
using CMS.DataProtection;

[assembly: RegisterModule(typeof(DataProtection.DataProtectionModule))]

namespace DataProtection
{
    /// <summary>
    /// Represents module providing data protection functionality.
    /// </summary>
    public class DataProtectionModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectionModule"/> class.
        /// </summary>
        public DataProtectionModule() : base("DataProtectionModule")
        {
        }


        /// <summary>
        /// Initializes the data protection module.
        /// </summary>
        protected override void OnInit()
        {
            IdentityCollectorRegister.Instance.Add(new SampleContactInfoIdentityCollector());
            IdentityCollectorRegister.Instance.Add(new SampleCustomerInfoIdentityCollector());

            PersonalDataCollectorRegister.Instance.Add(new SampleContactDataCollector());
            PersonalDataCollectorRegister.Instance.Add(new SampleCustomerDataCollector());

            PersonalDataEraserRegister.Instance.Add(new SampleContactPersonalDataEraser());
            PersonalDataEraserRegister.Instance.Add(new SampleCustomerPersonalDataEraser());
        }
    }
}
