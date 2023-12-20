/**
 * Provides methods available on the metadata endpoint.
 * @module api/endpoints/metadata
 */

import { builderConfig, http } from "../client";

/**
 * Requests metadata for all widgets.
 */
const get = (): Promise<any> => {
  return http.get(builderConfig.metadataEndpoint);
};

export {
  get,
};
