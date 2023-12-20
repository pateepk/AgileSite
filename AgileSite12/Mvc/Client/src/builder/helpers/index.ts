import Vue from "vue";
import { SECTION_MOUNTED_EVENT, SECTION_REMOVE_EVENT } from "../constants";

import { PopupType } from "../store/types";
import { Theme } from "../types";

const objectHelper = {

  /**
   * Checks if passed object is empty.
   * @param obj Object to check if it is empty.
   * @returns boolean True if the object has no properties, false otherwise.
   */
  isEmpty(obj: object): boolean {
    // because Object.keys(new Date()).length === 0;
    // we have to do some additional check
    return obj.constructor === Object && Object.keys(obj).length === 0;
  },

  /**
   * Copies the values of all enumerable own properties from one or more source objects to a target object while ignoring properties whose values are 'undefined'.
   * @param target Target object.
   * @param sources One or more source objects.
   * @returns Updated target object.
   */
  assignDefined<T extends object>(target: T, ...sources: object[]): T {
    for (const source of sources) {
      for (const key of Object.keys(source)) {
        const val = source[key];
        if (val !== undefined) {
          target[key] = val;
        }
      }
    }

    return target;
  },
};

const stringHelper = {

  /**
   * Inserts arguments into the provided string according to the positional placeholders.
   * @param stringToFormat String in which the parameters should be inserted.
   * @param parameters Parameters that should be inserted into the provided string.
   * @returns string Formatted string.
   */
  format(stringToFormat: string, ...parameters: any[]): string {
    let formatedString = stringToFormat;
    parameters.forEach((param, index) => {
      formatedString = formatedString.replace(new RegExp("\\{" + index + "\\}", "g"), param);
    });

    return formatedString;
  },
};

const arrayHelper = {

  /**
   * Creates a new array with items from original array and inserts an element into the new array at the specified position.
   * @param array The original array with items.
   * @param item The object to insert.
   * @param position The zero-based index at which item should be inserted.
   * @returns A new array with item at position.
   */
  insertItemIntoArray<T>(array: T[], item: T, position: number): T[] {
    if (position > array.length || position < 0) {
      return array;
    }

    return [
      ...array.slice(0, position),
      item,
      ...array.slice(position),
    ];
  },

  /**
   * Creates a new array with items from the original array and replaces an item in the new array on the specified position.
   * @param array The original source array.
   * @param item The item to be placed instead of another.
   * @param position The position of the item which should be replaced.
   * @returns A new array with the item at the specified position.
   */
  replaceItem: <T>(array: T[], item: T, position: number) => {
    if (!array.length || position > array.length || position < 0) {
      return array;
    }

    return [
      ...array.slice(0, position),
      item,
      ...array.slice(position + 1),
    ];
  },

  /**
   * Creates a new array with items from the original array except the one at the specified position.
   * @param array The original source array.
   * @param position The position of the item which should be omitted.
   * @returns A new array with the item at the specified position omitted.
   */
  removeItem: <T>(array: T[], position: number) => {
    if (position > array.length || position < 0) {
      return array;
    }

    return [
      ...array.slice(0, position),
      ...array.slice(position + 1),
    ];
  },

  /**
   * Moves item from source array to destination one at specific position.
   * If destination array is null move item only in source array and return null for destination array.
   * @param sourceArray Source array - the item will be removed from here.
   * @param destinationArray Destination array - the item will be added here.
   * @param item Item to move.
   * @param position The zero-based index at which item should be inserted.
   * @returns Two new arrays after move action was performed.
   */
  moveItemBetweenArrays: <T>(sourceArray: T[], item: T, position: number, destinationArray: T[] = null): {
    sourceArray: T[],
    destinationArray: T[],
  } => {
    const maxAllowedPosition = (destinationArray === null) ? sourceArray.length - 1 : destinationArray.length;

    if (position < 0 || (position > maxAllowedPosition)) {
      throw new RangeError("The [position] argument is invalid.");
    }

    if (!arrayHelper.contains(sourceArray, item)) {
      throw new Error("The [item] was not found in [sourceArray].");
    }

    const sourceArrayItemRemoved = sourceArray.filter((value) => value !== item);
    if (destinationArray === null) {
      return {
        sourceArray: arrayHelper.insertItemIntoArray(sourceArrayItemRemoved, item, position),
        destinationArray: null,
      };
    }

    return {
      sourceArray: sourceArrayItemRemoved,
      destinationArray: arrayHelper.insertItemIntoArray(destinationArray, item, position),
    };
  },

  /**
   * Returns an array consisting of unique values taken from the source array.
   * @param array Source array.
   * @param mapItemFn (Optional) Mapping function to get the value in case of array of complex types.
   * @returns Array of unique values.
   */
  getUniqueValues: <T>(array: T[], mapItemFn: (item: T, index: number, wholeArray: T[]) => T = (item) => item): T[] => {
    return [...new Set(array.map(mapItemFn))];
  },

  /**
   * Determines whether an element is in the Array<T>.
   * @param array Source array.
   * @param value The value to seek.
   * @returns True if item is found in the Array<T>; otherwise, false.
   */
  contains: <T>(array: T[], value: T): boolean => {
    return array.indexOf(value) >= 0;
  },

  /**
   * Moves element inside an array to specified position.
   * @param array Array.
   * @param from Index of the original element position.
   * @param to Index where the element should be moved.
   * @return The array with moved element.
   */
  move: <T>(array: T[], from: number, to: number): T[] => {
    array.splice(to, 0, array.splice(from, 1)[0]);
    return array;
  },
};

