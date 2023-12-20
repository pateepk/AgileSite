import { BuilderConfig } from "@/builder/BuilderConfig";
import { getService } from "@/builder/container";
import { BuilderConstants, ConfigurationEndpoints, LocalizationService, SelectorsConfig } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

import { FeatureSet, PageTemplateConfig } from "@/page-builder/declarations/config";

/**
 * Configuration object for page builder.
 */
export class PageBuilderConfig extends BuilderConfig {
  constructor(
    readonly pageIdentifier: number,
    readonly applicationPath: string,
    readonly configurationEndpoints: ConfigurationEndpoints,
    readonly metadataEndpoint: string,
    readonly allowedOrigins: string[],
    readonly constants: BuilderConstants,
    readonly featureSet: FeatureSet,
    readonly pageTemplate: PageTemplateConfig,
    readonly selectors: SelectorsConfig,
  ) {
    super(applicationPath, configurationEndpoints, metadataEndpoint, allowedOrigins, constants, selectors);
  }

  public validate() {
    if (this.pageIdentifier === 0) {
      throw new Error(getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("notinitialized"));
    }
  }

  public getComponentMarkupData(postedData: object): object {
    return {
      postedData,
      pageIdentifier: this.pageIdentifier,
    };
  }
}
