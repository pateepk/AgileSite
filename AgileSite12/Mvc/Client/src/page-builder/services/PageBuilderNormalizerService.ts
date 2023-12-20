/**
 * Handles data (de)normalization.
 * @module api/normalizer
 */

import { injectable } from "inversify";
import { denormalize, normalize } from "normalizr";

import { metadataSchema, pageSchema } from "@/builder/api/schemas";
import {
  DenormalizedPageBuilderConfiguration,
  Metadata,
  NormalizedConfiguration,
  NormalizerService,
  PageTemplateMetadata,
  PersonalizationConditionTypeMetadata,
  SectionMetadata,
  WidgetMetadata,
} from "@/builder/declarations";
import { objectHelper } from "@/builder/helpers/index";

@injectable()
export class PageBuilderNormalizerService implements NormalizerService {
  /**
   * Normalizes page configuration received from the server.
   * @param configuration Denormalized page configuration.
   */
  public normalizeConfiguration(configuration: DenormalizedPageBuilderConfiguration): NormalizedConfiguration {
    const normalizedConfiguration = normalize(configuration, pageSchema).entities;
    const emptyNormalizedConfiguration: NormalizedConfiguration = {
      editableAreas: {},
      sections: {},
      zones: {},
      widgets: {},
      widgetVariants: {},
    };

    const pageBuilderConfiguration = objectHelper.isEmpty(normalizedConfiguration) ? emptyNormalizedConfiguration : normalizedConfiguration;
    pageBuilderConfiguration.pageTemplate = configuration.pageTemplate;

    return pageBuilderConfiguration;
  }

  /**
   * Denormalizes page configuration.
   * @param configuration Normalized page configuration.
   */
  public denormalizeConfiguration(configuration: NormalizedConfiguration): DenormalizedPageBuilderConfiguration {
    return denormalize({
        page: {
          editableAreas: Object.keys(configuration.editableAreas),
        },
        pageTemplate: configuration.pageTemplate,
      }, pageSchema, configuration);
  }

  /**
   * Normalizes widgets metadata received from the server.
   * @param metadata Denormalized widgets metadata.
   */
  public normalizeMetadata(metadata: { widgets: WidgetMetadata[], sections: SectionMetadata[], pageTemplates: PageTemplateMetadata[],
    personalizationConditionTypes: PersonalizationConditionTypeMetadata[] }): Metadata {
    const normalizedMetadata = normalize(metadata, metadataSchema).entities;

    return {
      widgets: normalizedMetadata.widgets || {},
      sections: normalizedMetadata.sections || {},
      personalizationConditionTypes: normalizedMetadata.personalizationConditionTypes || {},
      pageTemplates: normalizedMetadata.pageTemplates || {},
    };
  }
}
