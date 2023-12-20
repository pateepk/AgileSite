export interface FeatureSet {
  readonly personalizationEnabled: boolean;
}

export interface PageTemplateConfig {
  readonly selectorDialogEndpoint: string;
  readonly changeTemplateEndpoint: string;
  readonly isSelectable: boolean;
}
