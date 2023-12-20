/**
 * Provides page configuration combined with default state.
 * @module page-configuration
 */

import { injectable } from "inversify";

import * as api from "@/builder/api";
import { BuilderConfiguration, State } from "@/builder/declarations/store";

import {
  removeExcessZones,
  removePersonalizationOfMissingConditionTypes,
  removeSectionsWithMissingType,
  removeWidgetsWithMissingType,
} from "./configuration-remover";
import {
  addDefaultSections,
  addEditableAreas,
  addMissingZones,
  ensureDefaultTemplateProperties,
  ensureDisplayedWidgetVariants,
  filterPageConfigurations,
  loadAreaConfigurations,
  loadComponentsMetadata,
  loadSectionMarkups,
  loadWidgetVariantMarkups,
  notifyAboutSectionsWithoutZones,
  prepareDefaultConfiguration,
} from "./provider";

/**
 * Returns configuration for the specified page.
 */
@injectable()
class PageConfigurationService {
  async getConfiguration(config: BuilderConfiguration): Promise<State> {
    const pageConfigurationFromDOM = prepareDefaultConfiguration();
    const configurationFromServer = await api.getState();

    let pageConfiguration = filterPageConfigurations(Object.keys(pageConfigurationFromDOM.editableAreas), configurationFromServer);
    pageConfiguration = addEditableAreas(pageConfigurationFromDOM.editableAreas, pageConfiguration);

    let state: any = {
      config:  {
        ...config,
      },
      editableAreasConfiguration: loadAreaConfigurations(),
      ...pageConfiguration,
      markups: {
        sections: {},
        variants: {},
      },
      metadata: await loadComponentsMetadata(),
    };

    state = removeSectionsWithMissingType(state);
    state = removeWidgetsWithMissingType(state);
    state = removePersonalizationOfMissingConditionTypes(state);

    await loadWidgetVariantMarkups(state);
    await loadSectionMarkups(state);

    await addDefaultSections(state);
    notifyAboutSectionsWithoutZones(state);
    state = removeExcessZones(state);
    addMissingZones(state);
    state = ensureDisplayedWidgetVariants(state);
    state = ensureDefaultTemplateProperties(state);

    return state;
  }
}

export {
  PageConfigurationService,
};
