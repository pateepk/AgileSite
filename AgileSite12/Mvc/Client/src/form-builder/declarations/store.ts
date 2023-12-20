import { Action } from "redux";

import { BuilderConfiguration, ThunkAction } from "@/builder/declarations";
import { FormBuilderAction, FormBuilderState } from "@/form-builder/store/types";

/**
 * Form builder configuration state.
 */
export interface FormBuilderConfiguration extends BuilderConfiguration {
  readonly formIdentifier: number;
  readonly propertiesEditorClientId: string;
  readonly propertiesEditorEndpoint: string;
  readonly validationRuleMetadataEndpoint: string;
  readonly validationRuleMarkupEndpoint: string;
  readonly visibilityConditionMarkupEndpoint: string;
  readonly saveMessageClientId: string;
}

/**
 * Defines an interface for validation rule metadata sent from the server.
 */
export interface ValidationRuleMetadata {
  readonly identifier: string;
  readonly name: string;
  readonly description: string;
}

/**
 * Defines an interface for validation rule configuration sent from the server.
 */
export interface ValidationRuleConfiguration {
  readonly identifier: string;
  readonly validationRule: any;
}

/**
 * Defines an interface for validation rule with its metadata.
 */
export interface ValidationRuleWithMetadata {
  readonly metadata: ValidationRuleMetadata;
  readonly validationRule: any;
}

/**
 * Defines an interface for binding widget variant as well as validation rule.
 */
export interface WidgetVariantToValidationRuleBinding {
  readonly widgetVariantIdentifier: string;
  readonly validationRuleIdentifier: string;
}

/**
 * Defines an interface for form component metadata sent from the server.
 */
export interface FormComponentMetadata {
  readonly identifier: string;
  readonly defaultPropertiesUrl: string;
  readonly markupUrl: string;
  readonly name: string;
  readonly description: string;
  readonly iconClass: string;
}

/**
 * Represents a Form builder thunk action.
 */
export interface ThunkAction<R = void, S = FormBuilderState, A extends Action = FormBuilderAction>
  extends ThunkAction<R, S, A> {
}

export {
  FormBuilderState,
};
