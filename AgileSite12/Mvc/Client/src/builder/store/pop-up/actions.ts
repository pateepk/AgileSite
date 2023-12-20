import { createAction } from "typesafe-actions";

import { ComponentPosition, ListingOffset, PopupType } from "../types";

/**
 * Opens popup dialog.
 * @param componentIdentifier Identifier of a component for which popup dialog should be opened.
 * @param position Position where the popup should be displayed.
 * @param popupType Type of the popup dialog.
 * @param listingOffset Pop-up listing's offset.
 * @param areaIdentifier Editable area identifier.
 * @param zoneIdentifier Widget zone identifier.
 */
export const openPopup = createAction("popup/OPEN", (resolve) =>
  (componentIdentifier: string, position: ComponentPosition, popupType: PopupType, listingOffset: ListingOffset = null,
   areaIdentifier: string = null, zoneIdentifier: string = null) => resolve({
      componentIdentifier,
      position,
      popupType,
      listingOffset,
      areaIdentifier,
      zoneIdentifier,
    }),
);

/**
 * Closes popup dialog.
 */
export const closePopup = createAction("popup/CLOSE");

/**
 * Sets the popup dialog position.
 * @param position Position of the popup dialog.
 * @param listingOffset Pop-up listing's offset.
 */
export const setPopupPosition = createAction("popup/SET_POSITION", (resolve) =>
  (position: ComponentPosition, listingOffset: ListingOffset) => resolve({
    position,
    listingOffset,
  }),
);
