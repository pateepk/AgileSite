import { ConfigurationEndpoints } from "@/builder/declarations/api";

/**
 * Provides interface for builder constants.
 */
export interface BuilderConstants {
  readonly editingInstanceHeader: string;
}

/**
 * Provides interface for selectors' configuration sent from server.
 */
export interface SelectorsConfig {
  readonly dialogEndpoints: DialogEndpointsConfig;
}

/**
 * Provides interface for selectors' configuration sent from server.
 */
export interface DialogEndpointsConfig {
  readonly mediaFilesSelector: string;
  readonly pageSelector: string;
}

/**
 * Provides interface for general configuration options sent from server to configure builder.
 */
export interface BuilderOptions {
  readonly applicationPath: string;
  readonly configurationEndpoints: ConfigurationEndpoints;
  readonly metadataEndpoint: string;
  readonly allowedOrigins: string[];
  readonly constants: BuilderConstants;
  readonly developmentMode: boolean;
  readonly selectors: SelectorsConfig;
}
