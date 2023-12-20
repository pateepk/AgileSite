import { builderConfig, http } from "@/builder/api/client";
import { logger } from "@/builder/logger";

import { RENDER_MARKUP_EVENT } from "@/form-builder/constants";
import { FormComponent } from "@/form-builder/declarations/entities";
import { FormBuilderConfig } from "@/form-builder/FormBuilderConfig";

/**
 * Gets editor markup for a form component.
 * @param formComponentState State of the component representing current configuration.
 */
const getPropertiesEditorMarkup = async (formComponentState: FormComponent): Promise<any> => {
  const endpoint = (builderConfig as FormBuilderConfig).propertiesEditorEndpoint;

  return performApiCall(async () => {
    const response = await http.post(endpoint, formComponentState);
    return response.data;
  });
};

/**
 * Retrieves all validation rules registered in the system.
 */
const getValidationRulesMetadata = async (): Promise<any> => {
  const endpoint = (builderConfig as FormBuilderConfig).validationRuleMetadataEndpoint;

  return performApiCall(async () => {
    const response = await http.get(endpoint);
    return response.data;
  });
};

/**
 * Retrieves markup to configure validation rule.
 * @param selectedFormComponentIdentifier Instance identifier of the currently selected form component.
 * @param validationRuleTypeIdentifier Identifier of the registered validation rule.
 * @param validationRule Validation rule instance for which the markup should be retrieved.
 * @param fieldName Name of form field the validation rule belongs to.
 */
const getValidationRuleMetadataMarkup = async (selectedFormComponentIdentifier: string, validationRuleTypeIdentifier: string,
                                               validationRule: any, fieldName: string): Promise<any> => {
  const endpoint = (builderConfig as FormBuilderConfig).validationRuleMarkupEndpoint;
  const data = {
      formComponentInstanceIdentifier: selectedFormComponentIdentifier,
      formFieldName: fieldName,
      validationRule: JSON.stringify(validationRule),
      validationRuleIdentifier: validationRuleTypeIdentifier,
    };

  return performApiCall(async () => {
    const response = await http.post(endpoint, data);
    return response.data;
  });
};

/**
 * Retrieves markup to configure visibility condition.
 * @param selectedFormComponentIdentifier Instance identifier of the currently selected form component.
 * @param fieldName Name of form field the visibility condition belongs to.
 */
const getVisibilityConditionMarkup = async (selectedFormComponentIdentifier: string, fieldName: string): Promise<any> => {
  const endpoint = (builderConfig as FormBuilderConfig).visibilityConditionMarkupEndpoint;
  const data = {
    formComponentInstanceIdentifier: selectedFormComponentIdentifier,
    formFieldName: fieldName,
  };

  return performApiCall(async () => {
    const response = await http.post(endpoint, data);
    return response.data;
  });
};

/**
 * Dispatches event for event listener to insert a markup to the target element.
 * Currently, the markup is inserted via jQuery.html() so that script tags inside the markup get executed.
 * @param targetElementId Id of the element where the markup should be inserted.
 * @param markup Markup to render by event listener.
 */
const renderMarkup = (targetElementId: string, markup: string) => {
  document.dispatchEvent(
    new CustomEvent(RENDER_MARKUP_EVENT, {
      detail: {
        markup,
        targetElementId,
      },
    }),
  );
};

/**
 * Performs passed function and handles all the errors thrown by the function.
 * @param apiCall Function requesting data from the server.
 */
const performApiCall = async (apiCall: () => Promise<any>): Promise<any> => {
  try {
    return await apiCall();
  } catch (err) {
    logger.logException(err);
    throw(err);
  }
};

export {
  getPropertiesEditorMarkup,
  getValidationRulesMetadata,
  getValidationRuleMetadataMarkup,
  getVisibilityConditionMarkup,
  renderMarkup,
  performApiCall,
};
