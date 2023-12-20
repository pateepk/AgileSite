/**
 * Name of event for signaling the Form builder to freeze the UI as currently executed action must end without any other disturbances.
 */
export const FORM_BUILDER_FREEZE_UI = "kenticoFormBuilderFreezeUI";

/**
 * Name of event for signaling the Form builder to unfreeze the UI.
 */
export const FORM_BUILDER_UNFREEZE_UI = "kenticoFormBuilderUnfreezeUI";

/**
 * Name of event for signaling the properties editor form was processed by the server and a new state is available.
 * The data of the event contain an identifier and the new state.
 */
export const PROPERTIES_STATE_CHANGED_EVENT = "kenticoPropertiesStateChanged";

/**
 * Name of event for signaling that validation rule configuration was processed by the server and a new state is available.
 * The data of the event contain an identifier and the new state.
 */
export const VALIDATION_RULE_STATE_CHANGED_EVENT = "kenticoValidationRuleStateChanged";

/**
 * Name of event for signaling that validation rule configuration was closed.
 */
export const VALIDATION_RULE_CONFIGURATION_CLOSED = "kenticoFormBuilderCloseExpandedValidationRuleConfiguration";

/**
 * Name of event for signaling that visibility condition configuration was processed by the server and a new state is available.
 * The data of the event contain an identifier and the new state.
 */
export const VISIBILITY_CONDITION_STATE_CHANGED_EVENT = "kenticoVisibilityConditionStateChanged";

/**
 * Name of event for signaling the properties editor form was processed by the server and a new state is available.
 * The data of the event contain an identifier and the new state.
 */
export const RENDER_MARKUP_EVENT = "kenticoRenderMarkup";

/**
 * Name of the default invalid component.
 */
export const INVALID_COMPONENT_TYPE_NAME = "Kentico.Invalid";

/**
 * Name of the section used as replacement for a unknown section.
 */
export const UNKNOWN_SECTION_TYPE_NAME = "Kentico.UnknownSection";

/**
 * Name of the class of the wrapping mvc form element of the form builder.
 */
export const FORM_BUILDER_FORM_ELEMENT_CLASS = "ktc-form-builder-mvc-form";
