/**
 * Responsible for communication with the API.
 * @module api
 */

import { builderConfig, http } from "@/builder/api/client";
import { getService } from "@/builder/container";
import { Metadata, NormalizedConfiguration, NormalizerService, State } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

import { configuration, metadata } from "./endpoints";

/**
 * Requests default properties on the specified URL.
 * @param componentDefaultPropertiesUrl Component default properties URL.
 */
const getComponentDefaultProperties = async (componentDefaultPropertiesUrl: string): Promise<object> => {
  const response = await http.get(componentDefaultPropertiesUrl);

  return response.data;
};

/**
 * Requests markup for a component on the specified URL.
 * @param componentMarkupUrl URL for component markup retrieval.
 * @param properties Component properties.
 */
const getComponentMarkup = async (componentMarkupUrl: string, properties: object): Promise<string> => {
  const data = builderConfig.getComponentMarkupData(properties);
  const response = await http.post(componentMarkupUrl, data);

  return response.data;
};

/**
 * Requests metadata for all components.
 */
const getComponentMetadata = async (): Promise<Metadata> => {
  const response = await metadata.get();

  return getService<NormalizerService>(SERVICE_TYPES.NormalizerService).normalizeMetadata(response.data);
};

/**
 * Requests state for the specified page.
 */
const getState = async (): Promise<NormalizedConfiguration> => {
  const response = await configuration.load();

  return getService<NormalizerService>(SERVICE_TYPES.NormalizerService).normalizeConfiguration(response.data);
};

/**
 * Sends a request to save the page's state.
 * @param state State of the page.
 * @param guid Page GUID.
 */
const saveState = async (state: State, guid: string): Promise<any> => {
  const denormalizedState = getService<NormalizerService>(SERVICE_TYPES.NormalizerService).denormalizeConfiguration(state);

  return configuration.save(denormalizedState, guid);
};

export {
  getComponentDefaultProperties,
  getComponentMarkup,
  getComponentMetadata,
  getState,
  saveState,
};
