import { createAction } from "typesafe-actions";

import { ComponentPosition } from "../types";

/**
 * 'Show section header' action creator
 * @param sectionIdentifier Identifier of the section to be highlighted.
 * @param headerPosition Position where the header should be displayed.
 */
export const showSectionHeader = createAction("highlightedSection/SHOW_SECTION_HEADER", (resolve) =>
  (sectionIdentifier: string, headerPosition: ComponentPosition) => resolve({
    sectionIdentifier,
    headerPosition,
  }),
);

/**
 * 'Hide section header' action creator
 */
export const hideSectionHeader = createAction("highlightedSection/HIDE_SECTION_HEADER");

/**
 * 'Highlight section border' action creator
 */
export const highlightSectionBorder = createAction("highlightedSection/HIGHLIGHT_SECTION_BORDER", (resolve) =>
  (sectionIdentifier: string = null) => resolve({
    sectionIdentifier,
  }),
);

/**
 * 'Dehighlight section border' action creator
 */
export const dehighlightSectionBorder = createAction("highlightedSection/DEHIGHLIGHT_SECTION_BORDER");
