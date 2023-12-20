import { BuilderConfig } from "@/builder/BuilderConfig";
import { getService } from "@/builder/container";
import { BuilderConstants, ConfigurationEndpoints, LocalizationService } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

/**
 * Configuration object for form builder.
 */
export class FormBuilderConfig extends BuilderConfig {
  constructor(readonly formIdentifier: number, readonly propertiesEditorClientId: string, readonly propertiesEditorEndpoint: string,
              readonly validationRuleMetadataEndpoint: string, readonly validationRuleMarkupEndpoint, readonly visibilityConditionMarkupEndpoint: string,
              readonly saveMessageClientId: string, applicationPath: string,
              configurationEndpoints: ConfigurationEndpoints, metadataEndpoint: string, allowedOrigins: string[],
              constants: BuilderConstants) {
    super(applicationPath, configurationEndpoints, metadataEndpoint, allowedOrigins, constants, null);
  }

  public validate() {
    if (this.formIdentifier === 0) {
      throw new Error(getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("notinitialized"));
    }
  }

  public getComponentMarkupData(postedData: object): object {
    return {
      postedData,
      formIdentifier: this.formIdentifier,
    };
  }
}
