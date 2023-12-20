import { injectable } from "inversify";
import { denormalize, normalize } from "normalizr";

import { areasSchema, metadataSchema } from "@/builder/api/schemas";
import { Metadata, NormalizedConfiguration, NormalizerService, SectionMetadata, WidgetMetadata } from "@/builder/declarations";
import { objectHelper } from "@/builder/helpers/index";

import { DenormalizedFormConfiguration, FormComponentMetadata } from "@/form-builder/declarations";

@injectable()
export class FormBuilderNormalizerService implements NormalizerService {
  /**
   * Normalizes Form builder's configuration received from the server to configuration with which the Builder can work.
   * @param configuration Configuration sent from the server.
   */
  public normalizeConfiguration(configuration: DenormalizedFormConfiguration): NormalizedConfiguration {
    this.renameZoneConfiguration(configuration, "formComponents", "widgets");

    const normalizedConfiguration = normalize(configuration, areasSchema).entities;
    this.mergePropertiesFromVariantsToComponents(configuration, normalizedConfiguration);

    const emptyNormalizedConfiguration: NormalizedConfiguration = {
      editableAreas: {},
      sections: {},
      zones: {},
      widgets: {},
      widgetVariants: {},
    };

    return objectHelper.isEmpty(normalizedConfiguration) ? emptyNormalizedConfiguration : normalizedConfiguration;
  }

  /**
   * Denormalizes Builder's configuration to be sent to the server.
   * @param configuration Builder's configuration to be denormalized.
   */
  public denormalizeConfiguration(configuration: NormalizedConfiguration): DenormalizedFormConfiguration {
    const denormalized = denormalize({
      editableAreas: Object.keys(configuration.editableAreas),
    }, areasSchema, configuration);

    this.renameZoneConfiguration(denormalized, "widgets", "formComponents");
    this.mergePropertiesFromComponentsToVariants(configuration, denormalized);

    return denormalized;
  }

  /**
   * Normalizes metadata sent from server's Form builder to fit builder.
   * @param metadata Denormalized Form builder's metadata.
   */
  public normalizeMetadata(metadata: { formComponents: FormComponentMetadata[], sections: SectionMetadata[] }): Metadata {
    const widgets: WidgetMetadata[] = metadata.formComponents.map((formComponentMetadata) => {
      const {
        identifier,
        // tslint:disable-next-line:trailing-comma
        ...noIdentifierProperty
      } = formComponentMetadata;

      return {
        ...noIdentifierProperty,
        hasEditableProperties: false,
        hasProperties: true,
        propertiesFormMarkupUrl: null,
        typeIdentifier: formComponentMetadata.identifier,
      };
    });

    const normalizedMetadata = normalize({ sections: metadata.sections, widgets }, metadataSchema).entities;
    return {
      widgets: normalizedMetadata.widgets || {},
      sections: normalizedMetadata.sections || {},
      pageTemplates: {},
      personalizationConditionTypes: {},
    };
  }

  /**
   * Takes object representing configuration of the Form builder and renames property of the Zone configuration.
   */
  private renameZoneConfiguration(configuration, originalName, newName) {
    for (const editableArea of configuration.editableAreas) {
      for (const section of editableArea.sections) {
        for (const zone of section.zones) {
          zone[newName] = zone[originalName];
          delete zone[originalName];
        }
      }
    }
  }

  private mergePropertiesFromComponentsToVariants(configuration: NormalizedConfiguration, denormalizedConfiguration) {
    for (const editableArea of denormalizedConfiguration.editableAreas) {
      for (const section of editableArea.sections) {
        for (const zone of section.zones) {
          for (const component of zone.formComponents) {
            component.properties = configuration.widgetVariants[component.variants[0].identifier].properties;
            delete component.variants;
          }
        }
      }
    }
  }

  private mergePropertiesFromVariantsToComponents(configuration, normalizedConfiguration) {
    for (const editableArea of configuration.editableAreas) {
      for (const section of editableArea.sections) {
        for (const zone of section.zones) {
          for (const widget of zone.widgets) {
            normalizedConfiguration.widgetVariants = normalizedConfiguration.widgetVariants || {};
            normalizedConfiguration.widgetVariants[widget.identifier] = {
              identifier: widget.identifier,
              name: widget.identifier,
              properties: widget.properties,
            };
            normalizedConfiguration.widgets[widget.identifier].variants = normalizedConfiguration.widgets[widget.identifier].variants || [];
            Object.assign(normalizedConfiguration.widgets[widget.identifier].variants, [widget.identifier]);
            delete normalizedConfiguration.widgets[widget.identifier].properties;
          }
        }
      }
    }
  }
}
