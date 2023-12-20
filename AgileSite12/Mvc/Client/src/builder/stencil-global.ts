/**
 * Provides shared functionality for Stencil components.
 * https://stenciljs.com/docs/context/
 */

import { LocalizationService } from "@/builder/declarations";

declare var Context: any;

Context.getString = (localizationService: LocalizationService, key: string, ...parameters: any[]): string => {
  return localizationService ? localizationService.getLocalization(key, parameters) : "";
};

/**
 * Ensures correct styles for given markup. If current browser does not support Shadow DOM, the method injects the given data attribute to all elements in a given markup.
 * Otherwise, the markup is returned unchanged.
 * @param markup Markup where the attribute should be injected.
 * @param attributeName Name of the data attribute.
 * @returns Markup of elements including the injected attribute.
 */
const ensureStyles = (markup: string, attributeName: string): string => {
  const supportsShadowDOMv1 = !!HTMLElement.prototype.attachShadow;

  if (supportsShadowDOMv1) {
    return markup;
  }

  const temporaryElement = document.createElement("div");
  temporaryElement.innerHTML = markup;
  const allElements = temporaryElement.querySelectorAll(":not(script):not(link):not(style)");
  Array.prototype.forEach.call(allElements, (element) => element.setAttribute(`data-${attributeName}`, ""));

  return temporaryElement.innerHTML;
};

Context.ensureStyles = ensureStyles;
