import { ThunkAction } from "@/builder/declarations";
import { calculateListingOffset } from "@/builder/helpers/position";

import { ComponentPosition, PopupType } from "../types";
import { openPopup as openPopupAction } from "./actions";

export const openPopup = (componentIdentifier: string, position: ComponentPosition, popupType: PopupType, parentClientRect: ClientRect,
                          areaIdentifier: string = null, zoneIdentifier: string = null): ThunkAction =>
  (dispatch) => {
    const listingOffset = calculateListingOffset(parentClientRect);
    dispatch(openPopupAction(componentIdentifier, position, popupType, listingOffset, areaIdentifier, zoneIdentifier));
  };
