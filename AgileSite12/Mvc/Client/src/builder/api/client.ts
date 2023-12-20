/**
 * Configures and provides the HTTP client.
 * @module api/client
 */

import axios, { AxiosInstance } from "axios";

import { BuilderConfig } from "@/builder/builderConfig";

let http: AxiosInstance;
let builderConfig: BuilderConfig;

/**
 * Initializes the HTTP client.
 * @param configuration Page builder configuration.
 */
const initHttpClient = (configuration: BuilderConfig): void => {
  http = axios.create({});
  builderConfig = configuration;
};

export {
  http,
  initHttpClient,
  builderConfig,
};
