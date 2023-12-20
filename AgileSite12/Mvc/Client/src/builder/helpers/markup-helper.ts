import v4 from "uuid/v4";

import { logger } from "@/builder/logger/index";

// tslint:disable-next-line:interface-over-type-literal
type ScriptMappings = {
  [scriptIdentifier: string]: HTMLScriptElement,
};

// tslint:disable-next-line:interface-over-type-literal
type StrippedMarkupResult = {
  strippedMarkup: string,
  scriptMappings: ScriptMappings,
};

// DOM Element class used for temporary scripts which are to be removed from DOM after an action.
const TMP_ELEMENT_CLASS = "ktc-tmp-element";

// Data attribute used to identify section script placeholders
const SECTION_SCRIPT_PLACEHOLDER_DATA_ATTRIBUTE = "data-kentico-script-id";

/**
 * Renders a given textual markup into given element. Scripts contained in the markup are executed as well.
 * @param markup Markup of the sub-tree.
 * @param targetElement Parent element of the sub-tree created from the markup.
 * @param externalScriptsIdentifier Identifier of external script tags.
 */
const renderMarkup = (markup: string, targetElement: HTMLElement, externalScriptsIdentifier: string): void => {
  targetElement.innerHTML = markup;
  const scripts = targetElement.querySelectorAll("script");

  Array.prototype.forEach.call(scripts, (scriptElement: HTMLScriptElement) => {
    const parent = scriptElement.parentNode;
    const temp = document.createElement("script");

    try {
      // Execute external scripts
      if (scriptElement.src) {
        temp.classList.add(TMP_ELEMENT_CLASS);
        temp.classList.add(externalScriptsIdentifier);
        temp.src = scriptElement.src;
        temp.type = scriptElement.type;
        document.head.appendChild(temp);
      } else {
        // Execute inline scripts
        temp.innerHTML = scriptElement.innerHTML;
        parent.replaceChild(temp, scriptElement);
      }

      scriptElement.remove();

    } catch (err) {
      logger.logException(err);
    }
  });
};

/**
 * Removes temporary script elements from the head element.
 * @param externalScriptsIdentifier Identifier of script tags which should be removed.
 */
const removeScriptElements = (externalScriptsIdentifier: string) => {
  const scripts = document.head.querySelectorAll(`.${externalScriptsIdentifier}`);
  Array.prototype.forEach.call(scripts, (scriptElement: HTMLScriptElement) => {
    scriptElement.remove();
  });
};

/**
 * Replaces script elements with script placeholders.
 * @param markup HTML markup to be stripped of script elements.
 * @returns Stripped markup with mappings of script placeholder identifiers to the original script elements.
 */
const stripScriptElements = (markup: string): StrippedMarkupResult => {
  const scripts: { [scriptIdentifier: string]: HTMLScriptElement } = {};

  const section = document.createElement("div");
  section.innerHTML = markup;
  const scriptsInSection = section.querySelectorAll("script");

  Array.prototype.forEach.call(scriptsInSection, (script: HTMLScriptElement) => {
    const scriptIdentifier = v4();

    // Create a placeholder element in place of the original script element
    const scriptPlaceholder = document.createElement("div");
    scriptPlaceholder.style.display = "none";
    scriptPlaceholder.dataset.kenticoScriptId = scriptIdentifier;

    // Need to create a copy, the original script element doesn't invoke when attached to DOM...
    const scriptCopy = document.createElement("script");
    scriptCopy.innerHTML = script.innerHTML;

    // Need to check if src and type attributes are empty, otherwise 'unknown' value is pass to the attribute and the script won't run
    if (script.src) {
      scriptCopy.src = script.src;
    }

    if (script.type) {
      scriptCopy.type = script.type;
    }

    scripts[scriptIdentifier] = scriptCopy;

    script.parentNode.replaceChild(scriptPlaceholder, script);
  });

  return {
    strippedMarkup: section.innerHTML,
    scriptMappings: scripts,
  };
};

/**
 * Replaces script placeholders with corresponding script elements.
 * @param elementWithScriptPlaceholders Element in which script placeholders should be replaced with corresponding script elements.
 * @param scriptMappings Mappings of script placeholder identifiers to the corresponding script elements.
 */
const replacePlaceholdersWithScripts = (elementWithScriptPlaceholders: HTMLElement, scriptMappings: ScriptMappings) => {
  const scriptPlaceholders = elementWithScriptPlaceholders.querySelectorAll(`div[${SECTION_SCRIPT_PLACEHOLDER_DATA_ATTRIBUTE}]`);

  Array.prototype.forEach.call(scriptPlaceholders, (scriptPlaceholder: HTMLElement) => {
    const script = scriptMappings[scriptPlaceholder.dataset.kenticoScriptId];
    scriptPlaceholder.parentNode.replaceChild(script, scriptPlaceholder);
  });
};

/**
 * Clamps a text to specified number of lines. The element needs to have the line-height style set.
 * @param {HTMLElement} element. Element containing the text to clamp.
 */
const lineClamp = (element: HTMLElement, lines: number) => {
  const originalText = element.innerHTML;
  const truncationChar = "…";
  const splitOnChars = [".", "-", "–", "—", " ", "_"].slice(0);

  let splitChar = splitOnChars[0];
  let chunks;
  let lastChunk;

  const truncate = (target, maxHeight) => {
    const text = originalText.replace(truncationChar, "");

    // Grab the next chunks
    if (!chunks) {
      // If there are more characters to try, grab the next one
      if (splitOnChars.length > 0) {
        splitChar = splitOnChars.shift();
      } else {
        splitChar = "";
      }

      chunks = text.split(splitChar);
    }

    // If there are chunks left to remove, remove the last one and see if the text fits
    if (chunks.length > 1) {
      lastChunk = chunks.pop();
      applyEllipsis(target, chunks.join(splitChar));
    } else {
      chunks = null;
    }

    // Search produced valid chunks
    if (chunks) {
      // It fits
      if (element.clientHeight <= maxHeight) {
        // There's still more characters to try splitting on, not quite done yet
        if (splitChar !== "") {
          applyEllipsis(target, chunks.join(splitChar) + splitChar + lastChunk);
          chunks = null;
        } else {
          return;
        }
      }
    }

    truncate(target, maxHeight);
  };

  function applyEllipsis(elem, str) {
    elem.innerHTML = str + truncationChar;
  }

  const height = parseInt(getComputedStyle(element).getPropertyValue("line-height"), 10) * lines;
  if (height < element.clientHeight) {
    truncate(element, height);
  }
};

export {
  renderMarkup,
  removeScriptElements,
  stripScriptElements,
  replacePlaceholdersWithScripts,
  lineClamp,
};
