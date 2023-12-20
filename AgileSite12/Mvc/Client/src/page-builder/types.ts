import { BuilderOptions } from "@/builder/declarations/config";
import { FeatureSet, PageTemplateConfig } from "@/page-builder/declarations/config";

/**
 * Provides interface for page builder options sent from server.
 */
export interface PageBuilderOptions extends BuilderOptions {
  readonly pageIdentifier: number;
  readonly featureSet: FeatureSet;
  readonly pageTemplate: PageTemplateConfig;
}

export enum ButtonIcon {
  Properties = "icon-cogwheel",
  Change = "icon-l-cols-30-70",
}