const eventHelper = {
  /**
   * Dispatches an event on the document object.
   * @param eventName Event name/type.
   * @param eventData Custom event data, stored under 'detail' key.
   */
  dispatchEvent: (eventName: string, eventData: object) => {
    const event = new CustomEvent(eventName, {
      detail: eventData,
    });
    document.dispatchEvent(event);
  },
};

const dragAndDropHelper = {
  /**
   * Remove section from drag and drop.
   * @param sectionIdentifier Identifier of the section to be removed from DnD.
   */
  removeSectionFromDragAndDrop: (sectionIdentifier: string) => {
    eventHelper.dispatchEvent(SECTION_REMOVE_EVENT, { sectionIdentifier });
  },

  /**
   * Mount section to drag and drop after the next DOM update cycle.
   * @param sectionIdentifier Identifier of the section to be added to DnD.
   * @param component Vue component.
   */
  mountSectionToDragAndDropAfterNextTick: (component: Vue, sectionIdentifier: string) => {
    component.$nextTick(() => {
      dragAndDropHelper.mountSectionToDragAndDrop(sectionIdentifier);
    });
  },

  /**
   * Mount section to drag and drop.
   * @param sectionIdentifier Identifier of the section to be added to DnD.
   */
  mountSectionToDragAndDrop: (sectionIdentifier: string) => {
    eventHelper.dispatchEvent(SECTION_MOUNTED_EVENT, { sectionIdentifier });
  },
};

const linkHelper = {
  /**
   * Disables all links in document.
   */
  disableDocumentLinks: () => {
    const linkElements = document.getElementsByTagName("a");
    const elementsCount = linkElements.length;

    for (let count = 0; count < elementsCount; count++) {
      linkElements[count].onclick = () => false;
    }
  },

  /**
   * Disables links in element that are not within inline editor.
   * @param element Element for which the links should be disabled.
   */
  disableElementLinks: (element: HTMLElement) => {
    const linkElements = element.getElementsByTagName("a");
    const elementsCount = linkElements.length;

    let linkElement: HTMLElement;

    for (let count = 0; count < elementsCount; count++) {
      linkElement = linkElements[count];
      if (!linkElement.closest("div[data-inline-editor]")) {
        linkElement.onclick = () => false;
      }
    }
  },
};

const componentHelper = {
  /**
   * Returns component color theme according to the popup type.
   * @param popupType Popup type.
   */
  getTheme: (popupType: PopupType): Theme => {
    switch (popupType) {
      case PopupType.AddWidget:
      case PopupType.Personalization:
        return Theme.Widget;

      case PopupType.AddSection:
      case PopupType.ChangeSection:
        return Theme.Section;
    }
  },
};

export {
  objectHelper,
  stringHelper,
  arrayHelper,
  eventHelper,
  linkHelper,
  dragAndDropHelper,
  componentHelper,
};
