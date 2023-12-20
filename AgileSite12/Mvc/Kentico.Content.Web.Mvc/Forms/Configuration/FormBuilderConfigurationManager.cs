using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc.Internal;
using Kentico.Forms.Web.Mvc.Sections;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Manages the state of a form builder configuration.
    /// </summary>
    internal class FormBuilderConfigurationManager : IFormBuilderConfigurationManager
    {
        private readonly IInvalidComponentConfigurationBuilder mInvalidComponentConfigurationBuilder;
        private readonly IFormComponentPropertiesMapper mFormComponentPropertiesMapper;
        private readonly ISectionDefinitionProvider mSectionDefinitionProvider;
        private readonly IFormBuilderConfigurationSerializer mFormBuilderConfigurationSerializer;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormBuilderConfigurationManager"/> class.
        /// </summary>
        /// <param name="invalidComponentConfigurationBuilder">Builder for invalid component configuration.</param>
        /// <param name="formComponentPropertiesMapper">Mapper for form component properties.</param>
        /// <param name="sectionDefinitionProvider"></param>
        /// <param name="formBuilderConfigurationSerializer">Serializer to be used for configuration (de)serialization.</param>
        public FormBuilderConfigurationManager(IInvalidComponentConfigurationBuilder invalidComponentConfigurationBuilder, IFormComponentPropertiesMapper formComponentPropertiesMapper,
                                               ISectionDefinitionProvider sectionDefinitionProvider, IFormBuilderConfigurationSerializer formBuilderConfigurationSerializer = null)
        {
            mInvalidComponentConfigurationBuilder = invalidComponentConfigurationBuilder ?? throw new ArgumentNullException(nameof(invalidComponentConfigurationBuilder));
            mFormComponentPropertiesMapper = formComponentPropertiesMapper ?? throw new ArgumentNullException(nameof(formComponentPropertiesMapper));
            mSectionDefinitionProvider = sectionDefinitionProvider ?? throw new ArgumentNullException(nameof(sectionDefinitionProvider));
            mFormBuilderConfigurationSerializer = formBuilderConfigurationSerializer ?? Service.Resolve<IFormBuilderConfigurationSerializer>();
        }


        /// <summary>
        /// Loads Form builder configuration from the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to load.</param>
        /// <returns>Configuration of Form builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> is null.</exception>
        public FormBuilderConfiguration Load(BizFormInfo bizFormInfo)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }

            var formComponents = LoadFormComponents(bizFormInfo);

            return CreateWrappingFormBuilderConfiguration(formComponents, bizFormInfo.FormBuilderLayout);
        }


        /// <summary>
        /// Loads Form builder configuration from the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to load.</param>
        /// <returns>Returns configuration of form's components.</returns>
        protected internal virtual List<FormComponentConfiguration> LoadFormComponents(BizFormInfo bizFormInfo)
        {
            DataClassInfo dataClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            FormInfo formInfo = new FormInfo(dataClassInfo.ClassFormDefinition);

            var formComponentsConfiguration = new List<FormComponentConfiguration>();

            foreach (var formField in formInfo.GetFields(true, true).Where(ffi => !ffi.PrimaryKey && !ffi.System && !ffi.IsDummyField))
            {
                var formComponentConfiguration = CreateFormComponentConfiguration(formField);

                formComponentsConfiguration.Add(formComponentConfiguration);
            }

            return formComponentsConfiguration;
        }


        /// <summary>
        /// Stores Form builder configuration to the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to save.</param>
        /// <param name="configurationJson">JSON configuration of Form builder.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bizFormInfo"/> or <paramref name="configurationJson"/> is null.</exception>
        public void Store(BizFormInfo bizFormInfo, string configurationJson)
        {
            if (bizFormInfo == null)
            {
                throw new ArgumentNullException(nameof(bizFormInfo));
            }
            if (String.IsNullOrEmpty(configurationJson))
            {
                throw new ArgumentNullException(nameof(configurationJson));
            }

            var configuration = mFormBuilderConfigurationSerializer.Deserialize(configurationJson);

            var formComponents = SelectFormComponents(configuration);

            if (ContainsDuplicateComponentNames(formComponents))
            {
                throw new InvalidOperationException("Duplicate form component names found.");
            }

            if (BizFormInfoProvider.LicenseVersionCheck(SiteContext.CurrentSite.DomainName, FeatureEnum.BizForms, ObjectActionEnum.Edit))
            {
                StoreFormComponents(bizFormInfo, formComponents);
                StoreFormLayout(bizFormInfo, configuration);
            }
        }


        /// <summary>
        /// Stores Form builder configuration to the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Biz form whose configuration to save.</param>
        /// <param name="formComponentsConfiguration">Configuration of form's components.</param>
        protected internal virtual void StoreFormComponents(BizFormInfo bizFormInfo, List<FormComponentConfiguration> formComponentsConfiguration)
        {
            DataClassInfo originalDataClassInfo = DataClassInfoProvider.GetDataClassInfo(bizFormInfo.FormClassID);
            FormInfo originalFormInfo = new FormInfo(originalDataClassInfo.ClassFormDefinition);

            DataClassInfo newDataClassInfo = originalDataClassInfo.Clone();
            FormInfo newFormInfo = new FormInfo();

            foreach (var originalFormField in originalFormInfo.GetFields(true, true).Where(ffi => ffi.PrimaryKey || ffi.System || ffi.IsDummyField))
            {
                var formFieldInfo = (FormFieldInfo)originalFormField.Clone();

                newFormInfo.AddFormItem(formFieldInfo);
            }

            foreach (var formComponentConfiguration in formComponentsConfiguration)
            {
                if (formComponentConfiguration.IsInvalidComponent())
                {
                    // The invalid component placeholder is replaced with original corrupted component before persisting it's form,
                    // so that no configuration is inadvertently lost during any autosave
                    var originalFormFieldInfo = originalFormInfo.GetFormField(formComponentConfiguration.Properties.Guid);

                    newFormInfo.AddFormItem(originalFormFieldInfo);
                }
                else
                {
                    RemoveInvalidDependentValidationRuleConfigurations(formComponentConfiguration, formComponentsConfiguration);

                    var formFieldInfo = CreateFormFieldInfo(formComponentConfiguration);
                    newFormInfo.AddFormItem(formFieldInfo);
                }
            }

            EnsureSystemContactField(newFormInfo);
            var newDefinition = newFormInfo.GetXmlDefinition();

            newDataClassInfo.ClassFormDefinition = newDefinition;
            DataClassInfoProvider.SetDataClassInfo(newDataClassInfo);
        }


        private void RemoveInvalidDependentValidationRuleConfigurations(FormComponentConfiguration componentConfiguration, List<FormComponentConfiguration> componentsConfiguration)
        {
            var dependsOnRuleConfigurations = componentConfiguration.Properties.ValidationRuleConfigurations
                .Where(vrc => vrc.ValidationRule.GetType().FindTypeByGenericDefinition(typeof(CompareToFieldValidationRule<>)) != null)
                .ToList();

            foreach (var ruleConfiguration in dependsOnRuleConfigurations)
            {
                var dependentRule = (ICompareToFieldValidationRule)ruleConfiguration.ValidationRule;
                var dependeeComponent = componentsConfiguration.Find(c => c.Properties.Guid.Equals(dependentRule.DependeeFieldGuid));

                if (dependeeComponent == null)
                {
                    componentConfiguration.Properties.ValidationRuleConfigurations.Remove(ruleConfiguration);
                }
            }
        }


        /// <summary>
        /// Creates a new form field for a form component configuration.
        /// </summary>
        /// <param name="formComponentConfiguration">Form component configuration for which to create a form field.</param>
        /// <returns>Returns a new form field representing <paramref name="formComponentConfiguration"/>.</returns>
        protected virtual FormFieldInfo CreateFormFieldInfo(FormComponentConfiguration formComponentConfiguration)
        {
            var properties = formComponentConfiguration.Properties;

            return mFormComponentPropertiesMapper.ToFormFieldInfo(properties, formComponentConfiguration.TypeIdentifier);
        }


        /// <summary>
        /// Stores form builder layout to the actual form.
        /// </summary>
        /// <param name="bizFormInfo">Form whose layout to save.</param>
        /// <param name="configuration">Configuration of the form.</param>
        private void StoreFormLayout(BizFormInfo bizFormInfo, FormBuilderConfiguration configuration)
        {
            // When configuration is saved with unknown section in it, then replace the unknown section with the default one
            ReplaceUnknownSections(configuration);

            bizFormInfo.FormBuilderLayout = mFormBuilderConfigurationSerializer.Serialize(configuration, true);

            BizFormInfoProvider.SetBizFormInfo(bizFormInfo);
        }


        /// <summary>
        /// Replaces unknown sections with the default section.
        /// </summary>
        /// <returns>Configuration of Form builder.</returns>
        private void ReplaceUnknownSections(FormBuilderConfiguration configuration)
        {
            foreach (var section in configuration.EditableAreas.First().Sections)
            {
                if (section.TypeIdentifier == KenticoDefaultFormSectionController.IDENTIFIER_UNKNOWN)
                {
                    section.TypeIdentifier = KenticoDefaultFormSectionController.IDENTIFIER;                    
                }
            }
        }


        protected virtual FormComponentConfiguration CreateFormComponentConfiguration(FormFieldInfo formFieldInfo)
        {
            FormComponentConfiguration formComponentConfiguration = null;

            try
            {
                formComponentConfiguration = new FormComponentConfiguration
                {
                    TypeIdentifier = mFormComponentPropertiesMapper.GetComponentIdentifier(formFieldInfo),
                    Properties = mFormComponentPropertiesMapper.FromFieldInfo(formFieldInfo)
                };
            }
            catch (Exception ex)
            {
                var message = ResHelper.GetString("kentico.formbuilder.error.invalidcomponentconfiguration");
                formComponentConfiguration = mInvalidComponentConfigurationBuilder.CreateInvalidFormComponentConfiguration(formFieldInfo, message, ex);
            }

            return formComponentConfiguration;
        }


        /// <summary>
        /// Ensures existence of systemic contact field in given <paramref name="formInfo"/> when it contains at least one smart field.
        /// </summary>
        internal void EnsureSystemContactField(FormInfo formInfo)
        {
            if (!formInfo.ContainsSmartField())
            {
                return;
            }

            if (!SmartFieldLicenseHelper.HasLicense())
            {
                return;
            }

            if (formInfo.FieldExists(SmartFieldConstants.CONTACT_COLUMN_NAME))
            {
                return;
            }

            var definition = new FormFieldInfo
            {
                AllowEmpty = true,
                DataType = FieldDataType.Guid,
                Name = SmartFieldConstants.CONTACT_COLUMN_NAME,
                PrimaryKey = false,
                System = true,
                PublicField = false,
                Visible = false,
                Caption = "Contact Guid"
            };

            formInfo.AddFormItem(definition);
        }


        private FormBuilderConfiguration CreateWrappingFormBuilderConfiguration(List<FormComponentConfiguration> formComponentsConfiguration, string layout = null)
        {
            var layoutConfiguration = string.IsNullOrEmpty(layout) ? GetDefaultLayoutConfiguration() : mFormBuilderConfigurationSerializer.Deserialize(layout);

            // When configuration is stored in the databse with the section that is no longer registered in the system
            // then mark all those section so that the client can be notified about using an unregistered section
            MarkUnknownSections(layoutConfiguration);

            AddComponentsToLayout(layoutConfiguration, formComponentsConfiguration);

            return layoutConfiguration;
        }


        /// <summary>
        /// Marks unregistered sections by setting their <see cref="SectionConfiguration.TypeIdentifier"/> to
        /// <see cref="KenticoDefaultFormSectionController.IDENTIFIER_UNKNOWN"/> and merges all components from original zones into one zone.
        /// </summary>
        /// <returns>Configuration of Form builder.</returns>
        private void MarkUnknownSections(FormBuilderConfiguration configuration)
        {
            foreach (var sectionConfiguration in configuration.EditableAreas.First().Sections)
            {
                if (mSectionDefinitionProvider.Get(sectionConfiguration.TypeIdentifier) == null)
                {
                    var joinedZone = new ZoneConfiguration()
                    {
                        Identifier = sectionConfiguration.Zones.First().Identifier
                    };
                    joinedZone.FormComponents.AddRange(sectionConfiguration.Zones.SelectMany(z => z.FormComponents));

                    sectionConfiguration.Zones.Clear();
                    sectionConfiguration.Zones.Add(joinedZone);
                    
                    EventLogProvider.LogWarning(FormBuilderConstants.EVENT_LOG_SOURCE, "FORMSECTION", null, SiteContext.CurrentSiteID,
                        $"Form section '{sectionConfiguration.TypeIdentifier}' was not found. Verify that it is registered.");

                    sectionConfiguration.TypeIdentifier = KenticoDefaultFormSectionController.IDENTIFIER_UNKNOWN;
                }
            }
        }


        private FormBuilderConfiguration GetDefaultLayoutConfiguration()
        {
            return new FormBuilderConfiguration
            {
                EditableAreas =
                {
                    new EditableAreaConfiguration
                    {
                        Sections =
                        {
                            new SectionConfiguration
                            {
                                Zones =
                                {
                                    new ZoneConfiguration
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }


        private void AddComponentsToLayout(FormBuilderConfiguration layoutConfiguration, List<FormComponentConfiguration> formComponentsConfiguration)
        {
            List<FormComponentConfiguration> notFoundComponents = new List<FormComponentConfiguration>();

            var layoutComponents = SelectFormComponents(layoutConfiguration);
            foreach (var component in formComponentsConfiguration)
            {
                var componentPlaceholder = layoutComponents.FirstOrDefault(c => c.Identifier.Equals(component.Identifier));
                if (componentPlaceholder != null)
                {
                    // Replace component placeholder in layout with real component
                    componentPlaceholder.TypeIdentifier = component.TypeIdentifier;
                    componentPlaceholder.Properties = component.Properties;
                }
                else
                {
                    notFoundComponents.Add(component);
                }
            }

            if (notFoundComponents.Count > 0)
            {
                // Add components which were not found in the layout to the last zone
                SelectZoneConfigurations(layoutConfiguration).Last().FormComponents.AddRange(notFoundComponents);
            }
        }


        private IEnumerable<ZoneConfiguration> SelectZoneConfigurations(FormBuilderConfiguration configuration)
        {
            return configuration.EditableAreas.First().Sections.SelectMany(s => s.Zones);
        }


        private List<FormComponentConfiguration> SelectFormComponents(FormBuilderConfiguration configuration)
        {
            return SelectZoneConfigurations(configuration).SelectMany(z => z.FormComponents).ToList();
        }


        private static bool ContainsDuplicateComponentNames(List<FormComponentConfiguration> formComponents)
        {
            return formComponents
                .GroupBy(component => component.Properties.Name)
                .Any(group => group.Count() > 1);
        }
    }
}
