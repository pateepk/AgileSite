/**
 * Provides methods available on the configuration endpoint.
 * @module api/endpoints/configuration
 */

import { DenormalizedEditableAreasConfiguration, DenormalizedPageBuilderConfiguration } from "@/builder/declarations/api";
import { builderConfig, http } from "../client";

/**
 * Loads the page configuration from server.
 */
const load = (): Promise<any> => {
  return http.get(builderConfig.configurationEndpoints.load);
};

/**
 * Sends a request to save the page's current configuration.
 * @param denormalizedPageConfiguration Denormalized page configuration.
 */
const save = (denormalizedPageConfiguration: DenormalizedEditableAreasConfiguration | DenormalizedPageBuilderConfiguration, guid: string): Promise<any> => {
  const headers = { [builderConfig.constants.editingInstanceHeader] : guid };
  return http.post(builderConfig.configurationEndpoints.store, denormalizedPageConfiguration, { headers });
};

export {
  load,
  save,
};
