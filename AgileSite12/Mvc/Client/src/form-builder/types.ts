import { BuilderOptions } from "@/builder/declarations/config";

/**
 * Provides interface for form builder options sent from server.
 */
interface FormBuilderOptions extends BuilderOptions {
  readonly formIdentifier: number;
  readonly propertiesEditorClientId: string;
  readonly propertiesEditorEndpoint: string;
  readonly validationRuleMetadataEndpoint: string;
  readonly validationRuleMarkupEndpoint: string;
  readonly visibilityConditionMarkupEndpoint: string;
  readonly saveMessageClientId: string;
}

export {
  FormBuilderOptions,
};
