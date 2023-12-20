import { BuilderConfiguration, BuilderConstants, ConfigurationEndpoints, SelectorsConfig } from "@/builder/declarations";

/**
 * Represents general configuration for a builder object.
 */
export abstract class BuilderConfig implements BuilderConfiguration {
  constructor(readonly applicationPath: string, readonly configurationEndpoints: ConfigurationEndpoints,
              readonly metadataEndpoint: string, readonly allowedOrigins: string[], readonly constants: BuilderConstants,
              readonly selectors: SelectorsConfig) {
  }

  /**
   * Validates the builder configuration.
   * @throws Throws an error when validation fails.
   */
  public abstract validate(): void;
  public abstract getComponentMarkupData(postedData: object): object;
}
