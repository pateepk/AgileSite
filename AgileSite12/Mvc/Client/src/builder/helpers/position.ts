import { POP_UP_CONTAINER_WIDTH } from "@/builder/constants";
import { ComponentPosition, ListingOffset } from "@/builder/store/types";
import { FORM_BUILDER_FORM_ELEMENT_CLASS } from "@/form-builder/constants";

/**
 * Gets position of the pop up container based on element location.
 * @param containerParentElement element containing the pop-up-container.
 * @param expectedWidth A number that is used to determine position if container's width can't be calculated from markup. Default value is half of POP_UP_CONTAINER_WIDTH constant.
 * @returns Component position.
 */
const getPosition = (parentClientRect: ClientRect, expectedWidth: number = POP_UP_CONTAINER_WIDTH / 2): ComponentPosition => {
  const offset = getOffset(parentClientRect);
  const boundaryWidth = getContainerWidth(expectedWidth);

  if (offset.left < boundaryWidth) {
    return ComponentPosition.Right;
  }
  if (offset.right < boundaryWidth) {
    return ComponentPosition.Left;
  }
  return ComponentPosition.Center;
};

/**
 * Calculates pop-up container width.
 * @param expectedWidth Expected width of the container.
 */
const getContainerWidth = (expectedWidth: number): number => {
  const container = document.body.querySelector("kentico-pop-up-container");

  if (!container || !container.offsetWidth) {
    return expectedWidth;
  }

  return container.offsetWidth / 2;
};

/**
 * Calculates needed offset from given parent client bounding rectangle.
 * @param clientRect Client bounding rectangle of the element.
 */
const getOffset = (clientRect: DOMRect | ClientRect) => {
  return {
    left: clientRect.left,
    right: window.innerWidth - clientRect.right,
  };
};

/**
 * Calculates pop-up listing offset for both page builder and form builder.
 * @param clientRect Client rectangle of add button or any other pop-up's "parent" element.
 */
const calculateListingOffset = (clientRect: ClientRect): ListingOffset => {
  // In form builder the wrapper is not a child of body but a child of a form builder wrapper element
  // therefore the scroll Y offset has to be read from the form builder wrapper element
  // In page builder the scroll Y offset is read from the document itself
  const formBuilderWrapper = document.body.querySelector(`.${FORM_BUILDER_FORM_ELEMENT_CLASS}`);
  const scrollYOffset = (formBuilderWrapper && !isNaN(formBuilderWrapper.scrollTop))
                        ? formBuilderWrapper.scrollTop
                        : pageYOffset;

  // The same as for scroll Y offset applies for scroll X offset
  const scrollXOffset = (formBuilderWrapper && !isNaN(formBuilderWrapper.scrollLeft))
                        ? formBuilderWrapper.scrollLeft
                        : pageXOffset;

  return {
    // Left boundary of add button + half of its' width + scroll X offset of the parent element
    left: clientRect.left + (clientRect.width / 2) + scrollXOffset,
    // Top boundary of add button + height of the button + height of the connecting triangle + scroll Y offset of the parent element
    top: clientRect.top + clientRect.height + 10  + scrollYOffset,
  };
};

export {
  getPosition,
  calculateListingOffset,
};
